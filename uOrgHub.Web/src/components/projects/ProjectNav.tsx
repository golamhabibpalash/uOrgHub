import { NavLink, useParams } from "react-router-dom";
import {
  Layers,
  FileSpreadsheet,
  ClipboardList,
  Package,
  Receipt,
  Milestone,
  FileText,
  MessageSquare,
  FolderOpen,
  Users,
  ClipboardCheck,
  AlertTriangle,
  ShieldAlert,
  Banknote,
} from "lucide-react";

const sections = [
  { label: "WBS", key: "wbs", icon: Layers },
  { label: "BOQ", key: "boq", icon: FileSpreadsheet },
  { label: "DPR", key: "dpr", icon: ClipboardList },
  { label: "Materials", key: "materials", icon: Package },
  { label: "Expenses", key: "expenses", icon: Receipt },
  { label: "Milestones", key: "milestones", icon: Milestone },
  { label: "Drawings", key: "drawings", icon: FileText },
  { label: "RFIs", key: "rfis", icon: MessageSquare },
  { label: "Submittals", key: "submittals", icon: FolderOpen },
  { label: "Resources", key: "resource-allocations", icon: Users },
  { label: "QA Checklists", key: "qa-checklists", icon: ClipboardCheck },
  { label: "NCRs", key: "ncrs", icon: AlertTriangle },
  { label: "Safety", key: "safety-incidents", icon: ShieldAlert },
  { label: "RA Bills", key: "ra-bills", icon: Banknote },
];

export default function ProjectNav() {
  const { id } = useParams<{ id: string }>();

  return (
    <div className="bg-white border border-gray-200 rounded-xl mb-4 overflow-x-auto">
      <div className="flex items-center px-2 py-1 gap-0.5 min-w-max">
        {sections.map(({ label, key, icon: Icon }) => (
          <NavLink
            key={key}
            to={`/projects/${id}/${key}`}
            className={({ isActive }) =>
              `flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium whitespace-nowrap transition-colors ${
                isActive
                  ? "bg-primary-50 text-primary-700"
                  : "text-gray-500 hover:text-gray-800 hover:bg-gray-50"
              }`
            }
          >
            <Icon size={13} />
            {label}
          </NavLink>
        ))}
      </div>
    </div>
  );
}
