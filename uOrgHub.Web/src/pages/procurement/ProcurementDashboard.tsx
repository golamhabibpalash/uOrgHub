import { ShoppingCart, FileText, Package, TrendingUp } from "lucide-react";

export default function ProcurementDashboard() {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-medium text-gray-900">Procurement Dashboard</h2>
          <p className="text-sm text-gray-400">Overview of procurement activities</p>
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-medium text-gray-900">Procurement Flow</h3>
        </div>
        <div className="space-y-3">
          <div className="flex items-center gap-3 p-3 bg-blue-50 rounded-lg">
            <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center">
              <FileText size={16} className="text-white" />
            </div>
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-900">Purchase Requisition</p>
              <p className="text-xs text-gray-500">Request for goods/services</p>
            </div>
            <span className="text-xs text-blue-600 font-medium">Draft → Approved</span>
          </div>
          <div className="flex items-center gap-3 p-3 bg-purple-50 rounded-lg">
            <div className="w-8 h-8 bg-purple-500 rounded-full flex items-center justify-center">
              <ShoppingCart size={16} className="text-white" />
            </div>
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-900">Request for Quotation</p>
              <p className="text-xs text-gray-500">Get vendor quotations</p>
            </div>
            <span className="text-xs text-purple-600 font-medium">Draft → Sent → Closed</span>
          </div>
          <div className="flex items-center gap-3 p-3 bg-green-50 rounded-lg">
            <div className="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center">
              <Package size={16} className="text-white" />
            </div>
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-900">Purchase Order</p>
              <p className="text-xs text-gray-500">Issue PO to vendor</p>
            </div>
            <span className="text-xs text-green-600 font-medium">Draft → Confirmed → Received</span>
          </div>
          <div className="flex items-center gap-3 p-3 bg-orange-50 rounded-lg">
            <div className="w-8 h-8 bg-orange-500 rounded-full flex items-center justify-center">
              <TrendingUp size={16} className="text-white" />
            </div>
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-900">Goods Received Note</p>
              <p className="text-xs text-gray-500">Receive and verify goods</p>
            </div>
            <span className="text-xs text-orange-600 font-medium">Draft → Confirmed</span>
          </div>
        </div>
      </div>
    </div>
  );
}