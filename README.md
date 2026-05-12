# uOrgHub

A modular ERP (Enterprise Resource Planning) system built with **.NET 8** and **PostgreSQL**.

## Project Structure

```
uOrgHub/
├── uOrgHub.API/          # Web API (references all modules)
├── uOrgHub.Shared/       # Shared library (common code)
├── uOrgHub.HR/           # Human Resources module
├── uOrgHub.Accounts/     # Accounts/Finance module
├── uOrgHub.Inventory/    # Inventory management module
├── uOrgHub.Procurement/  # Procurement module
├── uOrgHub.Projects/     # Project management module
└── uOrgHub.Tests/        # Unit tests
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8 |
| API | ASP.NET Core Web API |
| Database | PostgreSQL 16 |
| Container | Docker Compose |

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop

### Setup Database

```bash
docker-compose up -d
```

This starts a PostgreSQL 16 instance on port `5432`.

### Run API

```bash
dotnet run --project uOrgHub.API
```

### Run Tests

```bash
dotnet test
```

## Database Configuration

| Setting | Value |
|---------|-------|
| Host | localhost |
| Port | 5432 |
| Database | orgHub |
| User | postgres |
| Password | Admin1234! |

## Module Overview

| Module | Description |
|--------|-------------|
| **HR** | Employee management, payroll, attendance |
| **Accounts** | Financial transactions, ledger, reporting |
| **Inventory** | Stock management, warehouse operations |
| **Procurement** | Purchase orders, supplier management |
| **Projects** | Project planning, tracking, milestones |

## License

MIT