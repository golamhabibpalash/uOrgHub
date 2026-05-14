import { Outlet, useLocation } from "react-router-dom";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

const pageTitles: Record<string, { title: string; breadcrumb: string }> = {
  "/dashboard": { title: "Dashboard", breadcrumb: "Home / Dashboard" },
  "/hr": { title: "HR & Payroll", breadcrumb: "Dashboard / HR & Payroll" },
  "/accounts": { title: "Accounts", breadcrumb: "Dashboard / Accounts" },
  "/inventory": { title: "Inventory", breadcrumb: "Dashboard / Inventory" },
  "/procurement": {
    title: "Procurement",
    breadcrumb: "Dashboard / Procurement",
  },
  "/projects": { title: "Projects", breadcrumb: "Dashboard / Projects" },
  "/settings": { title: "Settings", breadcrumb: "Dashboard / Settings" },
};

export default function AppLayout() {
  const { pathname } = useLocation();
  const module = "/" + pathname.split("/")[1];
  const meta = pageTitles[module] ?? {
    title: "uOrgHub",
    breadcrumb: "Dashboard",
  };

  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar />
      <div className="flex flex-col flex-1 overflow-hidden">
        <Topbar title={meta.title} breadcrumb={meta.breadcrumb} />
        <main className="flex-1 overflow-y-auto p-5">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
