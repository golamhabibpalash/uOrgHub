import { useEffect, useState } from 'react';
import { Plus, Shield, Check } from 'lucide-react';
import { getRoles, getClaims, addRoleClaim, removeRoleClaim, type RoleDto, type ClaimDto } from '../../api/auth';

export default function RolesPage() {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [claims, setClaims] = useState<ClaimDto[]>([]);
  const [selected, setSelected] = useState<RoleDto | null>(null);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    const [r, c] = await Promise.all([getRoles(), getClaims()]);
    setRoles(r);
    setClaims(c);
    if (r.length > 0) setSelected(r[0]);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const toggleClaim = async (claim: ClaimDto) => {
    if (!selected) return;
    const has = selected.claims.some(c => c.id === claim.id);
    if (selected.isSystem && !has) {
      await addRoleClaim(selected.id, claim.id);
    } else if (!selected.isSystem) {
      if (has) await removeRoleClaim(selected.id, claim.id);
      else await addRoleClaim(selected.id, claim.id);
    } else return;
    const updated = await getRoles();
    setRoles(updated);
    const refresh = updated.find(r => r.id === selected.id);
    if (refresh) setSelected(refresh);
  };

  const modules = [...new Set(claims.map(c => c.module ?? 'Other'))];

  if (loading) return <div className="p-6 text-slate-400">Loading...</div>;

  return (
    <div className="p-6 flex gap-6 h-full">
      <div className="w-64 flex-shrink-0">
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-white text-sm font-semibold">Roles</h2>
          <button className="p-1 text-slate-400 hover:text-white"><Plus size={16} /></button>
        </div>
        <div className="space-y-1">
          {roles.map(role => (
            <button key={role.id} onClick={() => setSelected(role)}
              className={`w-full flex items-center justify-between px-3 py-2.5 rounded-lg text-sm transition-colors ${selected?.id === role.id ? 'bg-primary-600/20 text-primary-300' : 'text-slate-300 hover:bg-slate-700/50'}`}>
              <div className="flex items-center gap-2">
                <Shield size={14} />
                <span>{role.name}</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="text-xs text-slate-500">{role.claims.length}</span>
                {role.isSystem && <span className="text-xs bg-slate-700 text-slate-400 px-1 rounded">sys</span>}
              </div>
            </button>
          ))}
        </div>
      </div>

      <div className="flex-1">
        {selected && (
          <>
            <div className="mb-4">
              <h2 className="text-white text-lg font-semibold">{selected.name}</h2>
              <p className="text-slate-400 text-sm">{selected.description}</p>
            </div>

            <div className="space-y-5">
              {modules.map(module => {
                const moduleClaims = claims.filter(c => (c.module ?? 'Other') === module);
                return (
                  <div key={module}>
                    <h3 className="text-slate-400 text-xs font-medium tracking-wider mb-2">{module.toUpperCase()}</h3>
                    <div className="grid grid-cols-4 gap-2">
                      {moduleClaims.map(claim => {
                        const has = selected.claims.some(c => c.id === claim.id);
                        const isLocked = selected.isSystem && has;
                        return (
                          <button key={claim.id} onClick={() => !isLocked && toggleClaim(claim)}
                            disabled={isLocked}
                            className={`flex items-center gap-2 px-3 py-2 rounded-lg border text-left text-xs transition-all ${has ? 'bg-primary-600/20 border-primary-500/50 text-primary-300' : 'bg-slate-800 border-slate-700 text-slate-400 hover:border-slate-500'} ${isLocked ? 'cursor-default' : 'cursor-pointer'}`}>
                            {has && <Check size={12} />}
                            <span>{claim.name.split('.')[1] ?? claim.name}</span>
                          </button>
                        );
                      })}
                    </div>
                  </div>
                );
              })}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
