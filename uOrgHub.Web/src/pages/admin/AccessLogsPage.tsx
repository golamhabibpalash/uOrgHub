import { useEffect, useState, useCallback } from 'react';
import {
  Search, X, Filter, RefreshCw,
} from 'lucide-react';
import {
  getAccessLogs,
  type AccessLogDto,
  type PagedResult,
  type AccessLogFilterParams,
} from '../../api/auth';

const HTTP_METHODS = ['', 'GET', 'POST', 'PUT', 'PATCH', 'DELETE'];
const PAGE_SIZES = [20, 30, 50, 100];

function fromDatetimeLocal(value: string): Date | undefined {
  if (!value) return undefined;
  const d = new Date(value);
  return isNaN(d.getTime()) ? undefined : d;
}

export default function AccessLogsPage() {
  const [logs, setLogs] = useState<PagedResult<AccessLogDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [showFilters, setShowFilters] = useState(false);

  const [filters, setFilters] = useState<AccessLogFilterParams>({
    page: 1,
    pageSize: 30,
    sortDescending: true,
  });
  const pageSize = filters.pageSize ?? 30;
  const [local, setLocal] = useState({
    search: '',
    username: '',
    module: '',
    action: '',
    httpMethod: '',
    isSuccess: '',
    entityType: '',
    ipAddress: '',
    statusCodeFrom: '',
    statusCodeTo: '',
    durationMin: '',
    durationMax: '',
    dateFrom: '',
    dateTo: '',
  });

  const syncLocalFromFilters = useCallback(() => {
    setLocal({
      search: filters.search ?? '',
      username: filters.username ?? '',
      module: filters.module ?? '',
      action: filters.action ?? '',
      httpMethod: filters.httpMethod ?? '',
      isSuccess: filters.isSuccess === undefined ? '' : String(filters.isSuccess),
      entityType: filters.entityType ?? '',
      ipAddress: filters.ipAddress ?? '',
      statusCodeFrom: filters.statusCodeFrom !== undefined ? String(filters.statusCodeFrom) : '',
      statusCodeTo: filters.statusCodeTo !== undefined ? String(filters.statusCodeTo) : '',
      durationMin: filters.durationMin !== undefined ? String(filters.durationMin) : '',
      durationMax: filters.durationMax !== undefined ? String(filters.durationMax) : '',
      dateFrom: filters.dateFrom ?? '',
      dateTo: filters.dateTo ?? '',
    });
  }, [filters]);

  useEffect(() => { syncLocalFromFilters(); }, [syncLocalFromFilters]);

  const applyFilters = useCallback(() => {
    const f: AccessLogFilterParams = { sortDescending: true };
    if (local.search) f.search = local.search;
    if (local.username) f.username = local.username;
    if (local.module) f.module = local.module;
    if (local.action) f.action = local.action;
    if (local.httpMethod) f.httpMethod = local.httpMethod;
    if (local.isSuccess !== '') f.isSuccess = local.isSuccess === 'true';
    if (local.entityType) f.entityType = local.entityType;
    if (local.ipAddress) f.ipAddress = local.ipAddress;
    if (local.statusCodeFrom) f.statusCodeFrom = Number(local.statusCodeFrom);
    if (local.statusCodeTo) f.statusCodeTo = Number(local.statusCodeTo);
    if (local.durationMin) f.durationMin = Number(local.durationMin);
    if (local.durationMax) f.durationMax = Number(local.durationMax);
    if (local.dateFrom) f.dateFrom = fromDatetimeLocal(local.dateFrom)?.toISOString();
    if (local.dateTo) f.dateTo = fromDatetimeLocal(local.dateTo)?.toISOString();
    f.page = 1;
    f.pageSize = pageSize;
    setFilters(f);
  }, [local, pageSize]);

  const clearFilters = () => {
    setLocal({
      search: '', username: '', module: '', action: '', httpMethod: '',
      isSuccess: '', entityType: '', ipAddress: '',
      statusCodeFrom: '', statusCodeTo: '', durationMin: '', durationMax: '',
      dateFrom: '', dateTo: '',
    });
    setFilters({ page: 1, pageSize, sortDescending: true });
  };

  useEffect(() => {
    setLoading(true);
    getAccessLogs(filters)
      .then(setLogs)
      .finally(() => setLoading(false));
  }, [filters]);

  const hasActiveFilters = Object.entries(local).some(([, v]) => v !== '');

  const statusColor = (code: number) => {
    if (code < 300) return 'text-green-400';
    if (code < 400) return 'text-yellow-400';
    return 'text-red-400';
  };

  const totalPages = logs ? Math.ceil(logs.totalCount / pageSize) : 0;

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h1 className="text-white text-xl font-semibold">Access Logs</h1>
          <p className="text-slate-400 text-sm mt-0.5">{logs?.totalCount ?? 0} total entries</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={() => setShowFilters(!showFilters)}
            className={`flex items-center gap-1.5 text-sm px-3 py-1.5 rounded-lg border transition-colors
              ${showFilters ? 'bg-primary-600 border-primary-500 text-white' : 'bg-slate-800 border-slate-700 text-slate-300 hover:border-slate-500'}`}>
            <Filter size={14} /> Filters {hasActiveFilters && <span className="w-2 h-2 rounded-full bg-primary-400" />}
          </button>
          <button onClick={() => setFilters({ ...filters })}
            className="flex items-center gap-1.5 text-sm px-3 py-1.5 rounded-lg bg-slate-800 border border-slate-700 text-slate-300 hover:border-slate-500">
            <RefreshCw size={14} />
          </button>
        </div>
      </div>

      {showFilters && (
        <div className="mb-4 p-4 bg-slate-800/80 border border-slate-700 rounded-xl">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            <div>
              <label className="block text-xs text-slate-400 mb-1">Search</label>
              <input type="text" placeholder="Username, action, endpoint, IP..."
                value={local.search} onChange={e => setLocal(p => ({ ...p, search: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Username</label>
              <input type="text" placeholder="Filter by username"
                value={local.username} onChange={e => setLocal(p => ({ ...p, username: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">HTTP Method</label>
              <select value={local.httpMethod}
                onChange={e => setLocal(p => ({ ...p, httpMethod: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500">
                <option value="">All</option>
                {HTTP_METHODS.filter(Boolean).map(m => <option key={m} value={m}>{m}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Status</label>
              <select value={local.isSuccess}
                onChange={e => setLocal(p => ({ ...p, isSuccess: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500">
                <option value="">All</option>
                <option value="true">Success</option>
                <option value="false">Failed</option>
              </select>
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Module</label>
              <input type="text" placeholder="e.g. auth, hr, accounts"
                value={local.module} onChange={e => setLocal(p => ({ ...p, module: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Action</label>
              <input type="text" placeholder="e.g. View, Create, Update"
                value={local.action} onChange={e => setLocal(p => ({ ...p, action: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Entity Type</label>
              <input type="text" placeholder="e.g. Employee, User"
                value={local.entityType} onChange={e => setLocal(p => ({ ...p, entityType: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">IP Address</label>
              <input type="text" placeholder="e.g. 192.168.1.1"
                value={local.ipAddress} onChange={e => setLocal(p => ({ ...p, ipAddress: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Date From</label>
              <input type="datetime-local"
                value={local.dateFrom} onChange={e => setLocal(p => ({ ...p, dateFrom: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Date To</label>
              <input type="datetime-local"
                value={local.dateTo} onChange={e => setLocal(p => ({ ...p, dateTo: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Status Code From</label>
              <input type="number" placeholder="e.g. 200"
                value={local.statusCodeFrom} onChange={e => setLocal(p => ({ ...p, statusCodeFrom: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Status Code To</label>
              <input type="number" placeholder="e.g. 599"
                value={local.statusCodeTo} onChange={e => setLocal(p => ({ ...p, statusCodeTo: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Duration Min (ms)</label>
              <input type="number" placeholder="e.g. 100"
                value={local.durationMin} onChange={e => setLocal(p => ({ ...p, durationMin: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
            <div>
              <label className="block text-xs text-slate-400 mb-1">Duration Max (ms)</label>
              <input type="number" placeholder="e.g. 5000"
                value={local.durationMax} onChange={e => setLocal(p => ({ ...p, durationMax: e.target.value }))}
                className="w-full bg-slate-900 border border-slate-700 text-white text-xs rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500" />
            </div>
          </div>
          <div className="flex items-center gap-2 mt-4">
            <button onClick={applyFilters}
              className="text-xs px-4 py-2 bg-primary-600 hover:bg-primary-500 text-white rounded-lg font-medium transition-colors">
              Apply Filters
            </button>
            {hasActiveFilters && (
              <button onClick={clearFilters}
                className="text-xs px-4 py-2 bg-slate-700 hover:bg-slate-600 text-slate-300 rounded-lg transition-colors flex items-center gap-1">
                <X size={12} /> Clear
              </button>
            )}
          </div>
        </div>
      )}

      {/* Search bar (always visible) */}
      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
        <input type="text" placeholder="Quick search username, action, endpoint, IP..."
          value={local.search}
          onChange={e => {
            setLocal(p => ({ ...p, search: e.target.value }));
          }}
          onKeyDown={e => { if (e.key === 'Enter') applyFilters(); }}
          className="w-full max-w-sm bg-slate-800 border border-slate-700 text-white text-sm rounded-lg pl-9 pr-4 py-2 focus:outline-none focus:border-primary-500" />
      </div>

      <div className="bg-slate-800 border border-slate-700 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-slate-700">
              <th className="text-left text-slate-400 font-medium px-4 py-3">User</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Action</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Endpoint</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Module</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Status</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Duration</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">IP</th>
              <th className="text-left text-slate-400 font-medium px-4 py-3">Time</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={8} className="text-center text-slate-500 py-8">Loading...</td></tr>
            ) : (logs?.items ?? []).length === 0 ? (
              <tr><td colSpan={8} className="text-center text-slate-500 py-8">No access logs found.</td></tr>
            ) : (logs?.items ?? []).map(l => (
              <tr key={l.id} className="border-b border-slate-700/50 hover:bg-slate-700/20">
                <td className="px-4 py-3 text-slate-300">{l.username ?? '—'}</td>
                <td className="px-4 py-3 text-white font-medium">{l.action}</td>
                <td className="px-4 py-3 text-slate-400 text-xs font-mono">{l.httpMethod} {l.endpoint}</td>
                <td className="px-4 py-3 text-slate-400 text-xs">{l.module ?? '—'}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs font-semibold ${statusColor(l.responseStatusCode)}`}>{l.responseStatusCode}</span>
                  {!l.isSuccess && l.errorMessage && (
                    <span className="ml-1 text-xs text-red-400" title={l.errorMessage}>⚠</span>
                  )}
                </td>
                <td className="px-4 py-3 text-slate-400 text-xs font-mono">
                  {l.durationMs >= 1000 ? `${(l.durationMs / 1000).toFixed(1)}s` : `${l.durationMs}ms`}
                </td>
                <td className="px-4 py-3 text-slate-400 text-xs">{l.ipAddress ?? '—'}</td>
                <td className="px-4 py-3 text-slate-400 text-xs">{new Date(l.createdAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between mt-4 text-sm text-slate-400">
          <div className="flex items-center gap-3">
            <span>Page {filters.page} of {totalPages}</span>
            <select value={filters.pageSize}
              onChange={e => setFilters(p => ({ ...p, pageSize: Number(e.target.value), page: 1 }))}
              className="bg-slate-800 border border-slate-700 text-white text-xs rounded-lg px-2 py-1 focus:outline-none">
              {PAGE_SIZES.map(s => <option key={s} value={s}>{s} / page</option>)}
            </select>
          </div>
          <div className="flex gap-2">
            <button disabled={!filters.page || filters.page <= 1}
              onClick={() => setFilters(p => ({ ...p, page: (p.page ?? 1) - 1 }))}
              className="px-3 py-1 bg-slate-700 rounded disabled:opacity-50 hover:bg-slate-600">Prev</button>
            <button disabled={(filters.page ?? 1) >= totalPages}
              onClick={() => setFilters(p => ({ ...p, page: (p.page ?? 1) + 1 }))}
              className="px-3 py-1 bg-slate-700 rounded disabled:opacity-50 hover:bg-slate-600">Next</button>
          </div>
        </div>
      )}
    </div>
  );
}
