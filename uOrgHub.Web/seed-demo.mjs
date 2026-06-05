// One-off demo data seeder for brochure screenshots. Talks to the running API.
const BASE = "http://localhost:5177/api/v1";
let token = "";

async function login() {
  const r = await fetch(`${BASE}/auth/login`, {
    method: "POST", headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username: "admin", password: "Admin@12345" }),
  });
  const j = await r.json();
  token = j.data.accessToken;
  console.log("logged in:", !!token);
}

async function post(path, body) {
  const r = await fetch(`${BASE}/${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
    body: JSON.stringify(body),
  });
  const j = await r.json().catch(() => ({}));
  if (!r.ok || j.success === false) {
    console.log(`  ! ${path}: ${r.status} ${j.message ?? ""}`);
    return null;
  }
  return j.data;
}

const iso = (y, m, d) => new Date(Date.UTC(y, m - 1, d)).toISOString();
const now = new Date();
const thisYear = now.getUTCFullYear();
const thisMonth = now.getUTCMonth() + 1;

async function run() {
  await login();

  // ---- Departments ----
  console.log("Departments...");
  const deptDefs = [
    ["Civil Construction", "CIV", "Operations"],
    ["Engineering & Design", "ENG", "Technical"],
    ["Finance & Accounts", "FIN", "Finance"],
    ["Human Resources", "HR", "HR"],
    ["Procurement", "PROC", "Operations"],
    ["Administration", "ADM", "Administrative"],
  ];
  const dept = {};
  for (const [name, code, type] of deptDefs) {
    const d = await post("departments", { name, code, type, isActive: true });
    if (d) dept[code] = d.id;
  }

  // ---- Salary Grades ----
  console.log("Salary grades...");
  const gradeDefs = [
    ["G1", "Executive / Director", 250000, 500000],
    ["G2", "Senior Management", 150000, 250000],
    ["G3", "Mid-Level", 80000, 150000],
    ["G4", "Junior / Officer", 40000, 80000],
    ["G5", "Entry Level", 20000, 40000],
  ];
  const grade = {};
  for (const [gradeCode, name, minSalary, maxSalary] of gradeDefs) {
    const g = await post("payroll/salary-grades", { gradeCode, name, minSalary, maxSalary, description: `${name} pay band`, isActive: true });
    if (g) grade[gradeCode] = g.id;
  }

  // ---- Salary Components ----
  console.log("Salary components...");
  const compDefs = [
    ["Basic Salary", "BASIC", "BasicSalary", "PercentageOfGross", 60, false],
    ["House Rent Allowance", "HRA", "HouseRentAllowance", "PercentageOfBasic", 50, false],
    ["Medical Allowance", "MED", "MedicalAllowance", "Fixed", 1500, false],
    ["Transport Allowance", "TRP", "TransportAllowance", "Fixed", 2500, true],
    ["Provident Fund", "PF", "PF", "PercentageOfBasic", 10, false],
    ["Income Tax (TDS)", "TAX", "Tax", "Fixed", 2000, false],
  ];
  for (const [name, code, componentType, calculationType, defaultValue, isTaxable] of compDefs) {
    await post("payroll/salary-components", {
      name, code, componentType, calculationType, defaultValue, isTaxable,
      isFixed: calculationType === "Fixed", isActive: true, sortOrder: 0, description: name,
    });
  }

  // ---- Designations ----
  console.log("Designations...");
  const desigDefs = [
    ["Managing Director", "MD", 1, "ADM", "G1"],
    ["Project Director", "PD", 1, "CIV", "G1"],
    ["Chief Engineer", "CE", 2, "ENG", "G2"],
    ["Senior Civil Engineer", "SCE", 3, "ENG", "G3"],
    ["Site Engineer", "SE", 4, "CIV", "G4"],
    ["Quantity Surveyor", "QS", 4, "CIV", "G4"],
    ["Finance Manager", "FM", 2, "FIN", "G2"],
    ["Senior Accountant", "SA", 3, "FIN", "G3"],
    ["HR Manager", "HRM", 2, "HR", "G2"],
    ["HR Officer", "HRO", 4, "HR", "G4"],
    ["Procurement Officer", "PO", 4, "PROC", "G4"],
    ["Site Supervisor", "SS", 5, "CIV", "G5"],
  ];
  const desig = {};
  for (const [name, code, level, deptCode, gradeCode] of desigDefs) {
    const d = await post("designations", {
      name, code, level, isActive: true,
      departmentId: dept[deptCode], salaryGradeId: grade[gradeCode],
    });
    if (d) desig[code] = d.id;
  }

  // ---- Employees ----
  console.log("Employees...");
  const empDefs = [
    ["Rafiqul", "Islam", "MD", "ADM", "Male", "Permanent", 420000, iso(2015, 3, 1), "Dhaka", "Dhaka"],
    ["Tanvir", "Ahmed", "PD", "CIV", "Male", "Permanent", 310000, iso(2016, 7, 12), "Dhaka", "Dhaka"],
    ["Shahidul", "Karim", "CE", "ENG", "Male", "Permanent", 195000, iso(2017, 1, 9), "Chattogram", "Chattogram"],
    ["Nusrat", "Jahan", "SCE", "ENG", "Female", "Permanent", 120000, iso(2018, 9, 3), "Sylhet", "Sylhet"],
    ["Mahmud", "Hasan", "SE", "CIV", "Male", "Permanent", 62000, iso(2020, 2, 17), "Gazipur", "Dhaka"],
    ["Arif", "Hossain", "QS", "CIV", "Male", "Contract", 58000, iso(2021, 5, 22), "Narayanganj", "Dhaka"],
    ["Farhana", "Akter", "FM", "FIN", "Female", "Permanent", 180000, iso(2017, 11, 6), "Dhaka", "Dhaka"],
    ["Sabbir", "Rahman", "SA", "FIN", "Male", "Permanent", 95000, iso(2019, 8, 14), "Khulna", "Khulna"],
    ["Naila", "Sultana", "HRM", "HR", "Female", "Permanent", 165000, iso(2018, 4, 2), "Dhaka", "Dhaka"],
    ["Imran", "Khan", "HRO", "HR", "Male", "Permanent", 55000, iso(2022, 1, 10), "Rajshahi", "Rajshahi"],
    ["Sadia", "Noor", "PO", "PROC", "Female", "Permanent", 60000, iso(2021, 10, 25), "Dhaka", "Dhaka"],
    ["Jamal", "Uddin", "SS", "CIV", "Male", "Daily", 28000, iso(thisYear, thisMonth, 3), "Bogura", "Rajshahi"],
    ["Rumana", "Begum", "HRO", "HR", "Female", "Contract", 52000, iso(thisYear, thisMonth, 8), "Dhaka", "Dhaka"],
  ];
  let n = 1;
  const emps = [];
  for (const [first, last, dCode, deptCode, gender, empType, basic, joining, district, division] of empDefs) {
    const e = await post("employees", {
      employeeCode: `EMP-${String(n).padStart(4, "0")}`,
      firstName: first, lastName: last,
      email: `${first.toLowerCase()}.${last.toLowerCase()}@unitybd.com`,
      phone: `+88017${String(10000000 + n * 137).slice(0, 8)}`,
      gender, religion: "Islam", maritalStatus: n % 3 === 0 ? "Single" : "Married",
      nationality: "Bangladeshi", district, division,
      joiningDate: joining, employmentType: empType, status: "Active",
      designationId: desig[dCode], departmentId: dept[deptCode],
      basicSalary: basic,
    });
    if (e) emps.push(e);
    n++;
  }

  // ---- Leave Types ----
  console.log("Leave types...");
  const ltDefs = [
    ["Annual Leave", "AL", 20, true, 5],
    ["Sick Leave", "SL", 14, true, 0],
    ["Casual Leave", "CL", 10, true, 0],
    ["Maternity Leave", "ML", 112, true, 0],
  ];
  const lt = {};
  for (const [name, code, days, paid, cf] of ltDefs) {
    const t = await post("leave/types", {
      name, code, totalDaysPerYear: days, maxConsecutiveDays: 30,
      minDaysNotice: 1, isPaidLeave: paid, maxCarryForwardDays: cf,
    });
    if (t) lt[code] = t.id;
  }

  // ---- Leave Requests ----
  console.log("Leave requests...");
  if (emps.length) {
    await post("leave/requests", { employeeId: emps[4].id, leaveTypeId: lt.AL, startDate: iso(thisYear, thisMonth, 12), endDate: iso(thisYear, thisMonth, 14), reason: "Family vacation" });
    await post("leave/requests", { employeeId: emps[7].id, leaveTypeId: lt.SL, startDate: iso(thisYear, thisMonth, 5), endDate: iso(thisYear, thisMonth, 6), reason: "Fever and rest" });
    await post("leave/requests", { employeeId: emps[9].id, leaveTypeId: lt.CL, startDate: iso(thisYear, thisMonth, 18), endDate: iso(thisYear, thisMonth, 18), reason: "Personal work" });
  }

  // ---- Payroll Cycle ----
  console.log("Payroll cycle...");
  await post("payroll/cycles", {
    year: thisYear, month: thisMonth,
    title: `Payroll - ${now.toLocaleString("en-US", { month: "long" })} ${thisYear}`,
    startDate: iso(thisYear, thisMonth, 1), endDate: iso(thisYear, thisMonth, 28),
  });

  // ---- Job Postings ----
  console.log("Job postings...");
  const jobDefs = [
    ["Senior Civil Engineer", "ENG", "SCE", "Lead structural design for high-rise projects."],
    ["Site Engineer", "CIV", "SE", "Supervise on-site construction activities."],
    ["Procurement Officer", "PROC", "PO", "Manage vendor sourcing and purchase orders."],
  ];
  for (const [title, deptCode, dCode, description] of jobDefs) {
    await post("recruitment/job-postings", {
      title, departmentId: dept[deptCode], designationId: desig[dCode],
      description, requirements: "B.Sc. with 3+ years relevant experience.",
      requiredCount: 2, experienceYearsMin: 3, experienceYearsMax: 7,
      status: "Published", postedDate: iso(thisYear, thisMonth, 1),
    });
  }

  // ---- Candidates ----
  console.log("Candidates...");
  const candDefs = [
    ["Mehedi", "Hasan", "mehedi.h@gmail.com", "Referral"],
    ["Tasnia", "Rahman", "tasnia.r@gmail.com", "LinkedIn"],
    ["Rakib", "Hossain", "rakib.h@gmail.com", "JobPortal"],
  ];
  for (const [firstName, lastName, email, source] of candDefs) {
    await post("recruitment/candidates", { firstName, lastName, email, phone: "+8801800000000", source });
  }

  console.log("Done. Employees created:", emps.length);
}

run().catch((e) => { console.error(e); process.exit(1); });
