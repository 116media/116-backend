# Architecture: Why Clean Architecture + Vertical Slice + Modular Monolith + DDD

## Introduction

The 116 backend is built on a combination of **Clean Architecture**, **Vertical Slice**, **Modular Monolith**, and **Domain-Driven Design (DDD)**. Each of these patterns solves a specific problem, and together they create a powerful, future-proof foundation for a platform as ambitious as 116.

---

## Clean Architecture

Clean Architecture is a way of organising code so that the **core business logic is completely independent of external frameworks, databases, or delivery mechanisms** (web, CLI, etc.).

### The Layers

```
┌──────────────────────────────────────────────┐
│                   API Layer                  │  ← HTTP, Carter endpoints
├──────────────────────────────────────────────┤
│              Application Layer               │  ← Commands, Queries, Handlers
├──────────────────────────────────────────────┤
│                Domain Layer                  │  ← Entities, Value Objects, Events
├──────────────────────────────────────────────┤
│            Infrastructure Layer              │  ← DB, Cloudinary, EF Core
└──────────────────────────────────────────────┘
```

**Dependency rule**: Inner layers know nothing about outer layers. The domain does not import EF Core. The application does not import ASP.NET Core.

### Why it matters for 116

- If we switch from PostgreSQL to another database tomorrow, only the Infrastructure layer changes.
- If we move from REST to GraphQL, only the API layer changes.
- Business rules (e.g. "only SuperAdmin can hard-delete a role") live in the Domain and are impossible to accidentally bypass.

---

## Vertical Slice Architecture

Instead of grouping all controllers together, all services together, and all repositories together, **Vertical Slice groups code by feature**.

### Traditional vs Vertical Slice

```
❌ Traditional (horizontal layers)          ✅ Vertical Slice (feature-based)
──────────────────────────────             ──────────────────────────────────
/Controllers                               /Modules/Identity/Application/
  - UsersController.cs                       /Auth/Login/
  - RolesController.cs                         - LoginCommand.cs
/Services                                      - LoginCommandHandler.cs
  - UserService.cs                             - LoginCommandValidator.cs
  - RoleService.cs                             - LoginEndpoint.cs
/Repositories                                /Users/GetUser/
  - UserRepository.cs                          - GetUserQuery.cs
  - RoleRepository.cs                          - GetUserQueryHandler.cs
```

### Why it matters for 116

- Adding a new feature (e.g. "bookmark article") only requires touching files inside that one feature folder.
- Deleting a feature means deleting one folder without risk of breaking unrelated code.
- Easier for new team members to understand: everything for one use case is in one place.

---

## Modular Monolith

A Modular Monolith is a **single deployable application divided into strongly isolated modules**, each with its own database schema, domain logic, and API.

### Current Modules

```
src/Modules/
├── Identity/      ← Users, Roles, Permissions, Sessions, OTPs
├── Core/          ← File management (Cloudinary)
└── Content/       ← (planned) Articles, Videos, Lyrics, Playlists
```

Each module:
- Has its **own DbContext** and database schema.
- Communicates with other modules only through **domain events** (not direct service calls).
- Can be **extracted into a microservice** independently when the time comes.

### Why it matters for 116

- We start with one simple deployment (no Kubernetes complexity, no network latency between services).
- When 116 scales to millions of users, the Content module can be spun off as a standalone service without rewriting it.
- Team members can own individual modules without stepping on each other.

---

## Domain-Driven Design (DDD)

DDD is a way of modelling software around **the language and rules of the business domain**, rather than around technical database tables.

### Core DDD Building Blocks Used

| Concept | Example in 116 | Purpose |
|---|---|---|
| **Aggregate** | `UserEntity`, `RoleEntity` | Root of a consistency boundary, owns domain events |
| **Entity** | `SessionEntity`, `OtpEntity` | Has identity, lives inside an aggregate |
| **Value Object** | `Email`, `AuthProvider` | Immutable, compared by value not id |
| **Domain Event** | `UserCreatedEvent` | Notifies other modules of state changes |
| **Repository** | `IAuthRepository`, `IRoleRepository` | Persistence abstraction per aggregate |
| **Specification** | `RoleByNameSpecification` | Composable, reusable query predicates |

### Why it matters for 116

- The business rules are explicit in code, not buried in SQL queries or service methods.
- The `Aggregate` base class enforces that domain events are dispatched after every state change, making the system event-driven by design.
- Value Objects prevent invalid states: you cannot create a `UserEntity` with a plain `string` email — it must go through the `Email` value object which validates format.

---

## How They Work Together

```
HTTP Request
     │
     ▼
[Carter Endpoint]           ← API Layer (Vertical Slice: one file per endpoint)
     │
     ▼
[ValidationDecorator]       ← FluentValidation runs before handler
     │
     ▼
[LoggingDecorator]          ← Logs request/response
     │
     ▼
[CommandHandler / QueryHandler]  ← Application Layer (one handler per use case)
     │
     ▼
[Domain Aggregate]          ← Business rules enforced here
     │
     ▼
[Repository / EF Core]      ← Infrastructure Layer (PostgreSQL)
     │
     ▼
[Domain Events dispatched]  ← Cross-module communication
```

### Example: Creating a Role

1. `POST /api/v1/admin/roles` hits `CreateRoleEndpointV1` (Carter)
2. Dispatcher routes to `CreateRoleCommandHandler`
3. `ValidationDecorator` validates the command via `CreateRoleCommandValidator`
4. Handler calls `RoleEntity.Create(name, description)` — domain logic runs
5. `IRoleRepository.AddAsync(role)` persists via EF Core
6. Domain event `RoleCreatedEvent` is dispatched to notify other modules

---

## Future: Path to Microservices

The Modular Monolith is designed so that each module can become a microservice:

1. Extract module folder into its own solution.
2. Replace in-process domain events with RabbitMQ/Kafka messages (outbox pattern already in place).
3. Each module already has its own PostgreSQL schema — just point it to its own database instance.
4. No business logic rewrite required.

This is why the architecture was chosen: **build simple today, scale confidently tomorrow**.