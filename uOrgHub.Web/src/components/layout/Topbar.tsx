import { Bell, Search } from "lucide-react";

interface TopbarProps {
  title: string;
  breadcrumb: string;
}

export default function Topbar({ title, breadcrumb }: TopbarProps) {
  return (
    <header className="bg-white border-b border-gray-200 h-[52px] flex items-center justify-between px-5">
      <div>
        <h1 className="text-[15px] font-medium text-gray-900">{title}</h1>
        <p className="text-xs text-gray-500">{breadcrumb}</p>
      </div>
      <div className="flex items-center gap-2">
        <div className="relative">
          <Search
            size={14}
            className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
          />
          <input
            type="text"
            placeholder="Search..."
            className="pl-8 pr-3 py-1.5 text-sm border border-gray-200 rounded-lg w-44 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
        <button className="p-2 rounded-lg border border-gray-200 hover:bg-gray-50">
          <Bell size={16} className="text-gray-500" />
        </button>
      </div>
    </header>
  );
}
