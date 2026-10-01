# Application Services

How the modules name, place and size the classes that sit between a handler and the
infrastructure. The rules are enforced by `tests/Architecture.Tests/ServiceTaxonomyTests.cs` and
`HandlerDependencyBudgetTests.cs`.

---

## Two kinds of service

| | Interface declared in | Implemented in | Coordinates | Example |
| --- | --- | --- | --- | --- |
| **Application service** | Application | **Application** | repositories, other services, entities | `PublicLoginAuthService` |
| **Infrastructure service** | Application | **Infrastructure** | one technology | `CloudinaryService`, `JwtService` |

Both end in `Service`. The folder, not the name, says which one you are looking at.

A class keeps the `Factory` suffix only when it constructs: no injected collaborator and no
`await`, or it selects an implementation at runtime. `SocialTokenVerifierFactory` and the static
`StreamingLinkFactory` qualify; the entity `Create` methods do too. A class that loads, validates,
sequences or commits is an application service.

The test to apply to a new class:

1. Does it name a vendor, a driver, a protocol, the clock or the file system? Infrastructure service.
2. Does it only sequence repositories, entities and other services? Application service.
3. Does it inject nothing and await nothing? A factory or a pure function, and it never lives under `Services/`. The Mailer renderers are the example: they read a resx catalog, so they sit in `Shared/Templates/`.

---

## Folder structure

`Ports/` means the implementation is in Infrastructure. Everywhere else in Application, the
implementation is in Application.

```
src/modules/<M>/<M>/src/
├── <M>.Application/
│   └── <Area>/                         Auth, Session, User, Catalog, Commerce, Editorial, …
│       ├── Ports/                      interfaces implemented in <M>.Infrastructure
│       ├── Repositories/               repository interfaces
│       ├── Services/                   application services shared by sibling slices: interface + implementation
│       └── UseCases/<Admin|Public>/<Commands|Queries>/<UseCase>/
│           ├── Contracts/              the slice's own application-service interface (+ its output record)
│           ├── <Admin|Public><UseCase>Service.cs
│           └── <Admin|Public><UseCase>Handler.cs
└── <M>.Infrastructure/
    └── Services/                       one implementation per Ports/ interface
```

| You are looking at | Its implementation is |
| --- | --- |
| `<Area>/Ports/IJwtService.cs` | `<M>.Infrastructure/Services/JwtService.cs` |
| `<Area>/Services/IOtpVerificationService.cs` | `<Area>/Services/OtpVerificationService.cs`, next to it |
| `UseCases/.../Contracts/IPublicLoginAuthService.cs` | `UseCases/.../PublicLoginAuthService.cs`, one folder up |
| `<M>.Contracts/Application/Services/IFileStorageService.cs` | inside `<M>`, published to other modules |

Placement of a new application service:

- Used by one slice: in the use-case folder, `Admin`/`Public`-prefixed, interface and output record
  together in the slice's `Contracts/` subfolder.
- Used by sibling slices: in `<Area>/Services/`, unprefixed, interface beside the implementation.
- Registered as `Scoped` in the module class next to the existing registrations, so it shares the
  request's `DbContext` with the repositories.

An Admin handler never injects a `Public*` service or vice versa. Logic both need goes in an
unprefixed service in `<Area>/Services/`.

---

## The handler dependency budget

A command or query handler declares at most **4** primary-constructor parameters. Every parameter
counts. Handlers over budget on the day the rule landed are listed in
`tests/Architecture.Tests/HandlerBudgetBurnDown.txt`; the list only shrinks.

A handler retrieves, delegates, applies a domain transition and commits. A fifth dependency is the
signal that it owns a phase it should delegate. The four shapes that phase takes:

| Species | Owns | Example |
| --- | --- | --- |
| **Protocol service** (shared) | A multi-step sequence used by sibling slices | `OtpVerificationService`, `SessionService`, `RefreshTokenRotationService` |
| **Resolution service** (per slice) | Loading and validating the use case's associations, throwing the localized error | `PublicLoginAuthService`, `AdminAssignRoleToUserService` |
| **Security fan-out service** (shared) | The reaction that must follow a credential change | `CredentialInvalidationService` |
| **Response service** (shared) | Assembling a response DTO from an aggregate plus its lookups | `CategoryDtoService`, `VideoDtoService` |

What an extraction must never do:

- A pass-through wrapper with one delegating method. The count drops on paper; coupling is unchanged.
- A service that executes the whole use case. The handler becomes a pass-through and SRP moves down a level.
- A facade bundling unrelated seams (`IAuthServices` exposing repository + password service + i18n).
- Domain rules inside the service. Aggregates decide; a service calls a guarded transition, never re-implements one.
- A service hiding a repository the handler also injects for the same read. Each read has one owner.

Transaction boundaries never move in an extraction: the handler that committed before commits
after. A service commits mid-flow only where the flow already did (`OtpVerificationService` commits
the metered miss before throwing).

---

## Anatomy of a slice-local service

```
UseCases/Admin/Commands/AssignRoleToUser/
├── Contracts/
│   └── IAdminAssignRoleToUserService.cs    interface + output record, one file
├── AdminAssignRoleToUserService.cs          implementation
├── AdminAssignRoleToUserHandler.cs          injects the service
├── AdminAssignRoleToUserCommand.cs
├── AdminAssignRoleToUserValidator.cs
└── V1/
    └── AdminAssignRoleToUserEndpointV1.cs
```

```csharp
// Contracts/IAdminAssignRoleToUserService.cs
public record RoleGrantData(UserEntity User, RoleEntity Role);

public interface IAdminAssignRoleToUserService
{
    Task<RoleGrantData> GrantAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
}
```

```csharp
// AdminAssignRoleToUserService.cs
public class AdminAssignRoleToUserService(
    IRoleRepository roleRepository,
    IAuthRepository authRepository,
    IdentityI18n i18n
) : IAdminAssignRoleToUserService
{
    public async Task<RoleGrantData> GrantAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(roleId, cancellationToken);

        if (role!.IsDeleted)
        {
            throw i18n.User.RoleIsDeleted();
        }

        if (!role.IsActive)
        {
            throw i18n.User.RoleIsInactive();
        }

        UserEntity? user = await authRepository.GetUserWithRolesByIdOrThrow(userId, cancellationToken);

        if (!user!.GrantRole(roleId, role.Name))
        {
            throw i18n.User.RoleAlreadyAssignedToUser();
        }

        return new RoleGrantData(User: user, Role: role);
    }
}
```

```csharp
// AdminAssignRoleToUserHandler.cs — 4 dependencies
public class AdminAssignRoleToUserHandler(
    IAdminAssignRoleToUserService assignRoleService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAssignRoleToUserCommand, AdminAssignRoleToUserResult>
{
    public async Task<AdminAssignRoleToUserResult> Handle(AdminAssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        Guid userId = Guid.Parse(command.UserId);

        RoleGrantData grant = await assignRoleService.GrantAsync(userId, command.RoleId, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        await tokenStateRepository.BumpTokenVersionAsync(userId, cancellationToken);

        return new AdminAssignRoleToUserResult(Roles: /* map grant.User.UserRoles */);
    }
}
```

Rules:

- The output record lives in the same file as the interface and uses named positional properties.
- The service throws the localized error; the handler never re-checks what the service gated.
- The handler owns the commit unless the flow already committed mid-way before the extraction.

---

## Tests

- An application service gets a unit test file owning its branch matrix with mocked seams
  (`AdminAssignRoleToUserServiceTests`).
- The handler test mocks the service and covers orchestration only: the call, the commit, any
  post-commit side effect, the response, and that a refusal from the service commits nothing
  (`AdminAssignRoleToUserHandlerTests`).
- Integration tests are untouched by an extraction: they drive the endpoint, and the wire behaviour
  is the same before and after.

---

## Decision guide

| Situation | Decision |
| --- | --- |
| The handler calls one repository and maps | No service |
| The handler would hold a fifth dependency | Extract the phase that owns the extra seam |
| Logic is identical for the Public and Admin variants | One unprefixed service in `<Area>/Services/` |
| Logic differs per variant | One prefixed service per slice |
| The class names a technology | Interface in `<Area>/Ports/`, implementation in `<M>.Infrastructure/Services/` |
| The class injects nothing and awaits nothing | A factory or a static helper, not a service |
