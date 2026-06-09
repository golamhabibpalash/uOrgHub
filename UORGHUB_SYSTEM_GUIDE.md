# uOrgHub ERP — Complete System Guide

**Version**: 1.0  
**Audience**: Client / System Users  
**Updated**: June 2026

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Initial Setup Flow](#2-initial-setup-flow)
3. [Module Map & Navigation](#3-module-map--navigation)
4. [Auth & Access Control](#4-auth--access-control)
5. [HR Module](#5-hr-module)
6. [Accounts Module](#6-accounts-module)
7. [Inventory Module](#7-inventory-module)
8. [Procurement Module](#8-procurement-module)
9. [Projects Module](#9-projects-module)
10. [Settings Module](#10-settings-module)
11. [Cross-Module Business Flows](#11-cross-module-business-flows)
12. [Dashboard & Reporting](#12-dashboard--reporting)
13. [Activity Checklists by Role](#13-activity-checklists-by-role)

---

## 1. System Overview

uOrgHub is a **modular ERP system** designed for civil construction companies. It consists of 6 core business modules, an authentication layer, and a settings module — all integrated into a single web application.

### Architecture at a Glance

```
┌─────────────────────────────────────────────────────┐
│                   WEB BROWSER                        │
│           (React + TypeScript Frontend)              │
├─────────────────────────────────────────────────────┤
│                    API LAYER                          │
│           (ASP.NET Core Web API / C#)                │
├──────────┬──────────┬──────────┬──────────┬─────────┤
│   HR     │ Accounts │Inventory│Procurement│ Projects│
├──────────┴──────────┴──────────┴──────────┴─────────┤
│                   PostgreSQL 16                       │
│              (Single Database — orgHub)               │
└─────────────────────────────────────────────────────┘
```

### Technology Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, TypeScript, Vite, Tailwind CSS, TanStack Query, Zustand |
| Backend | .NET 8, ASP.NET Core Web API, MediatR (CQRS), Entity Framework Core |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer Tokens + Refresh Tokens, 2FA (OTP via Email/SMS) |
| Container | Docker Compose for local PostgreSQL |

### What You Can Do with Each Module

| Module | Purpose |
|--------|---------|
| **HR** | Manage employees, departments, attendance, leave, payroll, recruitment, performance reviews, training |
| **Accounts** | Full double-entry accounting: invoices, bills, payments, bank accounts, budgets, journal entries, tax management |
| **Inventory** | Track items, variants, warehouses, stock levels, stock movements (receipts, issues, transfers, adjustments) |
| **Procurement** | Purchase requisitions → RFQs → Quotations → Purchase Orders → Goods Received Notes |
| **Projects** | Full project lifecycle: WBS, BOQ, milestones, daily progress reports, material requests, expenses, drawings, RFIs, submittals, QA checklists, NCRs, safety incidents, RA bills |

---

## 2. Initial Setup Flow

When you first access the system, follow this sequence:

### Step 1: System Boot & Company Setup

The first page you see is the **Company Setup Wizard**. This runs only once.

**What you need to provide:**
- Company Name, Address, Phone, Email
- Tax ID (TIN/BIN)
- Logo (optional)
- Tagline
- Currency (default: BDT)
- Time Zone (default: Asia/Dhaka)

**What happens behind the scenes:**
- A `companies` record is created (table prefix: none — shared entity)
- Default theme settings are applied
- The system becomes fully operational

### Step 2: Create Admin User & Roles

After company setup, the **first user is created as a Super Admin**. This user has every permission and can:
- Create additional users
- Define roles (e.g., HR Manager, Accountant, Project Manager, Site Engineer)
- Assign permissions to each role
- Configure system settings

### Step 3: Configure System Settings

Navigate to **Settings → System Settings** to configure:
- Application-wide configuration values (key-value pairs, categorized)
- Validation rules per entity (field-level rules like required, max length, regex)

### Step 4: Set Up Theming (Optional)

Navigate to **Admin → Theme Settings** to customize:
- Primary color (with auto-generated shades)
- Sidebar background & text colors
- Dark mode toggle
- Predefined palettes available

### Step 5: Complete Initial Setup Per Module

Each module requires initial master data before daily operations:

| Module | Prerequisite Setup |
|--------|-------------------|
| **HR** | Departments → Designations → Salary Grades → Work Schedules → Shifts → Leave Types |
| **Accounts** | Account Groups → Fiscal Years → Chart of Accounts → Cost Centers → Tax Rates → Bank Accounts |
| **Inventory** | Inventory Types → Categories → Units of Measure → Attributes → Warehouses |
| **Procurement** | Vendors (or sync with Accounts vendors) |
| **Projects** | Project Categories → Clients |

---

## 3. Module Map & Navigation

### Sidebar Navigation Structure

Once logged in, the left sidebar shows dynamic menus based on your permissions:

```
📊 Dashboard
  └─ Home Dashboard

👥 HR
  ├─ HR Dashboard
  ├─ Departments
  ├─ Designations
  ├─ Employees
  ├─ Organogram
  ├─ Attendance
  ├─ Leave Management
  ├─ Payroll
  ├─ Recruitment
  └─ Performance

💰 Accounts
  ├─ Accounts Dashboard
  ├─ Account Groups
  ├─ Fiscal Years
  ├─ Chart of Accounts
  ├─ Journal Entries
  ├─ Cost Centers
  ├─ Tax Rates
  ├─ Bank Accounts
  ├─ Customers
  ├─ Invoices
  ├─ Vendors
  ├─ Bills
  ├─ Payments
  └─ Budgets

📦 Inventory
  ├─ Inventory Dashboard
  ├─ Types
  ├─ Categories
  ├─ Units of Measure
  ├─ Attributes
  ├─ Items
  ├─ Item Variants
  ├─ Warehouses
  ├─ Stock Balances
  └─ Stock Transactions

📋 Procurement
  ├─ Procurement Dashboard
  ├─ Vendors
  ├─ Purchase Requisitions
  ├─ Request for Quotations (RFQ)
  ├─ Vendor Quotations
  ├─ Purchase Orders
  └─ Goods Received Notes (GRN)

🏗️ Projects
  ├─ Projects Dashboard
  ├─ Clients
  ├─ Projects List
  └─ (Per-project sub-navigation when viewing a project)
       ├─ Overview / Dashboard
       ├─ WBS (Work Breakdown Structure)
       ├─ BOQ (Bill of Quantities)
       ├─ Team
       ├─ Milestones
       ├─ Daily Progress Reports (DPR)
       ├─ Material Requests
       ├─ Expenses
       ├─ Drawings
       ├─ RFIs
       ├─ Submittals
       ├─ Resource Allocations
       ├─ QA Checklists
       ├─ NCRs (Non-Conformance Reports)
       ├─ Safety Incidents
       └─ RA Bills (Running Account Bills)

⚙️ Settings
  ├─ System Settings
  └─ Validation Rules

🔧 Admin (requires special permission)
  ├─ Users
  ├─ Roles
  ├─ Access Logs
  ├─ Company Settings
  └─ Theme Settings
```

---

## 4. Auth & Access Control

### Login & Authentication Flow

```
1. Go to login page → enter username & password
2. If 2FA is enabled → enter OTP code (sent via email/SMS)
3. On success → JWT token issued → redirected to Dashboard
4. Token auto-refreshes when expired (background, seamless)
```

### Role-Based Access Control

Two layers of security:

1. **Authentication** — "Who you are" (username/password)
2. **Authorization** — "What you can do" (claims/permissions)

Claims are hierarchical strings, e.g.:
- `HR.Employees.View` — Can view employee list
- `HR.Employees.Create` — Can create employees
- `HR.LeaveRequests.Approve` — Can approve leave requests

Each role bundles many claims. A user can have multiple roles.

### Self-Service Permissions

Every employee can:
- View their own profile
- Edit their own profile
- Submit/view their own leave
- View their own attendance
- View their own payslip

### Password Management

- Forgot password: 3-step wizard (email → OTP → new password)
- Change password: available from profile page
- Admin can force password change on next login
- Account lockout after failed attempts

---

## 5. HR Module

### Core Organization Structure

```
Company
  └── Department (e.g., "Site Operations", "Finance", "HR")
        └── Designation (e.g., "Site Engineer", "Project Manager")
              └── Employee
```

### Master Data Setup Order

| Step | What to Create | Purpose |
|------|---------------|---------|
| 1 | **Departments** | Organizational units (can have parent/child hierarchy) |
| 2 | **Designations** | Job titles linked to departments and salary grades |
| 3 | **Salary Grades** | Pay bands (min/max salary, basic percentage) |
| 4 | **Salary Components** | Allowances & deductions (e.g., House Rent, Medical, Tax) |
| 5 | **Work Schedules** | Standard working hours per work type |
| 6 | **Shifts** | Specific timing within a work schedule (e.g., Day Shift, Night Shift) |
| 7 | **Leave Types** | Annual Leave, Sick Leave, Maternity Leave, etc. |
| 8 | **Overtime Rules** | Overtime calculation rules (multiplier, max hours) |

### Employee Lifecycle

```
1. RECRUITMENT (optional path)
   Create Job Posting → Receive Applications →
   Shortlist → Interview → Hire

2. ONBOARDING
   Create Employee → Assign to Department/Designation →
   Create Employee Contract → Upload Documents →
   Set Emergency Contacts → Assign Assets →
   Complete Onboarding Checklist

3. ACTIVE EMPLOYMENT
   Daily attendance logging → Leave requests & approvals →
   Expense requests → Training enrollments →
   Performance goals & reviews

4. OFFBOARDING
   Set Last Working Day → Update employment status →
   Return assets → Complete exit formalities
```

### Attendance & Leave

**Attendance tracking:**
- Employees log check-in/check-out via web or manual entry
- Status: Present, Late, Absent, Half-Day, Leave
- Overtime calculated automatically per rules
- Rosters can be pre-assigned per employee per date

**Leave management:**
- Employees submit leave requests
- Multi-level approval workflow
- Leave balances auto-calculated per leave type per year
- Carry-forward rules configurable per leave type

### Payroll Cycle

```
1. Define Payroll Cycle (monthly) → select month & year
2. System auto-calculates:
   - Basic salary from grade
   - Allowances & deductions from salary structure
   - Overtime pay from attendance logs
   - Tax as per configured rules
   - Leave deductions if applicable
3. Generate Payroll Entries (one per employee)
4. Review, adjust if needed
5. Process payroll → marks as completed
6. Generate payslips per employee
```

### Performance Management

- **Review Cycles**: Quarterly, Half-Yearly, or Yearly
- **Goals**: Set per employee with KPIs, target values, weight
- **Performance Reviews**: Self-review, Manager review, Peer review, Subordinate review
- **Feedback**: 360-degree feedback requests
- **Training**: Programs enrollment, completion tracking

### Organogram

Visual org chart showing the hierarchy:
- Departments → Managers → Employees
- Self-referencing: Employees report to managers (also employees)

---

## 6. Accounts Module

### Financial Chart of Accounts Structure

```
ACCOUNT GROUPS (5 standard types)
  ├── Asset (e.g., Current Assets, Fixed Assets)
  ├── Liability (e.g., Current Liabilities, Long-term)
  ├── Equity
  ├── Income / Revenue
  └── Expense

CHART OF ACCOUNTS (GL Accounts)
  ├── 1001 Cash in Hand (Asset)
  ├── 1002 Cash at Bank (Asset)
  ├── 1100 Accounts Receivable (Asset)
  ├── 2100 Accounts Payable (Liability)
  ├── 4001 Project Revenue (Income)
  ├── 5001 Material Cost (Expense)
  └── ... (your complete GL)
```

### Standard Accounting Workflows

**Accounts Receivable (getting paid by clients):**
```
1. Add Customer
2. Create Invoice (select customer, add line items with revenue accounts & taxes)
3. Post Invoice → Auto-creates JE: DR AR, CR Revenue, CR VAT Payable
4. Record Payment → Allocate to invoice → Auto-creates JE: DR Bank, CR AR
5. Invoice status becomes "Paid"
```

**Accounts Payable (paying suppliers):**
```
1. Add Vendor
2. Create Bill (select vendor, add line items with expense accounts & taxes)
3. Approve Bill → Auto-creates JE: DR Expense, DR VAT Input, CR AP
4. Record Payment → Allocate to bill → Auto-creates JE: DR AP, CR Bank
5. Bill status becomes "Paid"
```

**Manual Journal Entries:**
```
1. Create JE with date, description, reference
2. Add lines (each line: GL account, either Debit OR Credit amount)
3. Ensure Total Debits = Total Credits
4. Post → Updates all GL account balances
5. Cannot edit posted entries — must cancel & recreate
```

**Budgeting:**
```
1. Create Budget → link to Fiscal Year and optional Cost Center
2. Add budget lines per GL account, per period (Annual or Monthly)
3. Approve → becomes Active
4. Actuals vs Budget comparison shown on variance reports
```

### Fiscal Year Management

| Status | Meaning |
|--------|---------|
| **Pending** | Future year, not yet active |
| **Active** | Current year — only one can be active at a time |
| **Closed** | Past year — no new transactions allowed |

---

## 7. Inventory Module

### Master Data Setup

| Step | What to Create | Example |
|------|---------------|---------|
| 1 | **Inventory Types** | "Raw Material", "Finished Product", "Equipment" |
| 2 | **Categories** | "Cement", "Steel", "Electrical", "Plumbing" (hierarchical under types) |
| 3 | **Units of Measure** | "Piece", "Kg", "Ton", "Meter", "Liter" |
| 4 | **Attribute Definitions** | "Size", "Grade", "Color", "Brand" (with data types) |
| 5 | **Warehouses** | "Main Store", "Site A Store", "Site B Store" |

### Item & Variant Management

```
Item (Base Product)
  ├── "Cement"
  │     ├── Variant: "Portland Cement 50kg" (SKU: CEM-PC-50)
  │     └── Variant: "Portland Cement 25kg" (SKU: CEM-PC-25)
  ├── "Steel Rod"
  │     ├── Variant: "10mm Steel Rod" (SKU: STEEL-10MM)
  │     └── Variant: "16mm Steel Rod" (SKU: STEEL-16MM)
  └── ...
```

**Item properties:** Base name, item code, type, category, unit of measure, brand, manufacturer, reorder level, standard cost

**Variant properties:** SKU, variant name, barcode, cost price, selling price, attribute values (defined by attribute definitions)

### Stock Management

**Stock Balances**
- Per variant per warehouse
- Quantity on Hand, Quantity Reserved, Quantity Available (computed)
- Auto-updated on stock transactions

**Stock Transaction Types:**

| Type | Description | Effect on Stock |
|------|-------------|-----------------|
| **Receipt** | Goods received into warehouse | + Quantity |
| **Issue** | Goods taken out of warehouse | − Quantity |
| **Transfer** | Move between warehouses | − from source, + to destination |
| **Adjustment** | Physical count correction | +/− as needed |

### Integration with Other Modules

- **Procurement**: GRN items auto-update stock balances
- **Projects**: Material requests reference inventory items
- **Accounts**: Stock transactions that involve cost can feed into accounting (COGS)

---

## 8. Procurement Module

### Full Procurement Lifecycle

```
PURCHASE REQUISITION (PR)
  Employee requests items → Status: Draft → Submit → Status: Pending
  → Approver reviews → Approved ✓ or Rejected ✗
  ↓ (If approved)

REQUEST FOR QUOTATION (RFQ)
  Procurement officer creates RFQ from PR items
  Send to multiple vendors → Vendors submit quotations

VENDOR QUOTATION
  Compare quotations from different vendors
  Select best quotation

PURCHASE ORDER (PO)
  Create PO from selected quotation
  Send to vendor → Vendor delivers goods

GOODS RECEIVED NOTE (GRN)
  Receive goods at warehouse → Match against PO items
  Record accepted/rejected quantities
  Auto-updates inventory stock
```

### Key Statuses

| Document | Lifecycle |
|----------|-----------|
| **PR** | Draft → Pending → Approved / Rejected → Cancelled |
| **RFQ** | Draft → Sent → Closed / Cancelled |
| **Quotation** | Draft → Submitted → Accepted / Rejected / Expired |
| **PO** | Draft → Sent → Confirmed → PartiallyReceived → Received → Cancelled |
| **GRN** | Draft → Confirmed → Cancelled |

### Procurement + Inventory Integration

When a GRN is confirmed:
1. Stock balance for the received items increases
2. The PO item's received quantity updates
3. If PO is fully received, status changes to "Received"

---

## 9. Projects Module

### Project Lifecycle

```
1. INQUIRY / ESTIMATION
   Client inquiry → Create Project (status: Inquiry/Estimation)
   → Add project category, client, initial budget

2. PLANNING
   Create Work Breakdown Structure (WBS) → hierarchical task breakdown
   Create Bill of Quantities (BOQ) → itemized cost estimation
   → Approve BOQ
   Set milestones → critical dates
   Allocate resources → team members, equipment, materials

3. EXECUTION (active construction)
   Daily Progress Reports (DPR) → daily work done, manpower, equipment, weather
   Material Requests → request items from inventory/stores
   Project Expenses → record costs (materials, labor, equipment, subcontractor)
   Drawings Management → upload, revise, track drawing versions
   RFIs (Request for Information) → raise & respond to technical queries
   Submittals → contractor submissions for consultant/owner approval
   QA Checklists → inspection & quality checks
   NCRs (Non-Conformance Reports) → track quality deviations & corrective actions
   Safety Incidents → log & investigate

4. BILLING
   RA Bills (Running Account Bills) → periodic billing based on work done
   → BOQ quantities measured → RA Bill submitted → Certified → Paid

5. CLOSEOUT
   → Complete all deliverables
   → Final RA Bill
   → Project status: Completed
```

### Project Team Roles

| Role | Responsibility |
|------|---------------|
| **Project Manager** | Overall project oversight |
| **Site Engineer** | Daily execution & DPR |
| **QA/QC Engineer** | Quality checklists & NCRs |
| **Quantity Surveyor** | BOQ, RA bills, material quantities |
| **Safety Officer** | Safety inspections & incident reporting |
| **Store Keeper** | Material requests & stock |

### WBS (Work Breakdown Structure)

Hierarchical decomposition of project work:

```
Project
├── Phase 1: Foundation
│     ├── Task 1.1: Excavation
│     │     ├── Sub-task: Marking & Layout
│     │     └── Sub-task: Earthwork
│     ├── Task 1.2: Concrete
│     └── Task 1.3: Reinforcement
├── Phase 2: Structure
│     ├── Task 2.1: Column
│     └── Task 2.2: Slab
└── ...
```

Each WBS item has: code, title, level, dates, duration, completion %, status.

### BOQ (Bill of Quantities)

The financial backbone of the project:

```
BOQ Header
├── BOQ Item 1: "Earthwork Excavation"
│     ├── Item from Inventory (e.g., "Excavation - per m³")
│     ├── Specification (e.g., "Depth up to 3m")
│     ├── Unit of Measure: m³
│     ├── Estimated Qty: 5,000
│     ├── Unit Rate: 150 BDT
│     ├── Estimated Amount: 750,000 BDT
│     └── Actual Qty (updated during execution)
├── BOQ Item 2: "Reinforcement Steel"
│     ├── Item: "10mm Steel Rod"
│     ├── Unit: Kg
│     ├── Estimated Qty: 25,000
│     └── ...
└── ...
```

### RA Bills (Running Account Bills)

Periodic billing process:
```
1. Measure work done against BOQ items
2. Calculate: Current Qty × Rate = Current Amount
3. Add all items → Gross Amount
4. Apply deductions: Retention (commonly 5-10%), taxes, previous payments
5. Net Amount = Amount due this period
6. Submit → Certify → Mark as Paid
```

### Cross-Module Connections

- **Projects → HR**: Team members are Employees from HR
- **Projects → Inventory**: Material requests reference Inventory Items
- **Projects → Accounts**: RA bills connect to Accounts Invoicing; Expenses flow to Accounts
- **Projects → Procurement**: Material requests can trigger Purchase Requisitions

---

## 10. Settings Module

### System Settings

Key-value configuration store. Settings are categorized:

| Category | Examples |
|----------|----------|
| **General** | Company name, tax rates, notification preferences |
| **HR** | Auto-attendance cutoff time, leave year start month |
| **Payroll** | Tax slab rates, overtime rates |
| **Accounts** | Default AR/AP accounts, invoice numbering pattern |
| **Projects** | Retention percentage, billing cycle |

### Validation Rules

Define dynamic field-level validation rules without code changes:

| Rule Type | Example |
|-----------|---------|
| **Required** | Employee phone number is mandatory |
| **MaxLength** | Department name max 100 characters |
| **MinLength** | Password minimum 8 characters |
| **Regex** | Phone must match pattern: 01[3-9]\d{8} |
| **Range** | Salary between min and max of grade |

Rules can be assigned to specific entity types and fields, with severity (Error/Warning/Info).

---

## 11. Cross-Module Business Flows

### Flow 1: Employee Hire to Payroll

```
HR MODULE                          ACCOUNTS MODULE
─────────                          ───────────────
Create Employee
  └─ Create User Account (optional)
  └─ Assign Department/Designation
  └─ Set Salary Structure
  └─ Upload Documents
  └─ Assign Assets
       │
Daily Attendance                   
  └─ Log check-in/check-out       
  └─ Auto-calculate OT             
       │                            
Leave Request                      
  └─ Submit → Approve              
  └─ Update leave balance          
       │                            
Payroll Cycle Start                
  └─ Auto-calculate entries        
  └─ Gross, deductions, net        
  └─ Generate payslips             
       │                             
Payroll Processing                 
  └─ Approved for payment          
       │                             ──► Journal Entry:
                                         DR Salary Expense
                                         CR Salary Payable

                                          ──► Payment:
                                         DR Salary Payable
                                         CR Bank Account
```

### Flow 2: Project to Procurement to Inventory to Accounts

```
PROJECT MODULE         PROCUREMENT MODULE    INVENTORY MODULE      ACCOUNTS MODULE
─────────────          ─────────────────     ────────────────      ───────────────
Project Created
  └─ WBS created
  └─ BOQ created
       │
Material Request
  └─ Request items          │
       │                    │
       └─────────────────► Purchase Requisition
                            └─ PR Approved
                            └─ RFQ Created → Quotations
                            └─ PO Issued
                                 │
                            Goods Received      ──► Stock Updated
                            ──► GRN Confirmed        (Quantity +)        ──► JE: DR Inventory
                                                                              CR GRN Accrual
                                 │
                            Vendor Bill (in AP)
                            ──► PO Matched                               ──► JE: DR Expense
                                                                              CR AP
                                                                              ──► Payment:
                                                                              DR AP
                                                                              CR Bank
```

### Flow 3: Project RA Bill to Accounts

```
PROJECT                                   ACCOUNTS
────────                                   ────────
RA Bill Created
  └─ BOQ items measured
  └─ Current qty × rate
  └─ Deductions applied
       │
RA Bill Submitted
  └─ Certified by consultant
  └─ Marked as paid
       │
       └─────────────────────────────────► Invoice Created (Customer = Client)
                                           └─ Invoice Posted
                                           └─ Payment Received
                                           └─ JE: DR Bank
                                                  CR AR
```

### Flow 4: Daily Operations Cycle

```
MORNING
  ├── Employees check in (attendance)
  ├── Site Engineer submits DPR for yesterday
  └── Store Keeper processes material requests

DURING THE DAY
  ├── New purchase requisitions raised
  ├── Goods received at store (GRN)
  ├── Project expenses recorded
  ├── RFIs raised or responded to
  ├── QA inspections conducted
  └── Leave requests submitted & approved

EVENING
  ├── Employees check out (attendance)
  ├── DPR for today updated
  └── Stock transactions finalized

MONTHLY / PERIODIC
  ├── Payroll cycle processed
  ├── RA bills submitted for certification
  ├── Supplier/vendor payments made
  ├── Customer invoice payments collected
  ├── Financial reports reviewed
  └── Budget vs actual variance analyzed
```

---

## 12. Dashboard & Reporting

### Home Dashboard

Role-based KPIs and summaries:

| Role | Sees |
|------|------|
| **Admin / CEO** | All modules: pending approvals, financial summary, project status, employee count |
| **HR Manager** | Employee count, pending leave, attendance summary, payroll status |
| **Accountant** | Receivables, payables, bank balance, overdue invoices, budget variance |
| **Project Manager** | Their project status, milestones, DPRs, material requests pending |
| **Site Engineer** | Today's DPR, assigned tasks, QA items |
| **Store Keeper** | Low stock alerts, pending GRNs, stock transactions today |
| **Procurement Officer** | Pending PRs, active RFQs, PO status |

### Export Capabilities

Every data grid has an **Export** button supporting:
- **XLSX** (Excel)
- **CSV** (Comma Separated Values)

Exportable modules: Departments, Designations, Employees, Attendance, Leave, Payroll, all Accounts entities, all Inventory entities, all Procurement entities, all Projects entities, Users, Roles, Access Logs.

--- 

## 13. Activity Checklists by Role

### HR Manager

| Frequency | Activities |
|-----------|-----------|
| **Daily** | Approve/review leave requests; Review attendance exceptions |
| **Weekly** | Process new employee onboarding; Review pending recruitment status |
| **Monthly** | Run payroll cycle; Review training completions; Update employee statuses |
| **Quarterly** | Initiate performance review cycles; Review goal progress |
| **Yearly** | Set leave type allocations; Review salary grades; Plan training programs |
| **As needed** | Add/edit departments, designations; Create job postings; Interview scheduling |

### Accountant / Finance Manager

| Frequency | Activities |
|-----------|-----------|
| **Daily** | Post invoices & bills; Record payments & allocations; Reconcile bank transactions |
| **Weekly** | Review receivables aging; Review payables aging; Post manual journal entries |
| **Monthly** | Run trial balance; Post depreciation & accruals; Review budget vs actual; Close month |
| **Quarterly** | Tax filings preparation; Financial statement review |
| **Yearly** | Close fiscal year; Open new fiscal year; Set opening balances; Create annual budget |
| **As needed** | Add/edit chart of accounts; Add customers/vendors; Configure tax rates |

### Project Manager

| Frequency | Activities |
|-----------|-----------|
| **Daily** | Review DPRs; Approve material requests; Review site progress |
| **Weekly** | Review project expenses; Check milestone progress; Review RFIs & submittals |
| **Monthly** | Prepare RA bills; Review project budget; Team performance review |
| **As needed** | Create/edit WBS; Approve BOQ; Add project team members; Review NCRs & safety incidents |

### Site Engineer

| Frequency | Activities |
|-----------|-----------|
| **Daily** | Submit DPR (work done, manpower, equipment, weather) |
| **Weekly** | Update WBS completion %; Raise material requests |
| **As needed** | Raise RFIs; Submit submittals; Participate in QA inspections; Report NCRs & safety incidents |

### Store Keeper

| Frequency | Activities |
|-----------|-----------|
| **Daily** | Process GRN for received materials; Issue materials against requests; Update stock transactions |
| **Weekly** | Review stock balances; Identify low-stock items; Cycle count adjustments |
| **Monthly** | Full stock count; Adjust discrepancies |
| **As needed** | Transfer stock between warehouses; Add new inventory items/variants |

### Procurement Officer

| Frequency | Activities |
|-----------|-----------|
| **Daily** | Review new purchase requisitions; Send RFQs to vendors |
| **Weekly** | Evaluate quotations; Issue purchase orders; Follow up on PO deliveries |
| **Monthly** | Vendor performance review; Update vendor information |
| **As needed** | Add/edit vendors; Process urgent procurement requests |

### System Administrator

| Frequency | Activities |
|-----------|-----------|
| **As needed** | Create/manage users; Assign roles & permissions; Review access logs; Configure system settings; Manage company profile; Update theme; Force logout compromised users; Unlock locked accounts |

---

*End of document — uOrgHub ERP v1.0*
