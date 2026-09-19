# Etherna Credit

Etherna Credit is the service that manages user credit across the Etherna services. It is an **ASP.NET Core application**: it exposes a REST API to read balances/operation logs and to let other Etherna services charge or refund user credit, a Razor Pages site for users (deposit, withdraw, manage) and admins (manual balance updates), and an async task engine (Hangfire) for maintenance jobs. It uses MongoDB for persistence and authenticates against the Etherna SSO server.

## Build, run, test

Target framework is **.NET 10** with `TreatWarningsAsErrors=true` and `AnalysisMode=AllEnabledByDefault` (and `Nullable=enable`, `EnableNETAnalyzers=true`) on every project — warnings break the build.

```bash
dotnet restore EthernaCredit.sln
dotnet build EthernaCredit.sln -c Release
dotnet test  EthernaCredit.sln -c Release           # runs the xUnit test projects
dotnet test test/EthernaCredit.Persistence.Tests/EthernaCredit.Persistence.Tests.csproj      # single project
dotnet test --filter "FullyQualifiedName~CreditDbContextDeserializationTest"                 # single class
dotnet test --filter "FullyQualifiedName~CreditDbContextDeserializationTest.UserDeserialization"  # single test
dotnet run  --project src/EthernaCredit            # local dev server, https://localhost:44369 (http: 31597)
```

Frontend assets are bundled by Laravel Mix (webpack): `Static/js` and `Static/scss` compile into `wwwroot/dist`. The `EthernaCredit.csproj` MSBuild targets auto-run `npm install` (when `node_modules` is missing) and `npm run build-production` on both Debug and Release builds, so a plain `dotnet build` produces a working wwwroot. To iterate on JS/SCSS directly: `cd src/EthernaCredit && npm run watch`.

Running the app requires a reachable **MongoDB** instance (`ConnectionStrings`: `CreditDb`, `ServiceSharedDb`, `HangfireDb`, `DataProtectionDb`), **Elasticsearch** (`Elastic:Urls`, Serilog sink), and the **Etherna SSO server** (`SsoServer:*` settings) — see `src/EthernaCredit/appsettings.Development.json` for dev defaults. The `ASPNETCORE_ENVIRONMENT` variable is mandatory: logging configuration throws at startup without it. Outside Development the CORS policy admits only the browser origins listed in `Cors:AllowedOrigins` (`appsettings.Production.json` ships `https://etherna.io` and `https://app.etherna.io`, each entry overridable from the environment as `Cors__AllowedOrigins__<n>`), and the host refuses to start when the list is empty or an entry is not a bare web origin (`scheme://host[:port]`, http or https, no path: a trailing slash would never match a browser's `Origin` header) (`Extensions/CorsPolicyBuilderExtensions`); Development allows any origin. `Directory.Build.props` sets `NuGetAuditMode=direct`, so only direct package vulnerabilities are audited.

Docker: `docker build .` (uses `Dockerfile`, which installs Node.js 20 for the frontend build, runs `dotnet test` as part of the build stage, and exposes ports 80/443).

## Architecture

Four-project layered solution, plus two test projects. Root namespace is `Etherna.Credit[.<Layer>]` — note that project folders are named `EthernaCredit.*` but namespaces use `Etherna.Credit.*`; under that root, the namespace mirrors the folder path.

- **`src/EthernaCredit.Domain`** (`Etherna.Credit.Domain`) — Pure domain layer. Entities under `Models/`: `User` (flat), the polymorphic operation logs under `Models/OperationLogs/` (`AdminUpdateOperationLog`, `DepositOperationLog`, `UpdateOperationLog`, `WelcomeCreditDepositOperationLog`, `WithdrawOperationLog`, base `OperationLogBase`), and the `Models/UserAgg/` aggregate folder (`UserBalance`, `UserSharedInfo`). Base classes: `ModelBase`, `EntityModelBase`, `EntityModelBase<TKey>`. Domain events under `Events/` (`AdminUpdateUserBalanceEvent`, `UserDepositEvent`, `UserWithdrawEvent`) are dispatched via `Etherna.DomainEvents`. Exposes the `ICreditDbContext` interface (`Users`, `OperationLogs`, `EventDispatcher`), `ICreditDbContextInternal` (adds `UserBalances` — never use it directly, interact through `IUserService`), and `ISharedDbContext` (`UsersInfo` — read-only here, the data is owned by the SSO server). Depends on `Etherna.Scrinium.Core`, `Etherna.DomainEvents`, and `SwarmSdk` (the `EthAddress` and `XDaiValue` value types).
- **`src/EthernaCredit.Persistence`** (`Etherna.Credit.Persistence`) — Scrinium implementation. `CreditDbContext` (main) and `SharedDbContext` (shared with the other Etherna services) declare the repositories and their indexes; `ModelMaps/` (Credit, Shared) defines how each entity serializes (`BeeNetMap` registers the custom serializer maps for the SwarmSdk value types); `Repositories/DomainRepository` is a generic repository that dispatches domain events on create/delete; `Serializers/` holds the custom BSON serializers `EthAddressSerializer` and `XDaiValueSerializer` (stored as `Decimal128`). References `EthernaCredit.Domain` only.
- **`src/EthernaCredit.Services`** (`Etherna.Credit.Services`) — Application services and side effects. `Domain/` holds `IUserService`/`UserService`, the single entry point for user lookup/creation and balance reads/updates. `EventHandlers/` follows the `On<Event>Then<Action>Handler` convention and is auto-discovered by reflection in `ServiceCollectionExtensions.AddDomainServices` — any `IEventHandler` placed in this namespace is registered automatically (currently the deposit/withdraw email notification handlers). `Tasks/Infrastructure/Cron/` holds the Hangfire jobs (`CleanupOldFailedTasksTask`) and `Tasks/Queues.cs` the queue names. `Settings/` holds configuration objects (`SsoServerSettings`). `Views/Emails/` holds the Razor email templates (`DepositConfirmation`, `WithdrawalConfirmation`) rendered by the event handlers — this is why the project uses the Razor SDK. References `EthernaCredit.Domain`.
- **`src/EthernaCredit`** (`Etherna.Credit`) — ASP.NET Core host (Minimal APIs + Razor Pages). `Program.cs` wires everything: authentication against Etherna SSO, authorization policies, Scrinium (scoped db contexts over singleton engines) with Hangfire (Mongo storage), Serilog → Elasticsearch, OpenAPI + Scalar API Reference, CORS (policy composed by `Extensions/CorsPolicyBuilderExtensions`), data protection (keys in Mongo), and seeds the DbContexts at startup. Areas: `Api` (the minimal API), `Account` (login/logout), `Admin` (user administration and manual balance updates, gated by `RequireAdministratorRolePolicy`), `Deposit`/`Manage`/`Withdraw` (authorized user pages with the deposit/withdraw flows). Cross-cutting host code is under `Configs/` (consts, authorization handlers, dashboard auth filters, OpenAPI transformers) and `Extensions/`.
- **`test/EthernaCredit.Persistence.Tests`** (xUnit + Moq) — focuses on model-map deserialization: each `ModelMap` GUID/schema version is pinned by a stored BSON document and asserted to deserialize into the expected entity (`CreditDbContextDeserializationTest`, `SharedDbContextDeserializationTest`, helpers under `Helpers/`).
- **`test/EthernaCredit.Tests`** (xUnit + Moq) — mirrors the host project: the CORS policy test (`Extensions/CorsPolicyBuilderExtensionsTest`) builds the policy from an in-memory configuration and evaluates real requests through the framework's `CorsService`, asserting on the response headers; the admin user page model test (`Areas/Admin/Pages/Users/UserModelTest`) runs `OnGetAsync` over mocked db contexts and pins the text the view shows (views keep no logic of their own: what they print is composed by the page model, where it can be tested); the API exception mapping test (`Areas/Api/ExceptionHandlerTest`) throws real driver exceptions through `ExceptionHandler.RunAsync` and asserts on the status code of the result.

### API design

The API is built with **Minimal APIs**, not MVC controllers — this is the central pattern to follow when adding endpoints:

- Routes are registered in the static `CreditApiMapper` class and mounted from `Program.ConfigureApplication` via `app.MapCreditApi()`, which builds a `RouteGroupBuilder` on `/api/v0.3` tagged with the `CreditApiMarker` metadata.
- Every route delegates to the handler interface `ICreditApiHandler`, implemented by the sealed `CreditApiHandler` (registered `AddScoped` in `Program`). Handler methods return `Task<IResult>` and wrap their body in `ExceptionHandler.RunAsync(async () => { … })`, which logs the exception and maps it to the status code (400 argument/format/invalid entity type, 401 unauthorized, 404 not found, 409 a Mongo `TransientTransactionError` the db context couldn't retry away, so the client can repeat the request, 503 Swarm api errors, 500 anything else).
- The API has two surfaces, split by authorization policy: `user/…` endpoints act on the authenticated user (`UserInteractApiScopePolicy`: user JWT with scope `userApi.credit`), while `serviceInteract/users/{address}/…` endpoints are how the other Etherna services read and update a user's credit (`ServiceInteractApiScopePolicy`: service JWT with scope `ethernaCredit_serviceInteract_api`). On an address unknown to the shared users the service reads answer an empty result (zero balance, no logs), while a balance update answers 404.
- JSON serialization is centralized in `Program` via `ConfigureHttpJsonOptions` (camelCase, `JsonStringEnumConverter`, plus the SwarmSdk `EthAddress`/`XDaiValue` converters with `NumericFormat.AsFloat`) — handlers return plain `Results.Json(dto)`/`Results.Ok()` with no per-call serializer options.
- DTOs use the `Dto` suffix (`Areas/Api/DtoModels/`: `CreditDto`, `OperationLogDto`); request data is bound from route/query parameters (with validation attributes in the mapper). Deprecated endpoints stay mapped and get `.IsDeprecated(...)` route metadata instead of being removed.
- There is a single OpenAPI document, `Credit03`, filtered to the marker via `MetadataFilterDocumentTransformer<CreditApiMarker>`, served to Scalar at `/scalar/credit03` (OAuth2 authorization code + PKCE against SSO). Transformers live in `Configs/OpenApi/`.

### Key cross-cutting points

- **Money and addresses are SwarmSdk value types.** Balances are `XDaiValue` and users are identified by `EthAddress`. In BSON they serialize via the custom serializers in `Persistence/Serializers/` (`XDaiValue` as `Decimal128`), registered in `ModelMaps/Credit/BeeNetMap`; in JSON via the converters wired in `Program.ConfigureHttpJsonOptions` (xDAI as float).
- **`UserBalance` lives outside Scrinium change tracking.** It is "unmanaged from domain space": mutations happen only through `IUserService.TryIncrementUserBalanceAsync`, which performs an atomic `FindOneAndUpdate`/`$inc` directly on the Mongo collection — the `balance.Credit >= -amount` filter guards against overdrafts, and the returned `bool` reports whether the update applied. Users with `HasUnlimitedCredit` bypass balance updates entirely. Never touch `ICreditDbContextInternal.UserBalances` from anywhere else.
- **Every balance change records an `OperationLog`.** The `OperationLogBase` hierarchy is a create-only audit trail: API balance updates write an `UpdateOperationLog` (author = calling service's client id), the deposit/withdraw Razor flows write `DepositOperationLog`/`WithdrawOperationLog`, admin edits write `AdminUpdateOperationLog`, and first contact writes a `WelcomeCreditDepositOperationLog`.
- **Users are auto-created on first access.** `IUserService.FindUserAsync` creates the `User`, its `UserBalance` with the welcome credit, and the welcome-credit log when no user exists for the given `UserSharedInfo` — there is no explicit registration step in this service.
- **User data is split across two databases**: the credit-specific `User` entity (CreditDb) references `UserSharedInfo` (Ether address, previous addresses, ban state — ServiceSharedDb, owned by SSO and read-only here). Resolve both together via `IUserService.FindUserAsync`; address lookups must also check `EtherPreviousAddresses` (see `TryFindUserSharedInfoByAddressAsync`). For an address unknown to the shared users the `Find…` methods of `IUserService` throw `KeyNotFoundException` (404 on the API) and the `TryFind…` ones return null: the not found case is decided on the query result, never by catching the driver's exceptions. `SharedDbContext` is registered read-only (`IsReadOnly`) and declares no index builders: the SSO owns that database, its indexes, seeding and migrations, and any write from Credit throws `UnauthorizedAccessException`.
- **Domain events drive the side effects.** The deposit/withdraw/admin flows dispatch `UserDepositEvent`/`UserWithdrawEvent`/`AdminUpdateUserBalanceEvent` explicitly via `IEventDispatcher` after writing the operation log; `DomainRepository` additionally dispatches `EntityCreatedEvent`/`EntityDeletedEvent` on create/delete, and `CreditDbContext.SaveChangesAsync` dispatches events accumulated on entities. Handlers in `Services/EventHandlers/` are auto-discovered by reflection — the current ones render the `Views/Emails/` Razor templates via `IRazorViewRenderer` and send them via `IEmailSender` (both from `Etherna.ACR`), resolving the recipient through `IEthernaInternalSsoClient`.
- **Scrinium change tracking is snapshot based, with no annotations.** Loaded entities are proxies generated at compile time by the Scrinium source generator (shipped with the `Etherna.Scrinium.Core` package referenced by the Domain project): keep every property and method of a model `virtual`, so the proxy can intercept it for lazy loading and change candidate marking. On save, only the members whose serialization differs from the loaded document are written — the admin toggle of `User.HasUnlimitedCredit` (`Areas/Admin/Pages/Users/User`) is the only tracked mutation today, logs are create-only and balances bypass tracking.
- **Db contexts are scoped.** Every consumer must be scoped or transient: a singleton capturing a db context pins it (and its identity map) for the process lifetime, and scope validation is on in every environment (`Program.Main`), so it throws at resolve. Inside a scope one document materializes one instance, and a same-scope find of a just created model returns the created instance itself (no refind after create). Entity models are always referenced, never embedded: `User` is referenced by `OperationLogBase.User` and `UserBalance.User` through `UserMap.ReferenceSerializer` (id only). Users are never deleted, so that reference declares no origin delete policy; declare one explicitly (`config.OriginDelete`) for any new reference to a deletable entity, since the library default (`RemoveReference`) writes `null` into a non-nullable member.
- **Implicit lazy loads throw.** Both db contexts run with `ImplicitLazyLoad = ReactionMode.Throw` (`Program.cs`): reading a member a summary doesn't carry throws `ScriniumLazyLoadingException` instead of querying silently. The `User` summary inside a loaded log or balance carries only the id: read anything else from the full `User` resolved by `IUserService` (the deposit/withdraw/admin flows pass that instance into the log they create, so the email handlers read `OperationLog.User.SharedInfoId` without a load), or preload with `dbContext.LoadValuesAsync`.
- **Hangfire tasks get their own db scope and execution contexts.** Each job runs in its own DI scope (a fresh db context) with an async-local execution context opened by the Scrinium Hangfire filter, plus the one of the Etherna.DomainEvents library opened by `Configs/Hangfire/DomainEventsExecutionContextFilter` (its event dispatcher throws `ExecutionContextNotFoundException` without it, and a job inherits none from a request): there is nothing to open by hand in a task.
- **Model map IDs are fresh random GUIDs.** Every `MapRegistry.AddModelMap<T>("<guid>")` call (and every map inside a `ReferenceSerializer` config) needs a brand-new, randomly generated GUID (e.g. `uuidgen`) that collides with no existing map ID anywhere in the solution — never copy, edit, or hand-craft one. The ID permanently identifies that schema version; a collision silently corrupts serialization. When you add or change a map, add a matching deserialization test in `EthernaCredit.Persistence.Tests` that pins the GUID against a sample document.
- **Index definitions are strongly typed.** In `CreditDbContext`/`SharedDbContext` index builders, always select fields with lambda expressions, never magic strings (current unique indexes: `Users` on `SharedInfoId`, `UserBalances` on `User.Id`, `UsersInfo` on `EtherAddress`).
- **Hangfire queues and jobs.** Queues are declared in `Services/Tasks/Queues.cs` (`DB_MAINTENANCE`) and pinned in `Program.AddHangfireServer` alongside `"default"`. The Hangfire **server is not started in Staging** (see the `!env.IsStaging()` guard). The single recurring job, `CleanupOldFailedTasksTask`, is registered in `Program.ConfigureApplication` via `RecurringJob.AddOrUpdate` using the task's `TaskId` const. Dashboards: Hangfire at `/admin/hangfire`, Scrinium at `/admin/db` (both admin-gated).
- **Authentication uses a policy scheme** (`CommonConsts.UserAuthenticationPolicyScheme`): requests with `Authorization: Bearer …` go to JWT bearer (authority = SSO, audience `userApi`); otherwise fall back to the shared cookie (`ethernaSharedCookie`, domain `.etherna.io` in production) with OpenID Connect challenge against Etherna SSO. Service-to-service calls use a separate JWT scheme (audience `ethernaCreditServiceInteract`). The default policy adds `DenyBannedAuthorizationRequirement`; `/api` requests get a `401` instead of a login redirect. Razor Pages areas are authorized by folder convention in `Program` (Admin additionally requires the administrator role).

## Issue tracker

Bugs and features are tracked in Jira project **EC** (https://etherna.atlassian.net/projects/EC). Branch names follow `feature/EC-<id>-<slug>` / `improve/EC-<id>-<slug>` / `fix/EC-<id>-<slug>` — match this when creating branches. Release hotfixes use `hotfix/<version>` (e.g. `hotfix/0.3.14`); `dev` is the integration branch, `main` is production.

# Coding Style

## General Principles

- Keep commits clean: only include changes strictly necessary for the task at hand.
- Never reference AI agents or assistants in commits or code — no agent names, no `Co-Authored-By` agent trailers, no "generated/assisted by" notes. Commit messages and code must read as the team's own work.
- Exceptions to these conventions are accepted when strictly necessary or when they significantly improve code quality. Justify with a comment where needed.
- All elements (usings, properties, methods, fields, enum members, etc.) are always alphabetically ordered within their respective sections.
- Primary constructors are preferred everywhere the constructor is a simple parameter assignment.
- Keep code clean: remove unused variables, dead code, and redundant imports.
- Every source file starts with the standard AGPL-3.0 copyright header (`// Copyright 2021-present Etherna SA` … see any existing file).

## Naming

- **Classes/Structs**: PascalCase (`User`, `UserBalance`, `DepositOperationLog`)
- **Interfaces**: `I` prefix (`IUserService`, `ICreditDbContext`)
- **Async methods**: always `Async` suffix (`GetUserCreditAsync`, `SaveChangesAsync`)
- **Properties**: PascalCase, boolean `Has`/`Is` prefix (`HasUnlimitedCredit`)
- **Private fields**: `_camelCase` only when backing a same-named property; otherwise plain `camelCase`
- **Primary constructor parameters**: `camelCase` without underscore
- **Constants**: PascalCase (`EventHandlersSubNamespace`), except the all-caps underscore style used for Hangfire queue names (`DB_MAINTENANCE`)
- **Enums**: PascalCase type and members
- **Namespaces**: `Etherna.Credit.<Layer>.<Feature>` (e.g. `Etherna.Credit.Domain.Models`)
- **DTOs**: `Dto` suffix (`CreditDto`, `OperationLogDto`)
- **API handlers**: `<Feature>ApiHandler` implementing `I<Feature>ApiHandler`
- **Event handlers**: `On<Event>Then<Action>Handler`
- **Hangfire tasks**: `<Name>Task` implementing `I<Name>Task`, with a `public const string TaskId`
- **Operation logs**: `<Action>OperationLog` suffix (`DepositOperationLog`)
- **Aggregate folders**: `<Name>Agg` suffix (`UserAgg`)

## Code Organization

- One class per file, filename matches class name
- Namespace mirrors folder structure exactly (under the `Etherna.Credit` root namespace)
- Block-scoped namespaces: `namespace X { ... }` — NOT file-scoped
- Using directives before the namespace block (never inside it), always alphabetically ordered and kept to the minimum necessary
- No global usings

## Comments

Principal comments (generally multiline, important):
```csharp
// Capital start, ending period.
// Continued on next line if needed.
```

Secondary/separator comments:
```csharp
//no space, no capital, no ending period
```

## Member Ordering Within a Class

Use principal-style section comments:

```csharp
// Consts.
// Fields.
// Constructors.
// Properties.
// Methods.
// Helpers.
```

## Class Design

- `internal sealed` for service implementations and event handlers
- Primary constructors everywhere the constructor is a simple assignment
- Reflection-discovered types (event handlers, model-map collectors) are matched by namespace — keep them in the expected namespace

### Domain Entity Classes

These conventions exist for the Scrinium persistence layer and apply **only** to the Scrinium-mapped entities in `EthernaCredit.Domain` (the `OperationLogBase` hierarchy, `User`, the `UserAgg` aggregates, the `ModelBase`/`EntityModelBase` bases). **Never** carry them over to DTOs, API/service classes, or any other non-domain class — those follow ordinary C# style.

- `public abstract` for base classes (`ModelBase`, `EntityModelBase`, `OperationLogBase`)
- `virtual` on all properties and methods for Scrinium proxy support (lazy loading and change tracking), except members living in memory only (`EntityModelBase.Events`): those stay non-virtual, so reading them on a summary never triggers a load
- Use `public set` by default; use `protected set` only when the property requires validation or invariant enforcement
- Protected parameterless constructor for ORM:
  ```csharp
  #pragma warning disable CS8618
  protected EntityName() { }
  #pragma warning restore CS8618
  ```
- Collection encapsulation with backing fields
- `null!` (or `default!` for value-typed wrappers) for ORM-initialized properties: `public virtual string Name { get; protected set; } = null!;`
- Collection expressions for nullable collection setters: `[..value ?? []]`

### DTOs

- DTOs are immutable data carriers, **not** domain entities: they must not adopt any of the domain-entity conventions above (no `virtual` members, no `protected`/ORM parameterless constructor, no `null!`/`default!` initializers, no `protected set`).
- Prefer a positional `record` with every value passed explicitly through its primary constructor (`public record OperationLogDto(XDaiValue Amount, string Author, …)`). Besides being simpler, this keeps OpenAPI nullability correct: System.Text.Json infers a reference type's nullability from the constructor parameter, whereas a get-only reference-type property not bound to a constructor parameter is emitted as nullable regardless of its `string`/`string?` annotation.
- A DTO must never take a domain model as a constructor parameter. Map domain → DTO at the call site (the API handler), keeping the DTO free of any domain dependency.

## Async Patterns

- Always suffix with `Async`
- `CancellationToken cancellationToken = default` as the optional last parameter (non-nullable, `default` — not `CancellationToken? = null`)
- Return `Task` or `Task<T>`, never `async void`
- No `ConfigureAwait(false)` — ASP.NET Core app

## Null Handling

- Nullable reference types enabled
- `ArgumentNullException.ThrowIfNull(param)` for parameter validation
- `is null` / `is not null` patterns
- `??` and `??=` operators
- Prefer `null` over `default` wherever the type admits it: optional parameter defaults, late-init member initializers (`= null!`, not `= default!`), returns and assignments. Keep `default` only where `null` can't apply: non-nullable value types (e.g. `CancellationToken cancellationToken = default`) and unconstrained generic type parameters.

## Formatting

- Allman braces, 4-space indentation
- Expression-bodied members for single expressions
- LINQ method chains aligned, one operation per line
- Blank line between member sections

## C# Language Features

- Pattern matching, switch expressions
- Primary constructors everywhere applicable
- Collection expressions: `[]`, `[..spread]`
- Prefer collection expressions over constructors to initialize any collection: `[]` not `new()`, `["a", "b"]` not `new List<string> { "a", "b" }`. Use a constructor only when a collection expression can't express the intent (e.g. presizing capacity with `new List<T>(capacity)`).
- Target-typed `new()` when type is clear from context (for non-collection types)
- Tuple deconstruction for multiple return values (e.g. `IUserService.FindUserAsync` returning `(User, UserSharedInfo)`)
- Lock fields: prefer the dedicated `System.Threading.Lock` type (.NET 9+) over a plain `object` — more expressive, and the compiler enforces correct `lock` usage on it.

## LINQ

- Method syntax preferred
- Query syntax only for reflection-based type discovery (e.g. the event-handler query in `ServiceCollectionExtensions`)
- Async LINQ via Scrinium: `FindOneAsync`, `QueryElementsAsync`

## Dependency Injection

- Constructor injection exclusively
- Reflection-based event handler discovery (by namespace) in `AddDomainServices`
- `AddScoped` for domain services, API handlers, and Hangfire tasks; DbContexts registered via `AddScriniumWithHangfire().AddDbContext<…>`

## Testing (xUnit + Moq)

- `[Fact]` for basic tests, `[Theory]` with `[MemberData]` for parameterized cases (e.g. the per-schema-version deserialization tables)
- AAA with section comments: `// Arrange.`, `// Action.`, `// Assert.`
- Moq for mocking: `new Mock<ICreditDbContext>()`; entities under test are often built as `Mock<TEntity>` with `Setup(...)` on virtual members
- Each test project mirrors the project under test (`EthernaCredit.Persistence.Tests` mirrors `EthernaCredit.Persistence`, `EthernaCredit.Tests` mirrors the `EthernaCredit` host); add a new mirror test project only when a layer grows logic that needs covering
- When adding or changing a `ModelMap`, add a deserialization test that pins the new map GUID against a representative stored document (see `CreditDbContextDeserializationTest`)
