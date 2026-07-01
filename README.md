# Expense Management System

<div align="center">

![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoftazure&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-Azure-CC2927?logo=microsoftsqlserver&logoColor=white)
![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub_Actions-2088FF?logo=githubactions&logoColor=white)

A production-deployed, full-stack **expense management platform** built with Angular 21 and ASP.NET Core (.NET 10) following Clean Architecture principles. Handles the complete expense lifecycle — from employee submission to manager approval — with real-time role enforcement, JWT security, file uploads, and a live analytics dashboard.

[Live App](https://zealous-coast-0c7b24903.7.azurestaticapps.net) · [Swagger UI](https://expensemgmt-api.azurewebsites.net/swagger) · [Backend API](https://expensemgmt-api.azurewebsites.net)

</div>

---

## Table of Contents

- [Live Demo](#-live-demo)
- [Project Highlights](#-project-highlights)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Role Permissions](#-role-permissions)
- [API Reference](#-api-reference)
- [Getting Started](#-getting-started)
- [Deployment](#-deployment)
- [CI/CD](#-cicd)
- [Author](#-author)

---

## 🌐 Live Demo

| Service | URL |
|---|---|
| **Frontend** (Angular SPA) | https://zealous-coast-0c7b24903.7.azurestaticapps.net |
| **Backend REST API** | https://expensemgmt-api.azurewebsites.net |
| **Interactive Swagger Docs** | https://expensemgmt-api.azurewebsites.net/swagger |

> Backend runs on **Azure App Service (Linux, F1)** connected to **Azure SQL Database**. Frontend is served via **Azure Static Web Apps** with global CDN. Both services auto-deploy via **GitHub Actions** on every push to `master`.

---

## ✨ Project Highlights

These are the technically interesting decisions made in this project — not just CRUD features:

- **Real-time role invalidation without a blocklist** — Rather than maintaining a token blocklist, the backend hooks into `OnTokenValidated` JWT events and queries the database on every authenticated request. The moment an admin demotes a user, their next API call returns `401` and the Angular interceptor silently logs them out. No polling, no expiry delay.

- **Clean Architecture with zero cross-layer leakage** — The `Domain` layer has no NuGet dependencies whatsoever. `Application` defines all interfaces; `Infrastructure` implements them. Controllers never touch EF Core directly.

- **Signals-first Angular** — The entire frontend uses Angular's Signals API (`signal()`, `computed()`, `effect()`) with `OnPush` change detection. No `BehaviorSubject` wrappers on mutable state.

- **Production secrets management** — Connection strings and JWT keys live exclusively in Azure App Service Application Settings. `appsettings.Production.json` is gitignored. The committed `appsettings.json` contains only empty placeholders.

- **Atomic receipt management** — Uploading a new receipt replaces the old file on disk and updates the record in a single operation. Deleting an expense also deletes its receipt file, preventing orphaned storage.

---

## 🚀 Features

### 🔐 Authentication & Security
- JWT Bearer tokens with **refresh token rotation** — short-lived access tokens, long-lived encrypted refresh tokens
- Passwords BCrypt-hashed before storage — never stored in plain text
- **Real-time role enforcement via `OnTokenValidated`** — role/status changes take effect on the next request, not at token expiry
- Refresh token cleared on role change to block silent re-authentication
- **Rate limiting**: 100 requests per minute per IP (fixed window)
- CORS restricted to the Angular origin only
- Input validation via Data Annotations on every DTO

### 📊 Dashboard *(Admin & Manager)*
- **6 KPI cards**: All-Time Total, This Month, This Year, Approved, Pending, Rejected — all currency-formatted
- **Monthly bar chart** — spending trend over the last 12 months (Chart.js)
- **Category doughnut chart** — spending breakdown by category
- **Budget utilization bars** — per-category actual spend vs monthly budget, red-highlighted when over budget
- **Recent activity feed** — last 5 expenses with status badges, amounts, submitter names, and categories

### 💸 Expense Management
- Employees submit expenses; Admins & Managers view and manage all
- **Approval workflow**: `Pending → Approved` or `Pending → Rejected` with an optional rejection reason recorded
- **Receipt upload** *(Admin/Manager)*: JPG, JPEG, PNG, PDF — max 5 MB — old file replaced on re-upload, deleted on expense removal
- Edit expenses in `Pending` status with inline receipt update
- Admin-only hard delete
- **Paginated list** with full-text search (title, description, date range) and filter by category and status
- Color-coded status badges (green / amber / red)

### 🗂️ Category Management *(Admin only)*
- Create, edit, activate, deactivate, soft-delete, and hard-delete categories
- Set **monthly and/or yearly budgets** per category — both are optional but at least one required when a budget is provided
- Paginated list with search and active/inactive filter

### 👥 User Management *(Admin only)*
- View all users with role, status, and registration date
- **Inline role change** via color-coded dropdown *(Admin = indigo, Manager = amber, Employee = teal)* — triggers immediate JWT invalidation for the affected user
- Activate / deactivate accounts (inactive users cannot log in)
- Delete users — with safeguards: cannot delete yourself or another Admin
- Paginated list with search and filter by role and status

---

## 🛠 Tech Stack

### Frontend

| Technology | Version | Purpose |
|---|---|---|
| Angular | 21 | SPA framework — standalone components, Signals, `OnPush` |
| Angular Material (MDC) | 21 | UI component library |
| TypeScript | 5 | Strongly typed language |
| Reactive Forms | — | Form handling, validators, cross-field validation |
| Chart.js + ng2-charts | 4 / 10 | Dashboard charts (bar, doughnut) |
| RxJS | 7.8 | Async streams, `forkJoin`, `switchMap`, `takeUntilDestroyed` |
| HTTP Interceptor | — | Auto-attaches JWT; global `401` → logout redirect |

### Backend

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | .NET 10 | RESTful Web API |
| Entity Framework Core | 9 | ORM — Code First, fluent configuration, migrations |
| SQL Server / Azure SQL | — | Relational data store |
| JWT Bearer Authentication | — | Stateless auth with custom `OnTokenValidated` hook |
| BCrypt.Net | — | Password hashing |
| Swagger / OpenAPI | — | Interactive API documentation |

### Infrastructure & DevOps

| Tool | Purpose |
|---|---|
| Azure App Service (F1 Linux) | Backend hosting |
| Azure SQL Database (Basic) | Production database |
| Azure Static Web Apps | Frontend hosting + global CDN |
| GitHub Actions | CI/CD — separate workflows for frontend and backend |
| Azure Service Principal | Scoped RBAC credentials for automated deployment |

---

## 🏗 Architecture

```
Expense-Management-System/
├── backend/
│   └── ExpenseManagement.API/
│       ├── ExpenseManagement.Domain/           # Entities, Enums — zero external dependencies
│       ├── ExpenseManagement.Application/      # Interfaces, DTOs, service contracts
│       ├── ExpenseManagement.Infrastructure/   # EF Core, Services, Repositories, Migrations
│       └── ExpenseManagement.API/              # Controllers, Middleware, Program.cs
│
└── frontend/
    └── expense-management-ui/src/app/
        ├── core/       # Auth service, HTTP interceptor, guards, models
        ├── features/   # Dashboard, Expenses, Categories, Users (all lazy-loaded routes)
        └── shared/     # Shared components and Angular Material imports
```

### Dependency flow

```
API  →  Application  →  Domain
         ↑
   Infrastructure
```

The `Domain` layer is the innermost ring — it has no NuGet references and no knowledge of the database or HTTP. The `Application` layer defines interfaces (`IExpenseService`, `IAuthService`, etc.) and DTOs. `Infrastructure` provides the concrete implementations. The `API` layer wires everything together through dependency injection in `Program.cs`.

### Key design decisions

| Decision | Rationale |
|---|---|
| **Clean Architecture layers** | Business rules never depend on delivery mechanisms (HTTP) or data access (EF Core) |
| **7 named authorization policies** | `AdminOnly`, `AdminOrManager`, `ManagerOnly`, `All`, etc. — applied per endpoint, not per role check scattered in code |
| **Signals + `OnPush`** | Fine-grained reactivity; no zone.js triggers on unrelated state changes |
| **`OnTokenValidated` instead of blocklist** | No shared blocklist state needed; enforcement is immediate and stateless |
| **`fileReplacements` for environments** | Single `environment.ts` in source; Angular CLI swaps in `environment.prod.ts` at build time — no runtime config injection needed |

---

## 🔑 Role Permissions

| Feature | Employee | Manager | Admin |
|---|---|---|---|
| Submit expense | ✅ | ✅ | ✅ |
| View own expenses | ✅ | ✅ | ✅ |
| View all expenses | ❌ | ✅ | ✅ |
| Approve / Reject expense | ❌ | ✅ | ✅ |
| Edit expense *(Pending only)* | ❌ | ✅ | ✅ |
| Upload / replace receipt | ❌ | ✅ | ✅ |
| Delete expense | ❌ | ❌ | ✅ |
| View dashboard | ❌ | ✅ | ✅ |
| Manage categories | ❌ | ❌ | ✅ |
| Manage users | ❌ | ❌ | ✅ |

---

## 📡 API Reference

### Authentication
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register a new user |
| `POST` | `/api/auth/login` | Public | Login — returns JWT + refresh token |
| `POST` | `/api/auth/refresh-token` | Public | Rotate refresh token |

### Expenses
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/expense` | All roles | Paginated list (search + filters) |
| `POST` | `/api/expense` | All roles | Submit a new expense |
| `PUT` | `/api/expense/{id}` | Admin / Manager | Edit expense |
| `POST` | `/api/expense/{id}/approve` | Admin / Manager | Approve or reject with reason |
| `POST` | `/api/expense/{id}/receipt` | Admin / Manager | Upload / replace receipt file |
| `DELETE` | `/api/expense/{id}` | Admin | Hard delete expense + receipt |

### Categories
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/category` | All roles | Paginated category list |
| `POST` | `/api/category` | Admin | Create category with optional budget |
| `PUT` | `/api/category/{id}` | Admin | Update category |
| `PATCH` | `/api/category/{id}/deactivate` | Admin | Soft deactivate |
| `DELETE` | `/api/category/{id}` | Admin | Hard delete |

### Dashboard
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/dashboard/summary` | Admin / Manager | 6 KPI totals |
| `GET` | `/api/dashboard/monthly-expenses` | Admin / Manager | Monthly spend array |
| `GET` | `/api/dashboard/category-expenses` | Admin / Manager | Per-category spend + budget |

### Users
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/user` | Admin | Paginated user list |
| `PUT` | `/api/user/{id}/role` | Admin | Change user role |
| `PUT` | `/api/user/{id}/status` | Admin | Toggle active / inactive |
| `DELETE` | `/api/user/{id}` | Admin | Delete user |

> Full interactive documentation with request/response schemas available at the [Swagger UI](https://expensemgmt-api.azurewebsites.net/swagger).

---

## 💻 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- SQL Server (local instance or [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads))
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 1. Clone the repository

```bash
git clone https://github.com/Brisco15/Expense-Management-System.git
cd Expense-Management-System
```

### 2. Backend setup

```bash
cd backend/ExpenseManagement.API/ExpenseManagement.API
```

Configure `appsettings.json` with your local values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ExpenseManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "ExpenseManagementAPI",
    "Audience": "ExpenseManagementClient",
    "ExpirationMinutes": "60"
  }
}
```

Apply migrations and run:

```bash
dotnet ef database update
dotnet run
```

API: `https://localhost:5297` — Swagger: `https://localhost:5297/swagger`

### 3. Frontend setup

```bash
cd frontend/expense-management-ui
npm install --legacy-peer-deps
ng serve
```

App: `http://localhost:4200`

> The frontend points to `http://localhost:5297/api` in development mode. To change this, edit `src/environment.ts`.

---

## ☁️ Deployment

The full application is deployed to **Microsoft Azure**.

### Infrastructure

| Resource | Azure Service | Tier | Region |
|---|---|---|---|
| Backend REST API | App Service | F1 Free, Linux | West Europe |
| Production database | Azure SQL Database | Basic | West Europe |
| Frontend SPA | Azure Static Web Apps | Free | West Europe |

### Secrets management

Sensitive values are **never committed to source control**:

- `appsettings.Production.json` is listed in `.gitignore`
- Connection string, JWT key, issuer, and audience are stored as **Azure App Service Application Settings**
- Azure overrides `appsettings` values at runtime via environment variable injection
- The committed `appsettings.json` contains only empty placeholder strings

### Manual deployment

**Backend:**
```bash
dotnet publish -c Release -o ./publish
# Create zip with forward-slash paths (required for Linux App Service)
az webapp deploy --resource-group ExpenseManagementRG --name expensemgmt-api \
  --src-path publish.zip --type zip
```

**Frontend:**
```bash
npm run build -- --configuration production
swa deploy ./dist/expense-management-ui/browser \
  --deployment-token <token>
```

---

## 🔄 CI/CD

Two independent **GitHub Actions** workflows handle automated deployment. Each triggers only when its relevant part of the codebase changes.

### Frontend workflow — `.github/workflows/frontend-deploy.yml`

Triggers on push to `master` with changes under `frontend/`:

```
Setup Node.js 20
→ npm install --legacy-peer-deps
→ ng build --configuration production
→ Azure/static-web-apps-deploy@v1 (pre-built output)
```

### Backend workflow — `.github/workflows/backend-deploy.yml`

Triggers on push to `master` with changes under `backend/`:

```
Setup .NET 10 SDK
→ dotnet publish -c Release
→ zip output
→ azure/login@v2 (service principal)
→ az webapp deploy
```

### Required GitHub secrets

| Secret | Workflow | Description |
|---|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Frontend | SWA deployment token |
| `AZURE_CREDENTIALS` | Backend | Service principal JSON (`az ad sp create-for-rbac --json-auth`) |

---

## 👤 Author

**Brisco15** — Full-Stack Developer

[![GitHub](https://img.shields.io/badge/GitHub-Brisco15-181717?logo=github)](https://github.com/Brisco15)

> Built from scratch as a portfolio project demonstrating end-to-end production skills: Clean Architecture, JWT security with real-time enforcement, Angular Signals, RESTful API design, EF Core Code First, role-based access control, file management, Azure cloud deployment, and automated CI/CD pipelines.
