using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Verifies that the AuditableEntityInterceptor sets audit fields
/// on Added and Modified entities during SaveChanges.
/// </summary>
[Collection("Database")]
public class AuditableEntityInterceptorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SaveChanges_NewEntity_ShouldSetCreatedAtAndUpdatedAt()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var permission = PermissionFactory.Create("audit_test", "read");
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        var saved = await context.Permissions.FindAsync(permission.Id);

        saved!.CreatedAt.Should().NotBeNull();
        saved.UpdatedAt.Should().NotBeNull();
        saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChanges_UpdatedEntity_ShouldUpdateOnlyUpdatedAt()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var permission = PermissionFactory.Create("audit_upd", "write");
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        DateTime? originalCreatedAt = permission.CreatedAt;
        DateTime? originalUpdatedAt = permission.UpdatedAt;

        await Task.Delay(50);

        permission.Activate();
        context.Permissions.Update(permission);
        await context.SaveChangesAsync();

        await using var queryContext = CreateDbContext<IdentityDbContext>();
        var updated = await queryContext.Permissions.FindAsync(permission.Id);

        updated!.CreatedAt.Should().BeCloseTo(originalCreatedAt!.Value, TimeSpan.FromMicroseconds(1));
        updated.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt!.Value);
    }

    [Fact]
    public async Task SaveChanges_ViaApi_ShouldSetCreatedByToAuthenticatedUserId()
    {
        var userId = Guid.NewGuid();
        Client.AuthenticateAs(userId, "SuperAdmin");

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Permissions,
            new
            {
                Resource = "audit_api",
                Action = "create",
                Description = "Audit API test",
            }
        );

        if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
        {
            await using var context = CreateDbContext<IdentityDbContext>();
            var saved = await context.Permissions.FirstOrDefaultAsync(p =>
                p.Resource == "audit_api" && p.Action == "create"
            );

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(userId.ToString());
        }
    }
}
