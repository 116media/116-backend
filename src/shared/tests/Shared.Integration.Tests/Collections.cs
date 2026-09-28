using _116.Tests.Fixtures;
using Xunit;

namespace _116.Shared.Integration.Tests;

/// <summary>
/// The test collections this suite joins. xunit resolves a collection definition only inside the assembly
/// that uses it, so every suite declares its own; the fixtures themselves live in the shared boot library.
/// </summary>
[CollectionDefinition("AccountRateLimiting", DisableParallelization = true)]
public sealed class AccountRateLimitingCollection : ICollectionFixture<AccountRateLimitedPostgresFixture>;

[CollectionDefinition("Cors", DisableParallelization = true)]
public sealed class CorsCollection : ICollectionFixture<CorsPostgresFixture>;

[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<PostgresFixture>;

[CollectionDefinition("OtpPepperless", DisableParallelization = true)]
public sealed class OtpPepperlessCollection : ICollectionFixture<OtpPepperlessPostgresFixture>;

[CollectionDefinition("RateLimiting", DisableParallelization = true)]
public sealed class RateLimitingCollection : ICollectionFixture<RateLimitedPostgresFixture>;
