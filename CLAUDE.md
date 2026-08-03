# TaskManager — Project Context

## Overview

Project and task management system (Trello/Jira-style, simplified), built as a portfolio project to practice Azure + .NET/ASP.NET Core + React.

- **Backend**: ASP.NET Core Web API (.NET 10 target, but following .NET 8+ practices)
- **Auth**: ASP.NET Core Identity + JWT
- **Database**: Azure SQL Database (Entity Framework Core, Code First with Migrations)
- **Async notifications**: Azure Functions (Timer Trigger) running locally — separate project
- **Frontend**: React (not implemented yet at this stage)

## Solution structure

```
taskmanager.slnx
├── taskmanager.csproj              # Main API
│   ├── Authorization/              # Resource-based authorization handlers/requirements
│   ├── Context/
│   │   └── Configurations/         # Fluent API configs (IEntityTypeConfiguration per entity)
│   ├── Controllers/
│   ├── DTOs/
│   │   └── Mappings/                # Entity <-> DTO mapping
│   ├── Extensions/
│   ├── Middleware/
│   ├── Migrations/                 # EF Core Migrations
│   ├── Models/                     # Entities (User, Project, ProjectMember, TaskItem, TaskComment, Notification)
│   ├── Repositories/               # Interface + implementation (Repository Pattern)
│   └── Services/                   # Business logic
│
└── TaskManager_Functions.csproj    # Azure Functions (Timer Trigger)
    ├── DTOs/
    ├── Notifications/               # Due-date check logic + email sending (SendGrid) + persistence
    └── Properties/
```

## Architectural patterns in use

- **Repository Pattern**: the entire API accesses data through interfaces (`ITaskItemRepository`, `IProjectRepository`, etc.), never through `DbContext` directly inside Controllers or Services. Repositories implement the interface and receive `AppDbContext` via dependency injection.
- **Services** hold the business logic and orchestrate repository calls. Controllers are thin — they only route the request, validate model state, and call the corresponding Service.
- **DTOs** are always used at Controller boundaries — entities (`Models/`) are never exposed directly by the API.
- **Authorization**: custom handlers in `Authorization/` implement per-project RBAC rules (Owner/Editor/Viewer). Do not scatter manual role checks across Services — centralize them in the handlers.

## Domain model (core entities)

- `User` — system user (also managed by Identity)
- `Project` — has `OwnerId`, `Status` (Active/Archived)
- `ProjectMember` — User↔Project join table, with `Role` (Owner/Editor/Viewer)
- `TaskItem` — belongs to a `Project`, has `Status` (Todo/InProgress/Done/Cancelled), `Priority`, `AssignedToId` (nullable), `CreatedById`, `DueDate` (nullable), `CompletedAt` (nullable)
- `TaskComment` — belongs to a `TaskItem`
- `Notification` — used by the Function to record due-date warnings

## Critical business rules (see project README for the full list)

The most important rules for test coverage:

- **RN10/RN11**: when `TaskItem.Status` changes to `Done`, `CompletedAt` is set automatically. If it moves away from `Done` to any other status, `CompletedAt` is reset to `null`.
- **RN13**: `DueDate`, if provided, cannot be earlier than the task's `CreatedAt`.
- **RN09**: a task can only be assigned (`AssignedToId`) to a user who is a member of the project (via `ProjectMember`).
- **RN08**: a project's `Owner` cannot remove themselves — ownership must be transferred first.
- **RN05**: a user cannot be added to the same project twice.
- **RN03**: a project with `Status = Archived` cannot receive new tasks or new members.
- **RN06/RN07**: `Viewer` can only view; `Editor` can create/edit tasks but cannot manage members; `Owner` can do everything.
- **RN14**: only the task creator, assigned user, or Editor/Owner members can change task status.
- **RN12**: tasks with `Status = Cancelled` are excluded from metrics/reports.

## Notification Function flow

1. Timer Trigger runs periodically.
2. Queries tasks with `DueDate` approaching (e.g., within the next 24h) and `Status` not `Done` or `Cancelled`.
3. For each matching task:
   - Writes a `Notification` record (database).
   - Sends an email via SendGrid.
   - Marks the task as "already notified" to avoid duplicate alerts on future runs.

## Commands

```bash
# Run the API locally
dotnet run --project taskmanager.csproj

# Add a migration
dotnet ef migrations add MigrationName --project taskmanager.csproj

# Apply migrations
dotnet ef database update --project taskmanager.csproj

# Run the Function locally
cd TaskManager.Functions
func start

# Run all tests (once the test project exists)
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Code conventions

- Service class names: `{Entity}Service` (e.g., `TaskItemService`, `ProjectService`).
- Repository interface names: `I{Entity}Repository` (e.g., `ITaskItemRepository`).
- All async methods end with the `Async` suffix.
- Input DTOs: `Create{Entity}Dto`, `Update{Entity}Dto`. Output DTOs: `{Entity}ResponseDto` or `{Entity}Dto`.
- Enums live in `Models/` alongside their related entities (`ProjectStatus`, `ProjectRole`, `TaskStatus`, `TaskPriority`).

## What this project does NOT use (avoid suggesting)

- Does not use Azure AD B2C (we deliberately chose Identity + JWT "homegrown" for learning purposes).
- Does not use Azure Service Bus (left as a "nice to have", not implemented).
- No CI/CD configured yet (postponed due to a VM quota limitation on the Azure Free Trial subscription).
- React frontend hasn't been started yet — don't suggest frontend code unless explicitly asked.

## Infrastructure context (relevant for decisions, not for code)

- Azure SQL Database (serverless, free tier) already provisioned and in use.
- App Service and Container Apps have not been published yet due to a VM quota limit on the Free Trial subscription (`SubscriptionIsOverQuotaForSku`). This is an infrastructure limitation, not something that affects application code.
- The Azure Function currently runs locally only, for the same quota reason.
