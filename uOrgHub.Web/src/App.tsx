import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import AppLayout from "./components/layout/AppLayout";
import HRDashboard from "./pages/hr/HRDashboard";
import Departments from "./pages/hr/Departments";

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
          <Route
            path="hr/employees"
            element={<Placeholder name="Employees" />}
          />
          <Route path="accounts/*" element={<Placeholder name="Accounts" />} />
          <Route
            path="inventory/*"
            element={<Placeholder name="Inventory" />}
          />
          <Route
            path="procurement/*"
            element={<Placeholder name="Procurement" />}
          />
          <Route path="projects/*" element={<Placeholder name="Projects" />} />
          <Route path="settings/*" element={<Placeholder name="Settings" />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
