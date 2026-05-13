import { useState } from "react";
import StatCard from "../../components/shared/StatCard";
import DataTable from "../../components/shared/DataTable";
import Departments from "./Departments";

export default function HRDashboard() {
  const [activeTab, setActiveTab] = useState<"overview" | "departments">("overview");

  const stats = [
    { label: "Total Employees", value: "245", sub: "+12%" },
    { label: "Departments", value: "8", sub: "+2" },
    { label: "Open Positions", value: "15", sub: "-3" },
    { label: "Pending Leave", value: "23", sub: "+5" },
  ];

  const employeeColumns = [
    { key: "name", label: "Name" },
    { key: "department", label: "Department" },
    { key: "position", label: "Position" },
    { key: "status", label: "Status" },
  ];

  const sampleData = [
    { id: "1", name: "John Doe", department: "Engineering", position: "Senior Dev", status: "Active" },
    { id: "2", name: "Jane Smith", department: "HR", position: "Manager", status: "Active" },
    { id: "3", name: "Bob Wilson", department: "Finance", position: "Analyst", status: "On Leave" },
  ];

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-6">HR Dashboard</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {stats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} sub={stat.sub} />
        ))}
      </div>

      <div className="flex gap-4 mb-6">
        <button
          onClick={() => setActiveTab("overview")}
          className={`px-4 py-2 rounded ${activeTab === "overview" ? "bg-blue-600 text-white" : "bg-gray-200"}`}
        >
          Overview
        </button>
        <button
          onClick={() => setActiveTab("departments")}
          className={`px-4 py-2 rounded ${activeTab === "departments" ? "bg-blue-600 text-white" : "bg-gray-200"}`}
        >
          Departments
        </button>
      </div>

      {activeTab === "overview" && (
        <div className="bg-white rounded-lg shadow p-4">
          <h2 className="text-lg font-semibold mb-4">Recent Employees</h2>
          <DataTable columns={employeeColumns} data={sampleData} />
        </div>
      )}

      {activeTab === "departments" && <Departments />}
    </div>
  );
}