import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Shield, Key, Monitor, ScrollText } from 'lucide-react';
import {
  getUserById, getRoles, getClaims, setUserRoles, addUserClaim, removeUserClaim,
  getUserSessions, getUserAccessLogs,
  type UserDto, type RoleDto, type ClaimDto, type SessionDto, type AccessLogDto, type PagedResult
} from '../../api/auth';

type Tab = 'profile' | 'roles' | 'claims' | 'sessions' | 'logs';

export default function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState<Tab>('profile');
  const [user, setUser] = useState<UserDto | null>(null);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [claims, setClaims] = useState<ClaimDto[]>([]);
  const [sessions, setSessions] = useState<SessionDto[]>([]);
  const [logs, setLogs] = useState<PagedResult<AccessLogDto> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    Promise.all([getUserById(id), getRoles(), getClaims()])
      .then(([u, r, c]) => { setUser(u); setRoles(r); setClaims(c); })
      .finally(() => setLoading(false));
  }, [id]);

  useEffect(() => {
    if (!id) return;
    if (tab === 'sessions') getUserSessions(id).then(setSessions);
    if (tab === 'logs') getUserAccessLogs(id).then(setLogs);
  }, [tab, id]);

  const toggleRole = async (roleId: string, roleName: string) => {
    if (!user || !id) return;
    const hasRole = user.roles.includes(roleName);
    const newRoleIds = hasRole
      ? roles.filter(r => user.roles.includes(r.name) && r.name !== roleName).map(r => r.id)
      : [...roles.filter(r => user.roles.includes(r.name)).map(r => r.id), roleId];
    await setUserRoles(id, newRoleIds);
    const updated = await getUserById(id);
    setUser(updated);
  };

  const toggleClaim = async (claim: ClaimDto) => {
    if (!user || !id) return;
    const hasClaim = user.claims.includes(claim.name);
    if (hasClaim) { await removeUserClaim(id, claim.id); }
    else { await addUserClaim(id, claim.id, true); }
    const updated = await getUserById(id);
    setUser(updated);
  };

  const tabs: { key: Tab; label: string; icon: React.ElementType }[] = [
    { key: 'profile', label: 'Profile', icon: Monitor },
    { key: 'roles', label: 'Roles', icon: Shield },
    { key: 'claims', label: 'Claims', icon: Key },
    { key: 'sessions', label: 'Sessions', icon: Monitor },
    { key: 'logs', label: 'Access Log', icon: ScrollText },
  ];

  if (loading) return <div className="p-6 text-slate-400">Loading...</div>;
  if (!user) return <div className="p-6 text-slate-400">User not found.</div>;

  const claimsByModule = claims.reduce((acc, c) => {
    const m = c.module ?? 'Other';
    if (!acc[m]) acc[m] = [];
    acc[m].push(c);
    return acc;
  }, {} as Record<string, ClaimDto[]>);

  return (
    <div className="p-6">
      <button onClick={() => navigate('/admin/users')} className="flex items-center gap-2 text-slate-400 hover:text-slate-200 text-sm mb-4">
        <ArrowLeft size={16} /> Back to Users
      </button>

      <div className="flex items-center gap-4 mb-6">
        <div className="w-12 h-12 rounded-full bg-primary-700 flex items-center justify-center text-white font-semibold">
          {user.firstName[0]}{user.lastName[0]}
        </div>
        <div>
          <h1 className="text-white text-xl font-semibold">{user.fullName}</h1>
          <p className="text-slate-400 text-sm">{user.username} · {user.email}</p>
        </div>
        <span className={`ml-auto text-xs px-2.5 py-1 rounded-full ${user.isActive ? 'bg-green-500/20 text-green-400' : 'bg-red-500/20 text-red-400'}`}>
          {user.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>

      <div className="flex gap-1 mb-6 border-b border-slate-700 pb-0">
        {tabs.map(({ key, label, icon: Icon }) => (
          <button key={key} onClick={() => setTab(key)}
            className={`flex items-center gap-2 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${tab === key ? 'border-primary-500 text-primary-400' : 'border-transparent text-slate-400 hover:text-slate-200'}`}>
            <Icon size={14} /> {label}
          </button>
        ))}
      </div>

      {tab === 'profile' && (
        <div className="grid grid-cols-2 gap-4 max-w-2xl">
          {[
            ['Username', user.username], ['Email', user.email], ['Phone', user.phoneNumber ?? '—'],
            ['Employee ID', user.employeeId ?? '—'], ['2FA', user.isTwoFactorEnabled ? `Enabled (${user.twoFactorMethod})` : 'Disabled'],
            ['Last Login', user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleString() : 'Never'],
          ].map(([label, value]) => (
            <div key={label} className="bg-slate-800 border border-slate-700 rounded-lg p-4">
              <div className="text-slate-400 text-xs mb-1">{label}</div>
              <div className="text-white text-sm">{value}</div>
            </div>
          ))}
        </div>
      )}

      {tab === 'roles' && (
        <div className="grid grid-cols-3 gap-3 max-w-2xl">
          {roles.map(role => {
            const has = user.roles.includes(role.name);
            return (
              <button key={role.id} onClick={() => !role.isSystem || has ? toggleRole(role.id, role.name) : undefined}
                className={`p-4 rounded-lg border text-left transition-all ${has ? 'bg-primary-600/20 border-primary-500/50' : 'bg-slate-800 border-slate-700 hover:border-slate-500'}`}>
                <div className="flex items-center justify-between mb-1">
                  <span className="text-white text-sm font-medium">{role.name}</span>
                  {has && <span className="w-2 h-2 rounded-full bg-primary-400" />}
                </div>
                <div className="text-slate-400 text-xs">{role.claims.length} claims</div>
              </button>
            );
          })}
        </div>
      )}

      {tab === 'claims' && (
        <div className="space-y-4 max-w-3xl">
          {Object.entries(claimsByModule).map(([module, moduleClaims]) => (
            <div key={module}>
              <h3 className="text-slate-400 text-xs font-medium tracking-wider mb-2">{module.toUpperCase()}</h3>
              <div className="grid grid-cols-3 gap-2">
                {moduleClaims.map(claim => {
                  const has = user.claims.includes(claim.name);
                  return (
                    <button key={claim.id} onClick={() => toggleClaim(claim)}
                      className={`px-3 py-2 rounded-lg border text-left text-sm transition-all ${has ? 'bg-primary-600/20 border-primary-500/50 text-primary-300' : 'bg-slate-800 border-slate-700 text-slate-400 hover:border-slate-500'}`}>
                      {claim.name}
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      )}

      {tab === 'sessions' && (
        <div className="space-y-2 max-w-3xl">
          {sessions.length === 0 && <p className="text-slate-400 text-sm">No active sessions.</p>}
          {sessions.map(s => (
            <div key={s.id} className="bg-slate-800 border border-slate-700 rounded-lg p-4 flex items-center justify-between">
              <div>
                <div className="text-white text-sm">{s.browser ?? 'Unknown Browser'} · {s.os ?? 'Unknown OS'}</div>
                <div className="text-slate-400 text-xs">{s.ipAddress} · Login: {new Date(s.loginAt).toLocaleString()}</div>
              </div>
              <span className={`text-xs px-2 py-0.5 rounded-full ${s.isActive ? 'bg-green-500/20 text-green-400' : 'bg-slate-600 text-slate-400'}`}>
                {s.isActive ? 'Active' : 'Ended'}
              </span>
            </div>
          ))}
        </div>
      )}

      {tab === 'logs' && (
        <div className="max-w-4xl">
          {!logs || logs.items.length === 0 ? <p className="text-slate-400 text-sm">No access logs.</p> : (
            <table className="w-full text-sm">
              <thead><tr className="border-b border-slate-700">
                <th className="text-left text-slate-400 font-medium px-2 py-2">Action</th>
                <th className="text-left text-slate-400 font-medium px-2 py-2">Endpoint</th>
                <th className="text-left text-slate-400 font-medium px-2 py-2">Status</th>
                <th className="text-left text-slate-400 font-medium px-2 py-2">Time</th>
              </tr></thead>
              <tbody>
                {logs.items.map(l => (
                  <tr key={l.id} className="border-b border-slate-700/50">
                    <td className="px-2 py-2 text-white">{l.action}</td>
                    <td className="px-2 py-2 text-slate-400 text-xs">{l.httpMethod} {l.endpoint}</td>
                    <td className="px-2 py-2">
                      <span className={`text-xs px-1.5 py-0.5 rounded ${l.isSuccess ? 'text-green-400' : 'text-red-400'}`}>
                        {l.responseStatusCode}
                      </span>
                    </td>
                    <td className="px-2 py-2 text-slate-400 text-xs">{new Date(l.createdAt).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
