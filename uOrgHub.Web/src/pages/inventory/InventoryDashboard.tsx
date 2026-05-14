import { useQuery } from "@tanstack/react-query";
import { Package, Warehouse, ArrowDownToLine, ArrowUpFromLine, Tag, Ruler } from "lucide-react";
import { NavLink } from "react-router-dom";
import StatCard from "../../components/shared/StatCard";
import {
  getItems,
  getWarehouses,
  getInventoryTypes,
  getUnitsOfMeasure,
  getStockTransactions,
  getStockBalances,
} from "../../api/inventory";

const modules = [
  { label: "Inventory Types", path: "/inventory/types", icon: Tag, color: "bg-blue-50 text-blue-600" },
  { label: "Categories", path: "/inventory/categories", icon: Package, color: "bg-indigo-50 text-indigo-600" },
  { label: "Units of Measure", path: "/inventory/units-of-measure", icon: Ruler, color: "bg-purple-50 text-purple-600" },
  { label: "Attribute Definitions", path: "/inventory/attributes", icon: Tag, color: "bg-pink-50 text-pink-600" },
  { label: "Items", path: "/inventory/items", icon: Package, color: "bg-orange-50 text-orange-600" },
  { label: "Item Variants", path: "/inventory/item-variants", icon: Package, color: "bg-amber-50 text-amber-600" },
  { label: "Warehouses", path: "/inventory/warehouses", icon: Warehouse, color: "bg-teal-50 text-teal-600" },
  { label: "Stock Balances", path: "/inventory/stock-balances", icon: ArrowDownToLine, color: "bg-green-50 text-green-600" },
  { label: "Stock Transactions", path: "/inventory/stock-transactions", icon: ArrowUpFromLine, color: "bg-red-50 text-red-600" },
];

export default function InventoryDashboard() {
  const pagination = { page: 1, pageSize: 1 };

  const { data: itemsData } = useQuery({ queryKey: ["items", 1, ""], queryFn: () => getItems(pagination) });
  const { data: warehousesData } = useQuery({ queryKey: ["warehouses", 1, ""], queryFn: () => getWarehouses(pagination) });
  const { data: typesData } = useQuery({ queryKey: ["inventory-types", 1, ""], queryFn: () => getInventoryTypes(pagination) });
  const { data: uomsData } = useQuery({ queryKey: ["units-of-measure", 1, ""], queryFn: () => getUnitsOfMeasure(pagination) });
  const { data: txnData } = useQuery({ queryKey: ["stock-transactions", 1, ""], queryFn: () => getStockTransactions(pagination) });
  const { data: balancesData } = useQuery({ queryKey: ["stock-balances", 1, ""], queryFn: () => getStockBalances(pagination) });

  const totalItems = itemsData?.data?.data?.totalCount ?? 0;
  const totalWarehouses = warehousesData?.data?.data?.totalCount ?? 0;
  const totalTypes = typesData?.data?.data?.totalCount ?? 0;
  const totalUoMs = uomsData?.data?.data?.totalCount ?? 0;
  const totalTransactions = txnData?.data?.data?.totalCount ?? 0;
  const totalBalanceRecords = balancesData?.data?.data?.totalCount ?? 0;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-base font-medium text-gray-900">Inventory Dashboard</h2>
        <p className="text-xs text-gray-400">Overview of inventory data</p>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <StatCard label="Total Items" value={totalItems} />
        <StatCard label="Warehouses" value={totalWarehouses} />
        <StatCard label="Inventory Types" value={totalTypes} />
        <StatCard label="Units of Measure" value={totalUoMs} />
        <StatCard label="Transactions" value={totalTransactions} />
        <StatCard label="Stock Records" value={totalBalanceRecords} />
      </div>

      <div>
        <h3 className="text-sm font-medium text-gray-700 mb-3">Inventory Modules</h3>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-3">
          {modules.map(({ label, path, icon: Icon, color }) => (
            <NavLink
              key={path}
              to={path}
              className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col items-center gap-2 hover:shadow-sm hover:border-primary-200 transition-all"
            >
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${color}`}>
                <Icon size={20} />
              </div>
              <span className="text-xs font-medium text-gray-600 text-center leading-tight">{label}</span>
            </NavLink>
          ))}
        </div>
      </div>
    </div>
  );
}
