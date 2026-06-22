# Infra Spec 01 — Seeding helpers

## Problem
~200 tests repeat the same create-context / add / save boilerplate, ~2000 lines
of duplication.

## Before
```csharp
await using var seedContext = CreateDbContext<ContentDbContext>();
var customer = CustomerFactory.Create();
seedContext.Customers.Add(customer);
await seedContext.SaveChangesAsync();
```

## After — add to `BaseApiTest`
```csharp
protected async Task SeedAsync<TDbContext>(Func<TDbContext, Task> seed)
    where TDbContext : DbContext
{
    await using var ctx = CreateDbContext<TDbContext>();
    await seed(ctx);
    await ctx.SaveChangesAsync();
}

protected async Task<T> SeedAsync<TDbContext, T>(Func<TDbContext, T> seed)
    where TDbContext : DbContext
{
    await using var ctx = CreateDbContext<TDbContext>();
    var entity = seed(ctx);
    await ctx.SaveChangesAsync();
    return entity;
}
```

Usage:
```csharp
var customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
{
    var c = CustomerFactory.Create();
    ctx.Customers.Add(c);
    return c;
});
```

## TODO checklist
- [ ] Add `SeedAsync<TDbContext>` + `SeedAsync<TDbContext,T>` to `BaseApiTest`.
- [ ] Adopt incrementally during the per-module assertion rewrites (don't do a
      separate mass pass — fold it into each module).

## Acceptance
- New/updated tests use `SeedAsync(...)` instead of inline context boilerplate.
