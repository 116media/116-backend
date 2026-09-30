using _116.Api;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Persistence;
using _116.Storage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Shared.Integration.Tests.Infrastructure.Persistence;

/// <summary>
/// Integration tests for <see cref="DatabaseMigrator" />, the entry point the <c>migrate</c> command
/// and the development host both call. It applies four module contexts in dependency order.
/// </summary>
[Collection("Database")]
public class DatabaseMigratorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task MigrateAllAsync_ShouldApplyEveryModuleContext()
    {
        await DatabaseMigrator.MigrateAllAsync(Api.Services, CancellationToken.None);

        await using ContentDbContext content = CreateDbContext<ContentDbContext>();
        await using IdentityDbContext identity = CreateDbContext<IdentityDbContext>();
        await using StorageDbContext storage = CreateDbContext<StorageDbContext>();
        await using MailerDbContext mailer = CreateDbContext<MailerDbContext>();

        (await content.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await identity.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await storage.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await mailer.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task MigrateAllAsync_RunTwice_ShouldBeANoOp()
    {
        await DatabaseMigrator.MigrateAllAsync(Api.Services, CancellationToken.None);

        Func<Task> act = async () => await DatabaseMigrator.MigrateAllAsync(Api.Services, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
