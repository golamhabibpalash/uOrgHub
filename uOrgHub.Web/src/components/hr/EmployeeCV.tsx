import { forwardRef } from "react";
import { Employee } from "../../api/hr";

interface EmployeeCVProps {
  employee: Employee;
}

const BLOOD_GROUP_LABELS: Record<string, string> = {
  APositive: "A+", ANegative: "A-", BPositive: "B+", BNegative: "B-",
  ABPositive: "AB+", ABNegative: "AB-", OPositive: "O+", ONegative: "O-",
};

const fmtDate = (value?: string) => {
  if (!value) return "—";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return "—";
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
};

/**
 * A professional, print-ready CV/resume layout for an employee record. Rendered to a hidden ref and
 * serialised into a new print window by the caller, so it carries no interactive styling — it is
 * intended purely for print / save-as-PDF.
 */
const EmployeeCV = forwardRef<HTMLDivElement, EmployeeCVProps>(({ employee: e }, ref) => {
  const fullName = [e.firstName, e.middleName, e.lastName].filter(Boolean).join(" ") || e.fullName || "";
  const address = [e.currentAddress, e.district, e.division].filter(Boolean).join(", ") || "—";

  return (
    <div ref={ref} className="bg-white text-gray-900" style={{ fontSize: "13px" }}>
      {/* Header band */}
      <div className="flex items-center gap-5 border-b-4 border-primary-600 pb-5">
        <div className="flex items-center justify-center">
          <img
            src={e.profilePictureUrl}
            alt={fullName}
            className="h-28 w-28 object-cover rounded-full border-4 border-gray-100"
            onError={(ev) => (ev.currentTarget.style.display = "none")}
          />
        </div>
        <div className="flex-1">
          <h1 className="text-2xl font-bold uppercase tracking-wide text-primary-700">{fullName || "Employee"}</h1>
          {e.designationName && (
            <p className="text-base font-semibold text-gray-700 mt-1">{e.designationName}</p>
          )}
          {e.departmentName && (
            <p className="text-sm text-gray-500 mt-0.5">{e.departmentName}</p>
          )}
          {e.employeeCode && (
            <p className="text-xs text-gray-400 mt-1">Employee ID: {e.employeeCode}</p>
          )}
        </div>
        <span
          className={`self-start px-3 py-1 text-xs font-semibold rounded-full ring-1 ${
            e.status === "Active"
              ? "bg-green-50 text-green-700 ring-green-200"
              : e.status === "Inactive"
              ? "bg-yellow-50 text-yellow-700 ring-yellow-200"
              : e.status === "Terminated"
              ? "bg-red-50 text-red-700 ring-red-200"
              : "bg-gray-100 text-gray-600 ring-gray-200"
          }`}
        >
          {e.status ?? "N/A"}
        </span>
      </div>

      {/* Contact line */}
      <div className="flex flex-wrap gap-x-6 gap-y-1 py-3 text-xs text-gray-600 border-b border-gray-100">
        {e.email && (
          <span>
            <span className="font-semibold text-gray-700">Email:</span> {e.email}
          </span>
        )}
        {e.phone && (
          <span>
            <span className="font-semibold text-gray-700">Phone:</span> {e.phone}
          </span>
        )}
        <span>
          <span className="font-semibold text-gray-700">Address:</span> {address}
        </span>
      </div>

      {/* Professional summary */}
      <div className="mt-5">
        <h2 className="text-sm font-bold uppercase tracking-widest text-primary-700 border-b border-gray-200 pb-1 mb-2">
          Professional Summary
        </h2>
        <p className="text-sm leading-relaxed text-gray-700">
          {e.designationName ? "A dedicated professional currently serving as " : "A professional"}
          {e.designationName ? (
            <>
              <span className="font-semibold"> {e.designationName}</span>
              {e.departmentName ? (
                <>
                  {" "}in the <span className="font-semibold">{e.departmentName}</span> department
                </>
              ) : null}
            </>
          ) : null}
          {" "}of the organisation, employed on a <span className="font-semibold">{e.employmentType ?? "Permanent"}</span> basis
          {e.joiningDate ? ` since ${fmtDate(e.joiningDate)}` : ""}. Committed to contributing
          expertise, reliability and sustained performance to the team and the wider organisation.
        </p>
      </div>

      {/* Employment */}
      <div className="mt-5">
        <h2 className="text-sm font-bold uppercase tracking-widest text-primary-700 border-b border-gray-200 pb-1 mb-2">
          Employment Record
        </h2>
        <table className="w-full text-sm">
          <tbody>
            {[
              ["Designation", e.designationName],
              ["Department", e.departmentName],
              ["Employment Type", e.employmentType],
              ["Joining Date", fmtDate(e.joiningDate)],
              ["Confirmation Date", e.confirmationDate ? fmtDate(e.confirmationDate) : "—"],
              ["Reporting Manager", e.managerName],
              ["Basic Salary", e.basicSalary != null ? new Intl.NumberFormat("en-BD", { style: "currency", currency: "BDT", maximumFractionDigits: 0 }).format(e.basicSalary) : "—"],
            ].map(([label, value]) => (
              <tr key={label as string} className="border-b border-gray-50">
                <td className="py-1.5 pr-6 text-gray-500 w-48 font-medium">{label}</td>
                <td className="py-1.5 text-gray-800">{(value as string) || "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Personal details */}
      <div className="mt-5">
        <h2 className="text-sm font-bold uppercase tracking-widest text-primary-700 border-b border-gray-200 pb-1 mb-2">
          Personal Details
        </h2>
        <div className="grid grid-cols-2 gap-x-8 gap-y-1 text-sm">
          {[
            ["Date of Birth", fmtDate(e.dateOfBirth)],
            ["Gender", e.gender],
            ["Blood Group", e.bloodGroup ? (BLOOD_GROUP_LABELS[e.bloodGroup] ?? e.bloodGroup) : "—"],
            ["Marital Status", e.maritalStatus],
            ["Nationality", e.nationality],
            ["Religion", e.religion],
            ["National ID", e.nationalId],
            ["Passport No.", e.passportNo],
            ["Passport Expiry", fmtDate(e.passportExpiry)],
            ["Permanent Address", e.permanentAddress],
          ].map(([label, value]) => (
            <div key={label as string} className="py-0.5 flex gap-2">
              <span className="text-gray-500 font-medium w-36 shrink-0">{label}</span>
              <span className="text-gray-800">{(value as string) || "—"}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Signature */}
      <div className="mt-10 flex justify-end">
        <div className="text-center">
          <div className="border-t border-gray-700 pt-1 px-8 text-sm font-semibold">{fullName}</div>
          <div className="text-xs text-gray-500 mt-0.5">Signature</div>
        </div>
      </div>
    </div>
  );
});

EmployeeCV.displayName = "EmployeeCV";

export default EmployeeCV;
