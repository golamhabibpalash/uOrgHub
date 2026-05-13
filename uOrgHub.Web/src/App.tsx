import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import AppLayout from "./components/layout/AppLayout";

function Placeholder({ name }: { name: string }) {
  return (
    <div className="text-gray-500 text-sm">{name} module — coming soon</div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<Navigate to="/hr" replace />} />
          <Route path="hr/*" element={<Placeholder name="HR & Payroll" />} />
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
