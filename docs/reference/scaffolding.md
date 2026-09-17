# Scaffolding reference

Templates for the recurring shapes. Every use-case file and type carries the `Admin`/`Public`
scope prefix; handlers hold at most 4 constructor dependencies (see CLAUDE.md).

## Command / query (CQRS)

```csharp
// Command record
public record AdminDoThingCommand(string Data) : ICommand<AdminDoThingResult>;

// Handler (primary constructor, max 4 dependencies)
public class AdminDoThingHandler(IThingRepository thingRepository, IModuleUnitOfWork unitOfWork)
    : ICommandHandler<AdminDoThingCommand, AdminDoThingResult>
{
    /// <inheritdoc />
    public async Task<AdminDoThingResult> Handle(AdminDoThingCommand command, CancellationToken cancellationToken)
    {
        // load → delegate → transition → commit
    }
}

// Validator
public class AdminDoThingValidator : AbstractValidator<AdminDoThingCommand>
{
    public AdminDoThingValidator(ModuleI18n i18n)
    {
        RuleFor(x => x.Data).NotEmpty();
    }
}
```

Queries use `IQuery<TResult>` / `IQueryHandler<TQuery, TResult>` with the same shape.

## Entity

```csharp
public class MyEntity : Aggregate<Guid>
{
    public string Name { get; private set; }

    private MyEntity() { } // EF Core

    public static MyEntity Create(string name)
    {
        return new MyEntity { Id = Guid.NewGuid(), Name = name };
    }
}
```

Configuration in `Infrastructure/Persistence/Configurations/`, a `DbSet` on the module
context, then a migration (see [`database.md`](database.md)).

## Module

Modules are **static extension classes**, not subclasses. The registration method wires the
database through the shared `BaseModule` extensions and contributes Mapster mappings:

```csharp
public static class MyModuleModule
{
    private static ModuleOptions<MyModuleDbContext> GetModuleOptions()
    {
        return new ModuleOptions<MyModuleDbContext>
        {
            ModuleName = MyModuleConstants.ModuleName,
            SchemaName = MyModuleConstants.SchemaName,
        };
    }

    public static IServiceCollection AddMyModuleModule(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddModuleDatabase(GetModuleOptions());
        services.AddModuleMappings(new MappingRegistration());
        // repositories, factories, handlers, event handlers…
        return services;
    }
}
```

Registered in `src/Api/Program.cs` alongside the other modules.

## Exceptions

- Custom exceptions inherit `BaseException` (code, message, HTTP status); module-specific ones
  live under `Application/Exceptions/`.
- Handlers never construct or throw exceptions inline — i18n error factories
  (`throw i18n.User.SomethingWrong()`) or repository `*OrThrowAsync` guards own the throw.
- A custom mapping to `ProblemDetails` is an `IExceptionStrategy`:

```csharp
public class MyExceptionStrategy : IExceptionStrategy
{
    public bool CanHandle(Exception ex) => ex is MyException;
    public ProblemDetails Handle(Exception ex) { /* ... */ }
}
```

Everything unhandled is converted to `ProblemDetails` by the global exception middleware.
