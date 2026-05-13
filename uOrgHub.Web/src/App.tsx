import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import AppLayout from "./components/layout/AppLayout";
import HRDashboard from "./pages/hr/HRDashboard";
import Departments from "./pages/hr/Departments";
import Employees from "./pages/hr/Employees";
import Designations from "./pages/hr/Designations";
import LeaveManagement from "./pages/hr/LeaveManagement";
import AttendanceManagement from "./pages/hr/AttendanceManagement";
import PayrollManagement from "./pages/hr/PayrollManagement";
import Recruitment from "./pages/hr/Recruitment";
import PerformanceManagement from "./pages/hr/PerformanceManagement";

function Placeholder({ name }: { name: string }) {
  return <div className="text-gray-500 text-sm p-4">{name} — coming soon</div>;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<Navigate to="/hr" replace />} />
          <Route path="hr" element={<HRDashboard />} />
          <Route path="hr/departments" element={<Departments />} />
          <Route path="hr/employees" element={<Employees />} />
          <Route path="hr/designations" element={<Designations />} />
          <Route path="hr/leave" element={<LeaveManagement />} />
          <Route path="hr/attendance" element={<AttendanceManagement />} />
          <Route path="hr/payroll" element={<PayrollManagement />} />
          <Route path="hr/recruitment" element={<Recruitment />} />
          <Route path="hr/performance" element={<PerformanceManagement />} />
          <Route path="accounts/*" element={<Placeholder name="Accounts" />} />
          <Route path="inventory/*" element={<Placeholder name="Inventory" />} />
          <Route path="procurement/*" element={<Placeholder name="Procurement" />} />
          <Route path="projects/*" element={<Placeholder name="Projects" />} />
          <Route path="settings/*" element={<Placeholder name="Settings" />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}