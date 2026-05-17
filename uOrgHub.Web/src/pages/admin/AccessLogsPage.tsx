import { useEffect, useState } from 'react';
import { Search } from 'lucide-react';
import { getAccessLogs, type AccessLogDto, type PagedResult } from '../../api/auth';

export default function AccessLogsPage() {
  const [logs, setLogs] = useState<PagedResult<AccessLogDto> | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    getAccessLogs({ page, pageSize: 30, search: search || undefined })
      .then(setLogs)
      .finally(() => setLoading(false));
  }, [page, search]);

  const statusColor = (code: number) => {
    if (code < 300) return 'text-green-400';
    if (code < 400) return 'text-yellow-400';
    return 'text-red-400';
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-white text-xl font-semibold">Access Logs</h1>
          <p className="text-slate-400 text-sm mt-0.5">{logs?.totalCount ?? 0} total entries</p>
        </div>
      </div>

      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
        <input type="text" placeholder="Search by username, action, endpoint..."
          value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
          className="w-full max-w-sm bg-slate-800 border border-slate-700 text-white text-sm rounded-lg pl-9 pr-4 py-2 focus:outline-none focus:border-primary-500" />
      </div>

      <div className="bg-slate-800 border border-slate-700 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-slate-700">
              <th className="text-left text-slate-400 font-medium px-4 py-3">User</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Action</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Endpoint</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Status</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">IP</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Time</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={6} className="text-center text-slate-500 py-8">Loading...</td></tr>
            ) : (logs?.items ?? []).map(l => (
              <tr key={l.id} className="border-b border-slate-700/50 hover:bg-slate-700/20">
                <td className="px-4 py-3 text-slate-300">{l.username ?? '—'}</td>
                <td className="px-4 py-3 text-white font-medium">{l.action}</td>
                <td className="px-4 py-3 text-slate-400 text-xs font-mono">{l.httpMethod} {l.endpoint}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs font-semibold ${statusColor(l.responseStatusCode)}`}>{l.responseStatusCode}</span>
                </td>
                <td className="px-4 py-3 text-slate-400 text-xs">{l.ipAddress ?? '—'}</td>
                <td className="px-4 py-3 text-slate-400 text-xs">{new Date(l.createdAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {(logs?.totalCount ?? 0) > 30 && (
        <div className="flex items-center justify-between mt-4 text-sm text-slate-400">
          <span>Page {page} of {Math.ceil((logs?.totalCount ?? 0) / 30)}</span>
          <div className="flex gap-2">
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}
              className="px-3 py-1 bg-slate-700 rounded disabled:opacity-50 hover:bg-slate-600">Prev</button>
            <button disabled={page >= Math.ceil((logs?.totalCount ?? 0) / 30)} onClick={() => setPage(p => p + 1)}
              className="px-3 py-1 bg-slate-700 rounded disabled:opacity-50 hover:bg-slate-600">Next</button>
          </div>
        </div>
      )}
    </div>
  );
}
