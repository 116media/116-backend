# Reference: `EShopModularMonoliths`

Path: `/Users/coolbeatz/projects/116/116/EShopModularMonoliths`

## Repository layering

There is no generic repository. `IRepository<T>` is an empty marker:

```csharp
// src/Shared/Shared/Domain/IRepository.cs
public interface IRepository<T> where T : IAggregate;
```

Each aggregate declares the exact surface it needs, and that interface lives in the **domain**,
next to the entity it serves — not in the persistence project:

```
src/Modules/Basket/Basket/
├── Domain/Basket/
│   ├── Entities/          ShoppingCartEntity, ShoppingCartItemEntity
│   ├── Repositories/      IBasketRepository          <- contract lives with the domain
│   ├── UseCases/          AddItemToBasket, GetBasket, …
│   └── Validators/
└── DataSource/
    ├── Repositories/      BasketRepository, CachedBasketRepository
    ├── JsonConverters/    ShoppingCartConverter, ShoppingCartItemConverter
    └── Specifications/    BasketByUserNameSpecification
```

```csharp
// src/Modules/Basket/Basket/Domain/Basket/Repositories/IBasketRepository.cs
public interface IBasketRepository : IRepository<ShoppingCartEntity>
{
    Task<ShoppingCartEntity> GetBasket(
        Specification<ShoppingCartEntity> specification,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<ShoppingCartEntity> CreateBasket(ShoppingCartEntity basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteBasket(Specification<ShoppingCartEntity> specification, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(string? userName = null, CancellationToken cancellationToken = default);
}
```

Two consequences worth naming, because they are what make the caching design work:

- **The interface is narrow — four methods.** A decorator has four members to implement.
- **`SaveChangesAsync` is on the repository.** There is no separate unit of work, so the repository
  observes every commit. That is precisely what allows write-through eviction below.

## Caching

Real decorator, registered with Scrutor:

```csharp
// src/Modules/Basket/Basket/BasketModule.cs:25-26
services.AddScoped<IBasketRepository, BasketRepository>();
services.Decorate<IBasketRepository, CachedBasketRepository>();
```

`Decorate` rewrites the existing `ServiceDescriptor`: consumers resolving `IBasketRepository` receive
`CachedBasketRepository`, whose own `IBasketRepository` parameter receives `BasketRepository`. No
consumer changes, and `BasketRepository` has no idea it is being wrapped.

```csharp
// src/Modules/Basket/Basket/DataSource/Repositories/CachedBasketRepository.cs
public class CachedBasketRepository(
    IBasketRepository repository,
    IDistributedCache cache
): IBasketRepository
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new ShoppingCartConverter(), new ShoppingCartItemConverter() }
    };

    public async Task<ShoppingCartEntity> GetBasket(
        Specification<ShoppingCartEntity> specification,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        if (!asNoTracking)
        {
            return await repository.GetBasket(specification, false, cancellationToken);
        }

        var userName = ExtractUserName(specification);
        var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);
        if (!string.IsNullOrEmpty(cachedBasket))
        {
            return JsonSerializer.Deserialize<ShoppingCartEntity>(cachedBasket, _options)!;
        }

        var basket = await repository.GetBasket(specification, asNoTracking, cancellationToken);
        await cache.SetStringAsync(userName, JsonSerializer.Serialize(basket, _options), cancellationToken);

        return basket;
    }

    public async Task<int> SaveChangesAsync(string? userName = null, CancellationToken cancellationToken = default)
    {
        var result = await repository.SaveChangesAsync(userName, cancellationToken);

        if (userName is not null)
        {
            await cache.RemoveAsync(userName, cancellationToken);
        }

        return result;
    }
}
```

### The three ideas that make this work

**1. The tracking flag is the cache bypass.**

```csharp
if (!asNoTracking)
{
    return await repository.GetBasket(specification, false, cancellationToken);
}
```

Any caller intending to mutate asks for a tracked entity and therefore goes straight to EF. A
deserialized, detached aggregate can never reach `SaveChanges`. This is the same hazard
`dotnet-microservices` fences with `UnTrackCacheableEntities()`, solved at the entry point
instead of the exit — and it is strictly better, because the caller states its intent.

**2. Invalidation is trivial because the key and the write are the same identity.** The cache key is
`UserName`; the writer already has the username. No eviction tokens, no pub/sub, no fan-out —
`CreateBasket` writes through, `DeleteBasket` and `SaveChangesAsync` remove.

**3. The aggregate-serialization cost is paid explicitly.** `ShoppingCartEntity` exposes
`IReadOnlyList<ShoppingCartItemEntity> Items => _items.AsReadOnly()` with no setter, so the converter
reflects into the backing field to rehydrate:

```csharp
// src/Modules/Basket/Basket/DataSource/JsonConverters/ShoppingCartConverter.cs
var shoppingCart = ShoppingCartEntity.Create(id, userName);
var items = itemsElement.Deserialize<List<ShoppingCartItemEntity>>(options);

var itemsField = typeof(ShoppingCartEntity)
    .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
itemsField?.SetValue(shoppingCart, items);
```

That is the true price of putting an aggregate in a distributed cache: a hand-written converter
per type, plus reflection wherever the domain protects its invariants. Affordable here because
`ShoppingCartEntity` is two levels deep with no cross-aggregate navigation and no domain events
to preserve.

## Take

- **Scrutor `Decorate` over an interface.** The uncached repository stays pure and independently
  testable; the cache is one line of registration and can be omitted per environment.
- **An explicit cache-bypass on the read path for callers that intend to write.**
- **Write-through eviction keyed by the same identity the writer holds.**
- **An explicit serialization contract when a non-trivial type is cached.**

## Reject

- **`SaveChangesAsync(string? userName = null, …)`.** Eviction only happens when the caller remembers
  to pass the key. Both current call sites do, so it is correct today — but the nullable default means
  a future `SaveChangesAsync()` silently leaves a stale basket in Redis with no error and no test
  failure. If the cache key is required for correctness, it must be a required parameter.
- **Reflection into private backing fields.** Works, but it couples the cache layer to a field name
  the compiler will not defend. Cache a projection instead and the problem disappears.
- **The repository owning `SaveChangesAsync`.** Fine for a single-aggregate module; it cannot express
  a transaction spanning several repositories, which this backend needs.
- **Caching an aggregate at all**, unless the type is as flat as `ShoppingCartEntity`.
