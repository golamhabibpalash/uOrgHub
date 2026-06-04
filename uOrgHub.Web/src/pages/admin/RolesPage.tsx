import { useEffect, useState } from 'react';
import { Plus, Shield, Check, Lock } from 'lucide-react';
import { getRoles, getRoleById, getClaims, assignRoleClaims, type RoleDto, type ClaimDto } from '../../api/auth';

export default function RolesPage() {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [claims, setClaims] = useState<ClaimDto[]>([]);
  const [selected, setSelected] = useState<RoleDto | null>(null);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    const [r] = await Promise.all([getRoles(), getClaims().then(setClaims)]);
    setRoles(r);
    if (r.length > 0) {
      const full = await getRoleById(r[0].id);
      setSelected(full);
    }
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const toggleClaim = async (claim: ClaimDto) => {
    if (!selected) return;
    const currentIds = selected.claims?.map(c => c.id) ?? [];
    const has = currentIds.includes(claim.id);
    const nextIds = has ? currentIds.filter(id => id !== claim.id) : [...currentIds, claim.id];

    await assignRoleClaims(selected.id, nextIds);
    const updated = await getRoles();
    setRoles(updated);
    const full = await getRoleById(selected.id);
    setSelected(full);
  };

  const handleSelect = async (role: RoleDto) => {
    const full = await getRoleById(role.id);
    setSelected(full);
  };

  const modules = [...new Set(claims.map(c => c.module ?? 'Other'))];

  if (loading) return <div className="p-6 text-slate-400">Loading...</div>;

  return (
    <div className="p-6 flex gap-6">
      <div className="w-64 shrink-0">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-white text-sm font-semibold">Roles</h2>
            <p className="text-slate-400 text-xs mt-0.5">{roles.length} total</p>
          </div>
          <button className="flex items-center gap-1.5 bg-primary-600 hover:bg-primary-500 text-white text-xs font-medium px-3 py-1.5 rounded-lg transition-colors">
            <Plus size={14} /> Add
          </button>
        </div>
        <div className="bg-slate-800 border border-slate-700 rounded-xl overflow-hidden">
          <div className="divide-y divide-slate-700/50">
            {roles.map(role => (
              <button key={role.id} onClick={() => handleSelect(role)}
                className={`w-full flex items-center justify-between px-3 py-2.5 text-sm transition-colors ${selected?.id === role.id ? 'bg-primary-600/20 text-primary-300' : 'text-slate-300 hover:bg-slate-700/50'}`}>
                <div className="flex items-center gap-2 min-w-0">
                  <Shield size={14} className="shrink-0" />
                  <span className="truncate">{role.name}</span>
                </div>
                <div className="flex items-center gap-1.5 shrink-0">
                  <span className="text-xs text-slate-500">{role.claims?.length ?? 0}</span>
                  {role.isSystem && (
                    <span className="flex items-center gap-0.5 text-[10px] bg-amber-500/15 text-amber-400 px-1.5 py-0.5 rounded font-medium">
                      <Lock size={10} /> sys
                    </span>
                  )}
                </div>
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="flex-1 min-w-0">
        {selected && (
          <>
            <div className="mb-4">
              <div className="flex items-center gap-2">
                <h2 className="text-white text-lg font-semibold">{selected.name}</h2>
                {selected.isSystem && (
                  <span className="flex items-center gap-0.5 text-[11px] bg-amber-500/15 text-amber-400 px-1.5 py-0.5 rounded font-medium">
                    <Lock size={11} /> System
                  </span>
                )}
              </div>
              <p className="text-slate-400 text-sm mt-0.5">{selected.description || 'No description'}</p>
            </div>

            <div className="space-y-5">
              {modules.map(module => {
                const moduleClaims = claims.filter(c => (c.module ?? 'Other') === module);
                return (
                  <div key={module}>
                    <h3 className="text-slate-400 text-xs font-semibold tracking-wider mb-2">{module.toUpperCase()}</h3>
                    <div className="bg-slate-800 border border-slate-700 rounded-xl p-3">
                      <div className="grid grid-cols-4 gap-1.5">
                        {moduleClaims.map(claim => {
                          const has = selected.claims?.some(c => c.id === claim.id) ?? false;
                          return (
                            <button key={claim.id} onClick={() => toggleClaim(claim)}
                              className={`flex items-center gap-2 px-2.5 py-2 rounded-lg border text-left text-xs transition-all cursor-pointer ${has ? 'bg-primary-600/20 border-primary-500/50 text-primary-300' : 'bg-slate-800 border-slate-700 text-slate-400 hover:border-slate-500'}`}>
                              {has && <Check size={12} className="shrink-0" />}
                              <span className="truncate">{claim.name.split('.')[1] ?? claim.name}</span>
                            </button>
                          );
                        })}
                      </div>
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
