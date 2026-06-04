import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Plus, Search, UserCheck, UserX, Unlock, LogOut, CheckCircle } from 'lucide-react';
import { getUsers, activateUser, deactivateUser, unlockUser, forceLogout, type UserDto } from '../../api/auth';

export default function UsersPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const createdUser = (location.state as { created?: string })?.created;
  const [users, setUsers] = useState<UserDto[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const result = await getUsers({ page, pageSize: 20, search: search || undefined });
      setUsers(result.items);
      setTotal(result.totalCount);
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [page, search]);

  const action = async (fn: () => Promise<unknown>, msg: string) => {
    if (!confirm(msg)) return;
    try { await fn(); load(); } catch { /* ignore */ }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-white text-xl font-semibold">Users</h1>
          <p className="text-slate-400 text-sm mt-0.5">{total} total users</p>
        </div>
        {createdUser && (
          <div className="bg-emerald-500/15 border border-emerald-500/25 text-emerald-400 text-sm rounded-lg px-4 py-2.5 flex items-center gap-2">
            <CheckCircle size={16} /> User <strong className="mx-1">{createdUser}</strong> created successfully.
          </div>
        )}
        <button onClick={() => { window.history.replaceState({}, ''); navigate('/admin/users/new'); }}
          className="flex items-center gap-2 bg-primary-600 hover:bg-primary-500 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors">
          <Plus size={16} /> New User
        </button>
      </div>

      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
        <input type="text" placeholder="Search by name, username or email..."
          value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
          className="w-full max-w-sm bg-slate-800 border border-slate-700 text-white text-sm rounded-lg pl-9 pr-4 py-2 focus:outline-none focus:border-primary-500" />
      </div>

      <div className="bg-slate-800 border border-slate-700 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-slate-700">
              <th className="text-left text-slate-400 font-medium px-4 py-3">User</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Roles</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Status</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Last Login</th>
              <th className="text-right text-slate-400 font-medium px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5} className="text-center text-slate-500 py-8">Loading...</td></tr>
            ) : users.length === 0 ? (
              <tr><td colSpan={5} className="text-center text-slate-500 py-8">No users found</td></tr>
            ) : users.map(u => (
              <tr key={u.id} className="border-b border-slate-700/50 hover:bg-slate-700/30 cursor-pointer"
                onClick={() => navigate(`/admin/users/${u.id}`)}>
                <td className="px-4 py-3">
                  <div className="font-medium text-white">{u.fullName}</div>
                  <div className="text-slate-400 text-xs">{u.username} · {u.email}</div>
                </td>
                <td className="px-4 py-3">
                  <div className="flex gap-1 flex-wrap">
                    {u.roles.map(r => (
                      <span key={r} className="bg-primary-600/20 text-primary-300 text-xs px-2 py-0.5 rounded-full">{r}</span>
                    ))}
                  </div>
                </td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${u.isActive ? 'bg-green-500/20 text-green-400' : 'bg-red-500/20 text-red-400'}`}>
                    {u.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-slate-400 text-xs">
                  {u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleDateString() : 'Never'}
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center justify-end gap-1" onClick={e => e.stopPropagation()}>
                    {u.isActive ? (
                      <button title="Deactivate" onClick={() => action(() => deactivateUser(u.id), `Deactivate ${u.username}?`)}
                        className="p-1.5 text-slate-400 hover:text-red-400 hover:bg-red-500/10 rounded">
                        <UserX size={14} />
                      </button>
                    ) : (
                      <button title="Activate" onClick={() => action(() => activateUser(u.id), `Activate ${u.username}?`)}
                        className="p-1.5 text-slate-400 hover:text-green-400 hover:bg-green-500/10 rounded">
                        <UserCheck size={14} />
                      </button>
                    )}
                    <button title="Unlock" onClick={() => action(() => unlockUser(u.id), `Unlock ${u.username}?`)}
                      className="p-1.5 text-slate-400 hover:text-yellow-400 hover:bg-yellow-500/10 rounded">
                      <Unlock size={14} />
                    </button>
                    <button title="Force Logout" onClick={() => action(() => forceLogout(u.id), `Force logout ${u.username}?`)}
                      className="p-1.5 text-slate-400 hover:text-orange-400 hover:bg-orange-500/10 rounded">
                      <LogOut size={14} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {total > 20 && (
        <div className="flex items-center justify-between mt-4 text-sm text-slate-400">
          <span>Page {page} of {Math.ceil(total / 20)}</span>
          <div className="flex gap-2">
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}
              className="px-3 py-1 bg-slate-700 rounded disabled:opacity-50 hover:bg-slate-600">Prev</button>
            <button disabled={page >= Math.ceil(total / 20)} onClick={() => setPage(p => p + 1)}
              className="px-3 py-1 bg-slate-700 rounded disabled:opacity-50 hover:bg-slate-600">Next</button>
          </div>
        </div>
      )}
    </div>
  );
}
