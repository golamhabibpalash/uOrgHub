import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import ExportMenu from "../../components/shared/ExportMenu";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import { getStockBalances, getWarehouses, getItemVariants, StockBalance } from "../../api/inventory";

export default function StockBalances() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [filterWarehouseId, setFilterWarehouseId] = useState("");
  const [filterVariantId, setFilterVariantId] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["stock-balances", page, search, filterWarehouseId, filterVariantId],
    queryFn: () => getStockBalances({ page, pageSize: 10, search }, filterWarehouseId || undefined, filterVariantId || undefined),
  });

  const { data: warehousesData } = useQuery({
    queryKey: ["warehouses-all"],
    queryFn: () => getWarehouses({ page: 1, pageSize: 100 }),
  });

  const { data: variantsData } = useQuery({
    queryKey: ["item-variants-all"],
    queryFn: () => getItemVariants({ page: 1, pageSize: 200 }),
  });

  const balances = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const allWarehouses = warehousesData?.data?.data?.items ?? [];
  const allVariants = variantsData?.data?.data?.items ?? [];

  function qtyColor(qty: number) {
    if (qty <= 0) return "text-red-600 font-medium";
    if (qty < 5) return "text-amber-600 font-medium";
    return "text-gray-900";
  }

  const columns = [
    { key: "variantSKU", label: "SKU", render: (row: StockBalance) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.variantSKU}</span> },
    { key: "itemBaseName", label: "Item" },
    { key: "variantName", label: "Variant" },
    { key: "warehouseName", label: "Warehouse", render: (row: StockBalance) => <span>{row.warehouseName} <span className="text-gray-400 text-xs">({row.warehouseCode})</span></span> },
    { key: "quantityOnHand", label: "On Hand", render: (row: StockBalance) => <span className={qtyColor(row.quantityOnHand)}>{row.quantityOnHand}</span> },
    { key: "quantityReserved", label: "Reserved", render: (row: StockBalance) => <span className="text-gray-500">{row.quantityReserved}</span> },
    {
      key: "quantityAvailable", label: "Available",
      render: (row: StockBalance) => {
        const avail = row.quantityOnHand - row.quantityReserved;
        return <span className={`font-semibold ${avail <= 0 ? "text-red-600" : "text-green-600"}`}>{avail}</span>;
      },
    },
    {
      key: "lastUpdated", label: "Last Updated",
      render: (row: StockBalance) => <span className="text-gray-400 text-xs">{new Date(row.lastUpdated).toLocaleDateString()}</span>,
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Stock Balances</h2>
          <p className="text-xs text-gray-400">Real-time inventory levels per warehouse</p>
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3 flex-wrap items-center justify-between">
          <input
            type="text" placeholder="Search SKU, item..." value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-48 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <select value={filterWarehouseId} onChange={(e) => { setFilterWarehouseId(e.target.value); setPage(1); }} className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Warehouses</option>
            {allWarehouses.map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
          </select>
          <select value={filterVariantId} onChange={(e) => { setFilterVariantId(e.target.value); setPage(1); }} className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Variants</option>
            {allVariants.map((v) => <option key={v.id} value={v.id}>{v.sku} — {v.variantName}</option>)}
          </select>
          <div className="ml-auto"><ExportMenu baseUrl="/stockbalances" filters={{ search: search || undefined, warehouseId: filterWarehouseId || undefined, itemVariantId: filterVariantId || undefined }} /></div>
        </div>
        <DataTable columns={columns} data={balances} loading={isLoading} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>
    </div>
  );
}
