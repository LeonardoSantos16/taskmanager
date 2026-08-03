---
name: dotnet-testing
description: Use this skill whenever writing, reviewing, or planning tests for the TaskManager project (API or Functions) — unit tests, repository tests, or integration tests. Trigger this for any request involving xUnit, Moq, FluentAssertions, Testcontainers, WebApplicationFactory, test coverage, or when the user asks to "add tests", "test this service", "cover this rule", or references business rules (RN01-RN16) that need test coverage. Also use when setting up or modifying the test project structure itself.
---

# .NET Testing — TaskManager

Guide for writing tests for this specific project. Not a generic xUnit tutorial — assumes the reader already knows how to test in .NET, and focuses on this project's conventions and priorities.

## Testing stack

- **xUnit** — test runner
- **Moq** — mocking dependencies (repositories, when testing Services in isolation)
- **FluentAssertions** — all assertions use `.Should()`, never plain `Assert.Equal`
- **EF Core InMemory** — for Service tests that need a real DbContext but not Azure SQL
- **Testcontainers** (mssql) — only for Repository tests that need to validate real SQL Server behavior (complex queries, constraints). Do not use Testcontainers to test Services — it's slower and unnecessary when InMemory is enough.
- **WebApplicationFactory** — integration tests for full Controllers/endpoints

## Expected folder structure

```
TaskManager.Tests/
├── Services/            # unit tests for business rules (top priority)
├── Repositories/        # tests with Testcontainers
├── Integration/         # tests via WebApplicationFactory
├── Authorization/       # tests for role-based authorization handlers
└── Helpers/
    ├── TestDbContextFactory.cs
    └── EntityBuilders.cs   # builders to create test entities with sensible default values
```

## Priority order when generating tests

Always implement in this order, unless the user asks for something specific outside it:

1. **Services with real conditional logic** (not plain CRUD) — see business rules list below
2. **Authorization handlers** (RBAC by role: Owner/Editor/Viewer)
3. **Repository + Testcontainers** (only queries with actual logic: `GetDueSoonAsync`, `MarkDueSoonNotifiedAsync`)
4. **Controller integration tests** via WebApplicationFactory
5. Plain CRUD with no business rule — only if explicitly requested, lowest priority

## Business rules that MUST have tests (don't skip any)

When generating Service tests, cover each of these as a separate `[Fact]` or `[Theory]`, named so the rule is obvious from the test name:

| Rule | Expected behavior | Example test name |
|---|---|---|
| RN10 | Status → `Done` sets `CompletedAt` | `UpdateStatus_ToDone_SetsCompletedAt` |
| RN11 | Status moves away from `Done` → `CompletedAt` resets to `null` | `UpdateStatus_FromDoneToOther_ClearsCompletedAt` |
| RN13 | `DueDate` earlier than `CreatedAt` must be rejected | `CreateTask_DueDateBeforeCreatedAt_ThrowsValidationException` |
| RN09 | Assigning a task to a non-member must fail | `AssignTask_UserNotProjectMember_ThrowsException` |
| RN08 | Owner cannot remove themselves from the project | `RemoveMember_OwnerRemovingSelf_ThrowsException` |
| RN05 | Adding a duplicate member must fail | `AddMember_AlreadyExists_ThrowsException` |
| RN03 | Archived project rejects new tasks | `CreateTask_ProjectArchived_ThrowsException` |
| RN03 | Archived project rejects new members | `AddMember_ProjectArchived_ThrowsException` |
| RN06 | Viewer cannot create/edit a task | `CreateTask_UserIsViewer_Forbidden` |
| RN07 | Editor cannot remove members | `RemoveMember_UserIsEditor_Forbidden` |
| RN14 | Only creator/assignee/Editor/Owner can change status | `UpdateStatus_UserWithoutPermission_Forbidden` |
| RN12 | Cancelled tasks are excluded from metrics | `GetProjectMetrics_ExcludesCancelledTasks` |

Always test both the happy path AND the rejection/exception path for each rule above — a rule without a test for its negative case is incomplete.

## Mandatory AAA pattern

Every test follows Arrange / Act / Assert with comments marking each section:

```csharp
[Fact]
public async Task UpdateStatus_ToDone_SetsCompletedAt()
{
    // Arrange
    var context = TestDbContextFactory.CreateInMemory();
    var task = EntityBuilders.CreateTaskItem(status: TaskStatus.Todo);
    context.Tasks.Add(task);
    await context.SaveChangesAsync();
    var service = new TaskItemService(new TaskItemRepository(context));

    // Act
    await service.UpdateStatusAsync(task.Id, TaskStatus.Done, actingUserId: task.CreatedById);

    // Assert
    var updated = await context.Tasks.FindAsync(task.Id);
    updated!.CompletedAt.Should().NotBeNull();
}
```

## Test naming convention

`{Method}_{Scenario}_{ExpectedResult}`

Examples: `CreateProject_ValidData_ReturnsCreatedProject`, `AssignTask_UserNotMember_ThrowsBusinessRuleException`.

## Helpers to reuse (create if they don't exist)

- `TestDbContextFactory.CreateInMemory()` — returns an `AppDbContext` using `Guid.NewGuid().ToString()` as the InMemory database name, ensuring isolation between tests.
- `EntityBuilders` — static methods like `CreateTaskItem(...)`, `CreateProject(...)`, `CreateUser(...)` with optional parameters and sensible defaults, to avoid repeating verbose setup in every test.

## Testcontainers — when and how

Only use in `Repositories/`, to test behavior the InMemory provider doesn't faithfully reproduce (constraints, unique indexes, `DateTime` queries in SQL Server). Setup example:

```csharp
public class TaskItemRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
```

Don't use Testcontainers for anything InMemory already covers — it slows down the suite with no real benefit.

## Running tests

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~TaskItemServiceTests"
dotnet test --collect:"XPlat Code Coverage"
```

## What not to do

- Don't generate tests for getters/setters or trivial CRUD with no logic.
- Don't use `Assert.True(x == y)` — always FluentAssertions (`x.Should().Be(y)`).
- Don't mix Service tests and Repository tests in the same file/class.
- Never use the real (production) Azure SQL in any test — always InMemory or Testcontainers.
