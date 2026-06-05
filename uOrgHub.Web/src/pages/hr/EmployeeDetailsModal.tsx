import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { X, Mail, Briefcase, User, CalendarDays } from "lucide-react";
import Avatar from "../../components/shared/Avatar";
import { getEmployeeById } from "../../api/hr";

interface Props {
  employeeId: string | null;
  onClose: () => void;
}

function fmtDate(value?: string) {
  if (!value) return null;
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return null;
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

function money(value?: number) {
  if (value == null) return null;
  return new Intl.NumberFormat("en-BD", { style: "currency", currency: "BDT", maximumFractionDigits: 0 }).format(value);
}

/** A single labelled value; renders "N/A" (muted) when the value is missing. */
function Field({ label, value }: { label: string; value?: React.ReactNode }) {
  const empty = value === null || value === undefined || value === "";
  return (
    <div className="min-w-0">
      <dt className="text-[11px] uppercase tracking-wide text-gray-400 mb-0.5">{label}</dt>
      <dd className={`text-sm break-words ${empty ? "text-gray-300 italic" : "text-gray-800"}`}>
        {empty ? "N/A" : value}
      </dd>
    </div>
  );
}

function Section({ icon, title, children }: { icon: React.ReactNode; title: string; children: React.ReactNode }) {
  return (
    <div>
      <div className="flex items-center gap-2 mb-3">
        <span className="text-primary-500">{icon}</span>
        <h3 className="text-xs font-semibold uppercase tracking-wide text-gray-500">{title}</h3>
      </div>
      <dl className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-3 pl-1">{children}</dl>
    </div>
  );
}

function StatusBadge({ status }: { status?: string }) {
  const cls =
    status === "Active"
      ? "bg-green-50 text-green-700 ring-green-200"
      : status === "OnLeave"
      ? "bg-yellow-50 text-yellow-700 ring-yellow-200"
      : "bg-gray-100 text-gray-600 ring-gray-200";
  return (
    <span className={`text-xs px-2.5 py-0.5 rounded-full ring-1 ${cls}`}>{status ?? "N/A"}</span>
  );
}

export default function EmployeeDetailsModal({ employeeId, onClose }: Props) {
  const [show, setShow] = useState(false);

  // Drive the enter/exit animation off mount.
  useEffect(() => {
    if (employeeId) {
      const t = requestAnimationFrame(() => setShow(true));
      return () => cancelAnimationFrame(t);
    }
    setShow(false);
  }, [employeeId]);

  // Close on Escape.
  useEffect(() => {
    if (!employeeId) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [employeeId, onClose]);

  const { data, isLoading, isError } = useQuery({
    queryKey: ["employee-detail", employeeId],
    queryFn: () => getEmployeeById(employeeId as string),
    enabled: !!employeeId,
  });

  if (!employeeId) return null;
  const emp = data?.data?.data;
  const fullName = emp ? [emp.firstName, emp.middleName, emp.lastName].filter(Boolean).join(" ") : "";

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center p-4 transition-opacity duration-200 ${
        show ? "bg-black/40 opacity-100" : "bg-black/0 opacity-0"
      }`}
      onClick={onClose}
    >
      <div
        className={`bg-white rounded-2xl shadow-xl w-full max-w-2xl flex flex-col max-h-[88vh] transition-all duration-200 ${
          show ? "opacity-100 scale-100 translate-y-0" : "opacity-0 scale-95 translate-y-2"
        }`}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="relative shrink-0">
          <div className="h-20 rounded-t-2xl bg-gradient-to-r from-primary-500 to-primary-600" />
          <button
            onClick={onClose}
            className="absolute top-3 right-3 text-white/80 hover:text-white"
            title="Close"
          >
            <X size={18} />
          </button>
          <div className="px-6 pb-4 -mt-10 flex items-end gap-4">
            <div className="ring-4 ring-white rounded-full">
              <Avatar src={emp?.profilePicturePath} firstName={emp?.firstName} lastName={emp?.lastName} size="xl" />
            </div>
            <div className="pb-1 min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <h2 className="text-lg font-semibold text-gray-900 truncate">
                  {isLoading ? "Loading…" : fullName || "Employee"}
                </h2>
                {!isLoading && <StatusBadge status={emp?.status} />}
              </div>
              <p className="text-sm text-gray-500 truncate">
                {emp?.designationName || "—"}
                {emp?.departmentName ? ` · ${emp.departmentName}` : ""}
              </p>
              {emp?.employeeCode && (
                <p className="text-xs text-gray-400 mt-0.5">ID: {emp.employeeCode}</p>
              )}
            </div>
          </div>
        </div>

        {/* Body */}
        <div className="px-6 py-5 overflow-y-auto space-y-6">
          {isLoading ? (
            <div className="flex items-center justify-center py-16 text-sm text-gray-400 gap-2">
              <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
              Loading employee details…
            </div>
          ) : isError || !emp ? (
            <div className="text-center py-16 text-sm text-red-500">
              Could not load employee details. Please try again.
            </div>
          ) : (
            <>
              <Section icon={<Briefcase size={15} />} title="Job Information">
                <Field label="Employee Code" value={emp.employeeCode} />
                <Field label="Designation" value={emp.designationName} />
                <Field label="Department" value={emp.departmentName} />
                <Field label="Employment Type" value={emp.employmentType} />
                <Field label="Joining Date" value={fmtDate(emp.joiningDate)} />
                <Field label="Confirmation Date" value={fmtDate(emp.confirmationDate)} />
                <Field label="Reporting Manager" value={emp.managerName} />
                <Field label="Basic Salary" value={money(emp.basicSalary)} />
              </Section>

              <hr className="border-gray-100" />

              <Section icon={<Mail size={15} />} title="Contact Information">
                <Field label="Email" value={emp.email} />
                <Field label="Phone" value={emp.phone} />
                <Field label="Current Address" value={emp.currentAddress} />
                <Field label="Permanent Address" value={emp.permanentAddress} />
                <Field label="District" value={emp.district} />
                <Field label="Division" value={emp.division} />
              </Section>

              <hr className="border-gray-100" />

              <Section icon={<User size={15} />} title="Personal Information">
                <Field label="Gender" value={emp.gender} />
                <Field label="Date of Birth" value={fmtDate(emp.dateOfBirth)} />
                <Field label="Blood Group" value={emp.bloodGroup} />
                <Field label="Marital Status" value={emp.maritalStatus} />
                <Field label="Religion" value={emp.religion} />
                <Field label="Nationality" value={emp.nationality} />
                <Field label="National ID" value={emp.nationalId} />
                <Field label="Passport No." value={emp.passportNo} />
              </Section>
            </>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-3 border-t border-gray-100 flex items-center justify-between shrink-0">
          <span className="inline-flex items-center gap-1.5 text-xs text-gray-400">
            <CalendarDays size={13} />
            {emp?.createdAt ? `Joined system on ${fmtDate(emp.createdAt)}` : ""}
          </span>
          <button
            onClick={onClose}
            className="px-4 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
