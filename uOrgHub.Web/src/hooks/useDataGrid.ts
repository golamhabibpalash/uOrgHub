import { useCallback, useMemo, useState } from "react";

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
}

export function useDataGrid(options: UseDataGridOptions = {}) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(options.defaultPageSize ?? 10);
  const [search, setSearchRaw] = useState("");
  const [sortBy, setSortBy] = useState<string | undefined>(options.defaultSortBy);
  const [sortDescending, setSortDescending] = useState(options.defaultSortDescending ?? false);
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
    ...(search && { search }),
    ...(sortBy && { sortBy, sortDescending }),
    ...(Object.keys(filters).length > 0 && { filtersJson: JSON.stringify(filters) }),
  }), [page, pageSize, search, sortBy, sortDescending, filters]);

  // Stable array covering every param that affects server results.
  // Pages spread this into their queryKey: ["entity", ...dg.queryKey, ...extras]
  const queryKey = useMemo(
    () => [page, pageSize, search, sortBy, sortDescending] as unknown[],
    [page, pageSize, search, sortBy, sortDescending],
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
