# Expense Management System

A full-stack expense management web application built with **Angular 21** and **ASP.NET Core (.NET 10)**, following Clean Architecture principles. Designed to handle the full lifecycle of expense tracking — from submission to approval — with granular role-based access control and real-time security enforcement.

Built as a portfolio project to demonstrate end-to-end full-stack development across a production-grade architecture.

---

## Live Features

### 🔐 Authentication & Security
- JWT Bearer authentication with **refresh token rotation**
- Passwords hashed before storage — never stored in plain text
- **Real-time role enforcement**: when an admin changes a user's role, the backend validates the JWT claims against the database on every request. The moment the role changes, the user's next API call returns `401` and the frontend automatically logs them out and redirects to login — no waiting for token expiry
- Refresh token is cleared on role change to prevent silent re-authentication
- Rate limiting: 100 requests per minute per user (fixed window)
- CORS policy restricted to the Angular origin
- Input validation via Data Annotations on all DTOs

### 📊 Dashboard (Admin & Manager)
- **6 KPI cards**: All-Time Total, This Month, This Year, Approved, Pending, Rejected (all with currency formatting)
- **Monthly bar chart** — spending trend over time (Chart.js)
- **Category doughnut chart** — spending breakdown by category
- **Budget utilization** — progress bars per category showing actual vs monthly budget, highlighted in red when over budget
- **Recent expenses** — last 5 expenses with status badges, category, and submitter

### 💸 Expense Management
- Employees can submit expenses; Admins & Managers can view all
- **Full status workflow**: `Pending → Approved / Rejected` with optional rejection reason
- **Receipt upload** (Admin/Manager only): JPG, JPEG, PNG, PDF up to 5MB — stored on disk, old files replaced on re-upload, cleaned up on expense deletion
- Edit expenses (Pending only) with inline receipt attachment
- Delete expenses (Admin only)
- Paginated list with **search** (title, description, date) and **filter by category and status**
- Color-coded status badges (green/amber/red)

### 🗂️ Category Management (Admin only)
- Create, edit, soft-delete, and hard-delete categories
- Set **monthly and/or yearly budgets** per category
- Activate / deactivate categories
- Budget validation: at least one budget field required if providing budget
- Paginated list with search and active/inactive filter

### 👥 User Management (Admin only)
- View all users with role, status, and registration date
- **Inline role change** via color-coded dropdown (Admin = indigo, Manager = amber, Employee = teal) — triggers immediate session invalidation for the affected user
- Activate / deactivate accounts
- Delete users (with safeguards: cannot delete yourself or other Admins)
- Paginated list with search and filter by role and status

---

## Tech Stack

### Frontend
| Technology | Purpose |
|---|---|
| Angular 21 | SPA framework — standalone components, Signals API, `OnPush` change detection |
| Angular Material (MDC) | UI component library |
| TypeScript | Strongly typed language |
| Reactive Forms | Form handling and validation |
| Chart.js + ng2-charts | Dashboard charts |
| RxJS | Async data streams, `forkJoin`, `switchMap` |
| HTTP Interceptor | Attaches JWT to every request, handles global `401` → auto logout |

### Backend
| Technology | Purpose |
|---|---|
| ASP.NET Core (.NET 10) | RESTful Web API |
| Entity Framework Core | ORM with Code First migrations |
| SQL Server | Relational database |
| JWT Bearer Authentication | Stateless auth with custom token validation events |
| Clean Architecture | Domain / Application / Infrastructure / API layers |
| Swagger / OpenAPI | Interactive API documentation |

---

## Architecture

```
backend/
└── ExpenseManagement.API/
    ├── ExpenseManagement.Domain/          # Entities, Enums (no dependencies)
    ├── ExpenseManagement.Application/     # Interfaces, DTOs, business contracts
    ├── ExpenseManagement.Infrastructure/  # EF Core, Services, Migrations
    └── ExpenseManagement.API/             # Controllers, Middleware, Program.cs

frontend/
└── expense-management-ui/src/app/
    ├── core/          # Models, Services, Interceptors, Guards
    ├── features/      # Dashboard, Expenses, Categories, Users (lazy-loaded)
    └── shared/        # MaterialModule, shared components
```

**Design decisions:**
- **Clean Architecture**: the Domain layer has zero external dependencies; business logic lives in Application services, not controllers
- **Role-based policies**: 7 named policies (`AdminOnly`, `AdminOrManager`, `All`, etc.) applied per endpoint
- **OnPush + Signals**: the Angular frontend uses `ChangeDetectionStrategy.OnPush` throughout with Angular's Signals API for fine-grained reactivity, minimising unnecessary re-renders
- **Stateless JWT + active validation**: rather than a token blocklist, `OnTokenValidated` events query the user's current role/status on every request — instant enforcement without session state

---

## Role Permissions

| Feature | Employee | Manager | Admin |
|---|---|---|---|
| Submit expense | ✅ | ✅ | ✅ |
| View own expenses | ✅ | ✅ | ✅ |
| View all expenses | ❌ | ✅ | ✅ |
| Approve / Reject expense | ❌ | ✅ | ✅ |
| Edit expense (Pending only) | ❌ | ✅ | ✅ |
| Upload receipt | ❌ | ✅ | ✅ |
| Delete expense | ❌ | ❌ | ✅ |
| Manage categories | ❌ | ❌ | ✅ |
| Manage users | ❌ | ❌ | ✅ |
| View dashboard | ❌ | ✅ | ✅ |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) + npm
- SQL Server (local or cloud instance)

### Backend setup

```bash
cd backend/ExpenseManagement.API/ExpenseManagement.API
```

Update `appsettings.json` with your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ExpenseManagementDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "ExpenseManagementAPI",
    "Audience": "ExpenseManagementClient",
    "ExpirationMinutes": 15
  }
}
```

```bash
dotnet ef database update
dotnet run
```

API runs at `https://localhost:5297` — Swagger UI at `/swagger`.

### Frontend setup

```bash
cd frontend/expense-management-ui
npm install
ng serve
```

App runs at `http://localhost:4200`.

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register new user |
| POST | `/api/auth/login` | Public | Login, returns JWT + refresh token |
| POST | `/api/auth/refresh-token` | Public | Rotate refresh token |
| GET | `/api/expense` | All roles | Paginated expense list |
| POST | `/api/expense` | All roles | Create expense |
| PUT | `/api/expense/{id}` | Admin/Manager | Update expense |
| POST | `/api/expense/{id}/approve` | Admin/Manager | Approve or reject |
| POST | `/api/expense/{id}/receipt` | Admin/Manager | Upload receipt |
| DELETE | `/api/expense/{id}` | Admin | Delete expense |
| GET | `/api/category` | All roles | List categories |
| POST | `/api/category` | Admin | Create category |
| PUT | `/api/category/{id}` | Admin | Update category |
| PATCH | `/api/category/{id}/deactivate` | Admin | Soft delete |
| GET | `/api/dashboard/summary` | Admin/Manager | KPI summary |
| GET | `/api/dashboard/monthly-expenses` | Admin/Manager | Monthly totals |
| GET | `/api/dashboard/category-expenses` | Admin/Manager | Per-category totals + budgets |
| GET | `/api/user` | Admin | List all users |
| PUT | `/api/user/{id}/role` | Admin | Change user role |
| PUT | `/api/user/{id}/status` | Admin | Toggle active/inactive |
| DELETE | `/api/user/{id}` | Admin | Delete user |

---

## Author

**Brisco** — Full-Stack Developer  
Building production-grade applications with Angular and .NET.

> This project was built entirely from scratch as a portfolio piece to demonstrate real-world full-stack skills: REST API design, Clean Architecture, JWT security, responsive Angular UI, role-based access control, and SQL Server data modelling.
