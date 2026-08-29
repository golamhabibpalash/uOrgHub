import { useCallback, useEffect, useMemo, useState } from "react";

export interface DataGridState {
  page: number;
  pageSize: number;
  search: string;
  sortBy: string | undefined;
  sortDescending: boolean;
  filters: Record<string, string>;
}

export interface UseDataGridOptions {
  defaultPageSize?: number;
  defaultSortBy?: string;
  defaultSortDescending?: boolean;
  /**
   * Wait this long after the last keystroke before the search reaches the server. Opt-in: left
   * unset, the search fires on every keystroke, which is what every existing caller expects.
   */
  searchDebounceMs?: number;
  /**
   * Seed the grid state from a previously-persisted location (e.g. URL query params), so a page
   * the user navigated away from — page number, search term, sort — restores on return. Optional;
   * left unset, the grid starts fresh as before.
   */
  initialPage?: number;
  initialPageSize?: number;
  initialSearch?: string;
  initialSortBy?: string;
  initialSortDescending?: boolean;
}

export function useDataGrid(options: UseDataGridOptions = {}) {
  const debounceMs = options.searchDebounceMs ?? 0;

  const [page, setPage] = useState(options.initialPage ?? 1);
  const [pageSize, setPageSize] = useState(options.initialPageSize ?? options.defaultPageSize ?? 10);
  const [search, setSearchRaw] = useState(options.initialSearch ?? "");
  // The input renders from `search` so typing stays responsive; only the settled value is allowed
  // to reach the query, so a burst of keystrokes costs one request rather than one each.
  const [settledSearch, setSettledSearch] = useState(options.initialSearch ?? "");

  useEffect(() => {
    if (debounceMs <= 0) return;
    const timer = setTimeout(() => setSettledSearch(search), debounceMs);
    return () => clearTimeout(timer);
  }, [search, debounceMs]);

  // Derived rather than stored for the undebounced case: writing state in the effect just to
  // mirror `search` would cost an extra render on every keystroke.
  const effectiveSearch = debounceMs > 0 ? settledSearch : search;
  const [sortBy, setSortBy] = useState<string | undefined>(
    options.initialSortBy ?? options.defaultSortBy,
  );
  const [sortDescending, setSortDescending] = useState(
    options.initialSortDescending ?? options.defaultSortDescending ?? false,
  );
  const [filters, setFilters] = useState<Record<string, string>>({});

  const setSearch = useCallback((value: string) => {
    setSearchRaw(value);
    setPage(1);
  }, []);

  const handleSort = useCallback((column: string) => {
    setPage(1);
    if (sortBy === column) {
      if (sortDescending) {
        setSortBy(undefined);
        setSortDescending(false);
      } else {
        setSortDescending(true);
      }
    } else {
      setSortBy(column);
      setSortDescending(false);
    }
  }, [sortBy, sortDescending]);

  const setFilter = useCallback((key: string, value: string) => {
    setPage(1);
    setFilters(prev => {
      const next = { ...prev };
      if (value) next[key] = value;
      else delete next[key];
      return next;
    });
  }, []);

  const resetFilters = useCallback(() => {
    setPage(1);
    setFilters({});
  }, []);

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    ...(effectiveSearch && { search: effectiveSearch }),
    ...(sortBy && { sortBy, sortDescending }),
    ...(Object.keys(filters).length > 0 && { filtersJson: JSON.stringify(filters) }),
  }), [page, pageSize, effectiveSearch, sortBy, sortDescending, filters]);

  // Stable array covering every param that affects server results.
  // Pages spread this into their queryKey: ["entity", ...dg.queryKey, ...extras]
  //
  // `filters` belongs here because it reaches the server via queryParams — left out, a filter
  // change would alter the request without changing the key, and the stale page would be served
  // from cache instead of refetched.
  const queryKey = useMemo(
    () => [page, pageSize, effectiveSearch, sortBy, sortDescending, filters] as unknown[],
    [page, pageSize, effectiveSearch, sortBy, sortDescending, filters],
  );

  const state: DataGridState = useMemo(() => ({
    page, pageSize, search, sortBy, sortDescending, filters,
  }), [page, pageSize, search, sortBy, sortDescending, filters]);

  const resetPage = useCallback(() => setPage(1), []);

  return {
    ...state,
    setPage,
    setPageSize: useCallback((size: number) => { setPageSize(size); setPage(1); }, []),
    setSearch,
    handleSort,
    setFilter,
    resetFilters,
    resetPage,
    queryParams,
    queryKey,
  };
}
