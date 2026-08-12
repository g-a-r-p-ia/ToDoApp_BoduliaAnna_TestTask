# Candidate Test Task Evaluation: SoftPlus Ukraine

**Candidate level applied for:** Trainee / Junior Full Stack Angular/.NET Developer  
**Codebase reviewed:** Full-stack To-Do application (`.NET Core 8` REST API + `Angular 21` SPA)  
**Scope:** All application source code under `ToDoApp.API`, `ToDoApp.Services`, `ToDoApp.Interfaces`, `ToDoApp.DataAccess`, and `ClientApp` (node_modules/bin/obj and other gitignored artifacts excluded).

---

## 1. Overall Score & Verdict

### Score: **7.0 / 10**

### Verdict

This is a **solid junior-level submission with real architectural awareness**, but it falls short of "complete" in two dimensions: (1) several *assignment-critical* features (search, pagination, category management) are only implemented on the backend and are **not surfaced in the Angular UI**, and (2) there are **security/validation gaps** (no ownership check on `CategoryId`, a JWT signing key committed to the repository, unhandled exception paths) that a reviewer must not overlook.

For a Trainee/Junior, the layered 4-tier structure, DI wiring, real Google token validation, PBKDF2 password hashing, soft-delete with a global query filter, and a clean standalone Angular app with guards/interceptors are genuinely above average. The verdict is **"Hire for Trainee/early-Junior, with the condition that the candidate can explain and fix the identified gaps"** — the architecture is hireable, but UI feature-completeness and input-validation discipline are not yet at "mid-Junior" level.

---

## 2. Requirements Compliance Checklist

### Features

| # | Requirement | Status | Evidence / Notes |
|---|-------------|--------|------------------|
| 1 | Creating, Viewing, Editing, Deleting tasks | ✅ **Backend / ⚠️ Partial UI** | `TodoTasksController` (`Create`, `GetAllForUser`, `Update`, `SoftDelete`). Frontend can create, list, toggle-complete, edit (title + category only), delete. `Description`/`Deadline` exist in DTOs/entity but are **not exposed in the UI forms**. |
| 2 | Adding Categories for tasks | ⚠️ **Partial** | Backend `POST /api/categories` + `GET /api/categories` fully wired. **Frontend has no UI to create categories** — it can only select the three auto-seeded ones (`Work`, `Personal`, `Study`). No update/delete for categories (not strictly required, but notable). |
| 3 | Log in / log out | ✅ **(bonus: 3 flows)** | Email/password `register` + `login` (PBKDF2) **and** Google OAuth (`google-login`) with real `GoogleJsonWebSignature.ValidateAsync`. JWT in `localStorage`; navbar Login/Logout; route guard on `/tasks`. |
| 4 | Pagination for the list of tasks | ⚠️ **Partial** | Fully implemented **server-side** (`pageNumber`/`pageSize` → `Skip/Take` + `CountAsync` in `TodoTaskRepository`, wrapped in `PagedResultDto<T>`). **The Angular client always calls `GET /api/tasks` with no query params** (defaults page=1, size=10) and there is **no pagination UI**. A user with >10 tasks can never see past page 1. |
| 5 | Searching and filtering by categories | ⚠️ **Partial / Inconsistent** | Server-side `searchTerm` and `categoryId` query params work. **But:** (a) there is **no search input anywhere in the UI**; (b) category filtering is done **client-side over the single fetched page** (`tasks.component.ts#applyFilters`), so filter results can be incomplete (tasks on later pages are invisible). The server filter is never used by the client. |

### Technologies

| # | Technology | Status | Evidence / Notes |
|---|------------|--------|------------------|
| 1 | REST API on .NET Core with relational DB | ✅ | `net8.0` Web API + **MS SQL Server** (`UseSqlServer`, `Trusted_Connection` connection string). |
| 2 | 4-level architecture: controllers, services, interfaces, data access | ✅ | Four projects: `ToDoApp.API` → `ToDoApp.Services` → `ToDoApp.Interfaces` ← `ToDoApp.DataAccess`. Dependency direction correct and inward-pointing. |
| 3 | EF Core | ✅ | Code-First, Fluent API configurations, `ApplyConfigurationsFromAssembly`, global query filter, soft delete. (Migrations caveat — see §5.) |
| 4 | Dependency Injection | ✅ | Services + repositories registered via `AddScoped` in `Program.cs` (composition root). |
| 5 | Angular (Bootstrap or Tailwind) | ✅ | Angular 21 **standalone** components, `ReactiveFormsModule`, functional `CanActivateFn` guard, `HttpInterceptorFn` JWT interceptor, **Bootstrap 5** loaded via `angular.json`. |

---

## 3. Architecture & Data Flow Analysis

### 3.1 The four tiers and who depends on whom

```
                ┌─────────────────────────────────────────────────┐
   HTTP         │                 ToDoApp.API                     │
   JSON ──────► │  Controllers: Auth, TodoTasks, Categories      │
                │  Program.cs (composition root: DI + JWT + CORS) │
                └───────────────────────┬─────────────────────────┘
                                        │ (references)
                ┌───────────────────────▼─────────────────────────┐
                │             ToDoApp.Services                    │
                │  Implementations: AuthService, TodoTaskService, │
                │  CategoryService + DTOs + AutoMapper profiles   │
                └───────────────────────┬─────────────────────────┘
                                        │ depends ONLY on interfaces
                ┌───────────────────────▼─────────────────────────┐
                │            ToDoApp.Interfaces                   │
                │  Entities (User, Category, TodoTask)            │
                │  Repository interfaces + IGenericRepository     │
                └───────────────────────┬─────────────────────────┘
                                        ▲
                ┌───────────────────────┴─────────────────────────┐
                │           ToDoApp.DataAccess                    │
                │  ToDoDbContext, Fluent configurations,          │
                │  Repositories (TodoTask/Category/User)          │
                └─────────────────────────────────────────────────┘
                                        │
                                        ▼
                              SQL Server (MS SQL)
```

- **Controllers** depend only on `ToDoApp.Services.Interfaces`. No business logic in controllers.
- **Services** depend only on `ToDoApp.Interfaces` (repository contracts + entities) plus AutoMapper/`IConfiguration` — they have **no reference to `ToDoDbContext` or DataAccess** (a genuine dependency-inversion win).
- **Repositories** own all EF Core logic (filters, pagination, count, soft delete).
- **Entities live in the innermost layer** (`ToDoApp.Interfaces.Entities`) — unusual (entities are normally domain models, not interface contracts), but a defensible pragmatic choice that keeps the dependency arrow pointing inward. Worth probing in the interview (§6, Q4).

### 3.2 End-to-end trace — "Create a Task"

1. **Angular UI** — `TasksComponent.onSubmit()` (`tasks.component.ts:97`) validates the `taskForm` (title + categoryId) and calls `TaskService.addTask({ title, categoryId })`.
2. **HTTP layer** — `TaskService` (`core/services/task.service.ts:22`) POSTs to `http://localhost:5000/api/tasks`. The `jwtInterceptor` (`core/interceptors/jwt.interceptor.ts`) reads `jwt_token` from `localStorage` and injects `Authorization: Bearer <token>`.
3. **Controller** — `TodoTasksController.Create` (`TodoTasksController.cs:21`) is `[Authorize]`; it extracts the user id from the JWT claim `ClaimTypes.NameIdentifier` (the `Sub` claim written by `AuthService.GenerateToken`), validates it with `Guid.TryParse`, and calls `ITodoTaskService.CreateAsync(dto, userId)`. **The client-supplied `userId` is never trusted** — it always comes from the token. Good security decision.
4. **Service** — `TodoTaskService.CreateAsync` (`TodoTaskService.cs:20`) maps the DTO to `TodoTask` with AutoMapper, then stamps `taskEntity.UserId = userId` and `CreatedAt = DateTime.UtcNow`. It does **not** verify that `dto.CategoryId` belongs to `userId` (see §5 security flaw).
5. **Repository** — `TodoTaskRepository.AddAsync` (`TodoTaskRepository.cs:64`) does `_context.TodoTasks.Add(entity)` + `SaveChangesAsync()`.
6. **EF Core / DB** — SQL Server persists the row (FKs enforced by Fluent config).

### 3.3 End-to-end trace — "List tasks (pagination / filter / search)"

1. `TasksComponent.ngOnInit` → `TaskService.getTasks()` → `GET /api/tasks` (no query params → defaults `pageNumber=1, pageSize=10`).
2. `TodoTasksController.GetAllForUser` extracts `userId` from claims and forwards query params.
3. `TodoTaskService.GetAllForUserAsync` calls `CountForUserAsync` (count query) then `GetForUserAsync` (paged query).
4. `TodoTaskRepository.GetForUserAsync` builds an `IQueryable` with `Where(UserId == userId && !IsDeleted)` (plus the global `HasQueryFilter`), conditionally applies `categoryId` and `Title.Contains(searchTerm)`, `Include(t => t.Category)` (so `CategoryName` populates via AutoMapper flattening), `OrderByDescending(CreatedAt)`, `Skip/Take`.
5. The result is mapped to `PagedResultDto<TodoTaskDto>` `{ Items, TotalCount, PageNumber, PageSize }` and returned.
6. **Client gap:** `TasksComponent` takes only `data.items`, throws away `totalCount`, and re-filters the 10 items client-side by category/status. The server's search/filter/pagination capabilities are bypassed by the UI.

### 3.4 Auth flow

`LoginComponent` (Google GSI button rendered via polling) → `AuthService.loginWithGoogle(token)` → `POST /api/auth/google-login` → `AuthService.LoginWithGoogleAsync` validates the token with Google (`GoogleJsonWebSignature.ValidateAsync`), upserts the `User` by email (creating the three default categories on first login), and returns a signed JWT (`Sub` = user id, `Email`, `Jti`, 1-hour expiry). The same `GenerateToken` path serves `register` and `login`. Logout is client-side only (token removed from `localStorage`); no server-side token invalidation (acceptable for a JWT MVP, but worth a mention).

---

## 4. Code Quality: OOP, SOLID, and Clean Code

### 4.1 OOP Principles

| Principle | Assessment | Examples |
|-----------|-----------|----------|
| **Encapsulation** | ✅ Good | Internal plumbing is well-hidden: `AuthService` keeps `HashPassword`/`VerifyPassword` private, repositories encapsulate all EF Core usage, and the Angular HTTP details stay inside services. |
| **Abstraction / Polymorphism** | ✅ Good | Heavy use of interfaces (`IAuthService`, `ITodoTaskService`, `ICategoryService`, `ITodoTaskRepository`, ...). Implementations are swapped purely through DI in `Program.cs` — textbook polymorphism via dependency injection. |
| **Inheritance** | ✅ Appropriate | Controllers inherit `ControllerBase`; repositories use composition over inheritance (correct). `GenericRepository<T>` exists but is orphaned (see below). |
| **Modeling** | ⚠️ Mixed | `TodoTask`, `Category`, `User` are simple anemic POCOs — acceptable for EF Core CRUD at this level. The `IsDeleted`/`UpdatedAt` soft-delete modeling is a nice touch. |

### 4.2 SOLID — with concrete evidence from the candidate's code

- **S — Single Responsibility: mostly ✅, two violations.**
  - ✅ `TodoTasksController`, `TodoTaskService`, `TodoTaskRepository` each have one clear job; DTOs are separated from entities; mappings live in a dedicated `TodoTaskMappingProfile`.
  - ❌ **`AuthService` does too much**: it performs Google validation, local registration, password hashing/verification, JWT generation **and** default-category seeding (`SeedDefaultCategoriesIfMissingAsync`). Category seeding belongs in `CategoryService` — `AuthService` had to inject a second repository (`ICategoryRepository`) just to seed categories.
  - ❌ **`AuthController` mixes error mapping with orchestration** — it catches service exceptions (`KeyNotFoundException`, `InvalidOperationException`, `UnauthorizedAccessException`) to decide HTTP status codes. Services should not use exceptions as their primary error channel.

- **O — Open/Closed: ✅ mostly.** New storage behavior can be added behind repository interfaces; new mappings are added to a Profile; Fluent API configurations follow OCP. Weak spot: `AuthService`'s three-in-one responsibility means adding a new login provider would require modifying the existing class.

- **L — Liskov Substitution: ✅.** All implementations honor their interfaces; no weakened contracts observed.

- **I — Interface Segregation: ⚠️ mixed.**
  - ✅ Specific repository interfaces (`ITodoTaskRepository`, `ICategoryRepository`, `IUserRepository`) expose only what each consumer needs.
  - ❌ **`GenericRepository<T>`/`IGenericRepository<T>` are dead code.** They are never registered in DI (`Program.cs` registers only the three concrete repositories) and never referenced anywhere. An unused generic abstraction is speculative generality that contradicts ISP/YAGNI — use it or delete it.

- **D — Dependency Inversion: ✅ strong point.** Services depend on abstractions in `ToDoApp.Interfaces`; `ToDoDbContext` lives only in DataAccess; the API project is the composition root. The Services→DataAccess reference was removed — the best-executed SOLID principle in the submission.

### 4.3 Clean Code Assessment

**Naming conventions — ✅ strong.** `LoginWithGoogleAsync`, `GetAllForUserAsync`, `SoftDeleteAsync`, `CountForUserAsync`, `SeedDefaultCategoriesIfMissingAsync`, `selectedTaskToEdit`, `editTaskForm`, `onToggleComplete` are all self-documenting. No needless abbreviations. This is a genuine junior-plus habit.

**Method lengths & complexity — ✅ mostly good.** Backend methods are short and single-purpose (the longest, `AuthService`, is cohesive per method). Angular `TasksComponent` methods are short; `getErrorMessage` centralizes status-code→message mapping.

**Code duplication — ❌ noteworthy.**
- The 4-line "extract `userId` from claims + `Guid.TryParse` → `Unauthorized()`" block is copy-pasted **four times** in `TodoTasksController` (lines 24–28, 41–45, 54–58, 72–76) and twice in `CategoriesController`. This belongs in a shared helper, a base controller, or (better) a claim-extraction extension/`ActionFilter`.
- The 3-default-categories seeding list `"Work"/"Personal"/"Study"` is duplicated in `AuthService` (`LoginWithGoogleAsync` and `RegisterAsync`), plus a third copy in `SeedDefaultCategoriesIfMissingAsync`. One constant/single seeding method would be cleaner.

**Error handling — ⚠️ the weakest Clean Code area.**
- `AuthController` uses try/catch + exception types as control flow (`KeyNotFoundException` → 404, `UnauthorizedAccessException` → 401, `InvalidOperationException` → 409). If an exception ever escapes the catch blocks, the API returns an opaque 500 instead of a structured `ProblemDetails` response. There is **no global exception-handling middleware** (`UseExceptionHandler` / custom `ExceptionFilter`), so unhandled errors leak stack details in Development and generic 500s in Production.
- `AuthController.GoogleLogin` has **no error handling at all**: an invalid Google token throws `InvalidJwtException` from `GoogleJsonWebSignature.ValidateAsync` and results in a **500** instead of a 400 — the client's "unexpected error" message is all the user sees.
- `AuthService.VerifyPassword` (lines 164–175) blindly does `storedHash.Split('.')` and `int.Parse`/`Convert.FromBase64String` on the three parts — a malformed/corrupted stored hash throws an unhandled `IndexOutOfRangeException`/`FormatException` → 500. Defensive checks are missing.
- **Mixed HTTP semantics:** `LoginAsync` throws `KeyNotFoundException` (→404) for a missing account. Semantically "wrong password" and "no such user" should both be 401 (you should not reveal whether an email is registered); returning 404 leaks account existence.

**Frontend quality — ✅ good, with caveats.**
- Reactive forms with validators + `markAllAsTouched`, `invalid-feedback` UI, error alerts, and an `HttpInterceptorFn` for Bearer tokens are all above junior baseline.
- `tasks.component.ts` models everything as `any[]` (lines 23–24) even though strict TS is enabled — the `TodoTask` interface exists but is only used for `selectedTaskToEdit`. Typed models + a shared `PagedResult<T>` interface would be cleaner and would have caught the `data.items`/`totalCount` usage mismatch.
- RxJS subscriptions (`getTasks`, `getCategories`, `login`, etc.) are never unsubscribed (`takeUntil`/`AsyncPipe`); in a real app this leaks subscriptions on repeated navigation.
- `onToggleComplete` mutates `task.isCompleted` *before* the server call and never rolls back on error — optimistic update without reconciliation.

**Comments — ✅ aligned with Clean Code.** No educational/excessive comments; the few that exist (`auth.guard.ts`) explain the *why*.

**DB/SQL concerns.**
- `Title.Contains(searchTerm)` translates to `LIKE '%term%'` — correct, but non-sargable for the leading wildcard; acceptable at this scale, worth knowing.
- `GetAllForUserAsync` runs two round-trips (count + page) — standard for paging; fine.
- **Soft delete does a redundant double read:** `TodoTaskService.SoftDeleteAsync` fetches the entity (line 70) and then `TodoTaskRepository.SoftDeleteAsync` fetches it again (line 78) before updating — one extra SELECT per delete. The service could rely on the repository's `bool` return instead of pre-fetching.
- **No migrations bootstrap:** only a `ToDoDbContextModelSnapshot.cs` was identified in `Migrations/` (no migration `.cs` files were found during the audit), and `Program.cs` never calls `EnsureCreated()`/`Migrate()`. A fresh clone has **no automated way to create the SQL Server schema**. Even if a migration exists, the absence of an explicit migrate/ensure step at startup (or documented `dotnet ef database update` instructions) is an operational gap — and the README contains only `# TestPetProject`.

---

## 5. Candidate Profile: Strengths and Weaknesses

### Strengths — what the candidate did exceptionally well for a Junior

1. **Real 4-tier architecture with correct dependency inversion.** The Services layer has no dependency on DataAccess; everything is wired through interfaces in a dedicated `ToDoApp.Interfaces` project. Many juniors produce a single monolithic project or controllers that call `DbContext` directly — this candidate did not.
2. **Production-aware authentication.** PBKDF2 password hashing with a per-user random salt, high iteration count (100,000), and constant-time comparison (`CryptographicOperations.FixedTimeEquals`); **real** Google token validation via `GoogleJsonWebSignature.ValidateAsync` (not a stub); JWT with proper `Sub`/`Email`/`Jti` claims, issuer/audience/lifetime validation, and `ClockSkew = TimeSpan.Zero` on the verification side.
3. **Authorization-by-claims discipline.** User identity is derived from the token (`ClaimTypes.NameIdentifier`), never from the client request body/route — the correct pattern for multi-tenant-style data isolation, applied consistently to tasks and categories.
4. **Soft delete done right.** `IsDeleted` + `UpdatedAt` on the entity with a **global query filter** (`HasQueryFilter(t => !t.IsDeleted)`) — deleted rows stay out of every query automatically.
5. **Server-side paging/search/filter implemented cleanly** in the repository layer (`CountAsync` + `Skip/Take` + conditional `Where`), wrapped in a generic `PagedResultDto<T>`.
6. **Modern Angular 21 standalone patterns.** Functional `CanActivateFn` guard, functional `HttpInterceptorFn`, `@for`/`@if` control flow, Reactive Forms with validation UX, `BehaviorSubject`-driven auth state, Bootstrap 5 throughout, and a consistent `environment.ts` for the Google client id.
7. **Self-documenting naming** and short, single-purpose methods on the backend; no leftover commented-out code or placeholder TODOs.
8. **DI composition root done correctly** in `Program.cs` (scoped repositories + services + AutoMapper profiles).

### Weaknesses / Flaws — what is poorly written, missing, or fundamentally wrong (detailed)

1. **🔴 Security flaw — no ownership validation on `CategoryId`.**
   `TodoTaskService.CreateAsync`/`UpdateAsync` (`TodoTaskService.cs:20–49`) blindly accept any `CategoryId` without checking that the category belongs to `userId`. SQL Server's FK only enforces *existence*, not *ownership*. Consequences:
   - A user can attach another user's category to their task.
   - `TodoTaskRepository.GetForUserAsync` does `.Include(t => t.Category)`, so the victim's **category name is leaked** into the attacker's response (`TodoTaskDto.CategoryName`).
   - Task creation also never verifies the category exists (a random GUID → FK violation → opaque 500).
   Fix direction: ownership check in the service/repository, or a composite FK `(UserId, CategoryId)`.

2. **🔴 Security flaw — JWT signing key committed to source control.**
   `appsettings.json` contains the literal signing key (`"ThisIsMySuperSecretKeyForJwtTokenGenerationWhichNeedsToBeVeryLong"`). Anyone with repo access can forge tokens for any user. For a test task this is common, but the candidate should use `appsettings.Development.json` + User Secrets / environment variables, and keep `appsettings.json` secret-free. The `Trusted_Connection` connection string is also committed.

3. **🔴 Assignment-critical features missing from the UI.**
   - **No search box** (server `searchTerm` support exists but is dead code from the UI's perspective).
   - **No pagination UI** — the client always fetches page 1 / size 10 and renders `tasks.length` as the "total" badge (wrong: shows 10, not `totalCount`).
   - **Category filter is client-side only** over the single fetched page → incorrect/incomplete filtering results as soon as more than one page of data exists.
   - **No category management UI** — categories are backend-only; users can never add their own.
   This is the single biggest gap between the assignment and the delivered product.

4. **🟠 Exception-based control flow and unhandled error paths.**
   `AuthController` catches service exceptions to compute HTTP status codes; `GoogleLogin` catches nothing, so an invalid Google token returns a raw 500; `VerifyPassword` (`AuthService.cs:164–175`) can throw `IndexOutOfRangeException`/`FormatException` on malformed stored hashes; there is no global exception-handling middleware or `ProblemDetails`. The 404-vs-401 split in `LoginAsync` (missing email → `KeyNotFoundException` → 404) also reveals whether an account exists.

5. **🟠 Incomplete and inconsistent validation.**
   `CreateTodoTaskDto` has `[Required]`/`[MaxLength]`, but `UpdateTodoTaskDto`, `RegisterDto`, `LoginDto`, and `CreateCategoryDto` have **no validation attributes at all**. The API accepts empty passwords/emails/category names/titles on update; emails are not normalized, so case-variant duplicate accounts are possible (Google stores lowercase, `RegisterDto` doesn't).

6. **🟠 Dead code.** `IGenericRepository<T>` (`ToDoApp.Interfaces/IGenericRepository.cs`) and `GenericRepository<T>` (`ToDoApp.DataAccess/Repositories/GenericRepository.cs`) are implemented but never registered in DI or referenced. Speculative leftover abstraction.

7. **🟠 Duplicated claim-extraction logic.** The "find `NameIdentifier` claim → `Guid.TryParse` → `Unauthorized()`" block is copy-pasted **4× in `TodoTasksController` and 2× in `CategoriesController`** — a cross-cutting concern that should be a shared helper/extension or `ActionFilter`.

8. **🟠 No automated database bootstrap.** No migration `.cs` files were identified in `Migrations/` (only `ToDoDbContextModelSnapshot.cs`), and `Program.cs` never calls `EnsureCreated()`/`Migrate()`. A fresh clone cannot create the SQL Server schema without manual intervention; the README (`# TestPetProject`) gives zero run instructions, and there is no `launchSettings.json` — the "clone → run" experience is undocumented and fragile.

9. **🟡 No backend tests.** Only a single Angular smoke test (`app.spec.ts`) exists. Given the job description explicitly mentions *"participate in code reviews, help fix bugs"*, at least a couple of unit tests for `AuthService`/`TodoTaskService` or the repository paging would meaningfully raise the profile.

10. **🟡 Frontend polish issues.** `tasks: any[]` / `filteredTasks: any[]` bypass strict TypeScript even though the `TodoTask` interface exists; RxJS subscriptions are never unsubscribed; `onToggleComplete` optimistically mutates state with no rollback on error; `totalCount` from `PagedResultDto` is discarded; error handling mixes `console.error` with user-facing alerts inconsistently.

---

## 6. Technical Interview Questions (Tailored to this Code)

1. **On your error model.** In `AuthService.LoginAsync` you throw `KeyNotFoundException` for a missing user, and `AuthController.Login` catches it to return 404. What happens if that exception escapes (e.g., a race between the `GetByEmailAsync` check and token generation)? How would you design a `Result<T>`-style return or a global `ExceptionFilter`/`ProblemDetails` handler instead of try/catch in controllers?

2. **On data isolation.** In `TodoTaskService.CreateAsync` you set `taskEntity.UserId = userId` from the token but never check that `dto.CategoryId` belongs to that user. Can user A attach user B's category to a task, and what exactly gets leaked to user A through `TodoTaskRepository.GetForUserAsync`'s `.Include(t => t.Category)`? Walk me through how you'd fix this (ownership check vs. composite FK `(UserId, CategoryId)`).

3. **On the UI/API mismatch.** Your `TasksComponent` filters tasks by category and status entirely client-side over `getTasks()` results, but `GET /api/tasks` is paginated server-side with `pageNumber`/`pageSize` (default 1/10) and supports `searchTerm`/`categoryId`. If a user has 40 tasks and selects a category whose tasks are on page 4, what does your UI show, and why? How would you reconcile server-side pagination with the filters (e.g., move filtering to the server and add a pager)?

4. **On the generic repository.** `IGenericRepository<T>` and `GenericRepository<T>` exist in the codebase but are never registered in `Program.cs` or used by any service. Why did you keep them, and what are the trade-offs between a generic CRUD repository and the specific repositories (`ITodoTaskRepository`, etc.) you actually used? When would a generic repository violate Interface Segregation?

5. **On password security.** `AuthService.RegisterAsync` hashes with PBKDF2 (`Rfc2898DeriveBytes.Pbkdf2`, 100,000 iterations, per-user 16-byte salt) and `VerifyPassword` uses `CryptographicOperations.FixedTimeEquals`. Explain why you chose PBKDF2 over SHA-256, what `FixedTimeEquals` protects against, and what you would change for production (e.g., Argon2id, pepper, lockout policies).

6. **On secret management.** Your JWT signing key and SQL connection string live in `appsettings.json` and are committed to the repository. How would you manage those secrets in a SoftPlus deployment, and what is the concrete attack scenario if the signing key leaks? How would you rotate the key without logging every user out?

7. **On database provisioning.** The repository contains an EF Core model snapshot (`ToDoDbContextModelSnapshot.cs`) but no migration files, and `Program.cs` never calls `EnsureCreated()` or `Database.Migrate()`. How would you create the SQL Server schema on a fresh machine, and what is the difference between `EnsureCreated()` and EF Migrations — specifically, which one supports evolving the schema over time?

8. **On API contract design.** `AuthController.GoogleLogin` accepts a raw `[FromBody] string googleToken`, and the Angular client sends `JSON.stringify(token)`. What are the problems with a primitive-string request body (validation, versioning, extensibility, Swagger documentation), and how would you redesign it with a proper DTO? (Bonus: why does returning 404 for a missing email in `LoginAsync` contradict OWASP user-enumeration guidance?)

---

*Report generated as a structured code-review deliverable for the SoftPlus Ukraine Trainee/Junior hiring process. No source code was modified.*







