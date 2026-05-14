import { ShoppingCart, Users, FileText, Package, TrendingUp, Clock } from "lucide-react";
import StatCard from "../../components/shared/StatCard";

export default function ProcurementDashboard() {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-medium text-gray-900">Procurement Dashboard</h2>
          <p className="text-sm text-gray-400">Overview of procurement activities</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Active Vendors" value="24" icon={Users} color="blue" subtitle="+3 this month" />
        <StatCard title="Pending PRs" value="12" icon={FileText} color="orange" subtitle="Awaiting approval" />
        <StatCard title="Open RFQs" value="8" icon={ShoppingCart} color="purple" subtitle="Active quotations" />
        <StatCard title="POs to Receive" value="15" icon={Package} color="green" subtitle="In transit" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-medium text-gray-900">Recent Activities</h3>
          </div>
          <div className="space-y-4">
            {[
              { text: "PR-2026-0045 approved by Admin", time: "2 hours ago", icon: FileText, color: "green" },
              { text: "RFQ-2026-0012 sent to vendors", time: "4 hours ago", icon: ShoppingCart, color: "purple" },
              { text: "PO-2026-0023 confirmed by ABC Suppliers", time: "6 hours ago", icon: Package, color: "blue" },
              { text: "GRN-2026-0018 confirmed - received 150 items", time: "1 day ago", icon: TrendingUp, color: "orange" },
              { text: "New vendor 'Steel Corp Ltd' added", time: "2 days ago", icon: Users, color: "gray" },
            ].map((activity, i) => (
              <div key={i} className="flex items-start gap-3">
                <div className={`w-8 h-8 rounded-full flex items-center justify-center bg-${activity.color}-50`}>
                  <activity.icon size={14} className={`text-${activity.color}-600`} />
                </div>
                <div className="flex-1">
                  <p className="text-sm text-gray-900">{activity.text}</p>
                  <div className="flex items-center gap-1 mt-0.5">
                    <Clock size={10} className="text-gray-400" />
                    <span className="text-xs text-gray-400">{activity.time}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}