using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Domain;
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

        await using var queryContext = CreateDbContext<IdentityDbContext>();
        PermissionEntity? saved = await queryContext.Permissions.FindAsync(permission.Id);

        saved.Should().NotBeNull();
        saved!.CreatedAt.Should().NotBeNull();
        saved.UpdatedAt.Should().Be(saved.CreatedAt, "one save reads the clock once");
        saved
            .CreatedAt.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "the host registers the system clock");

        saved.CreatedBy.Should().Be(nameof(EnumAuditActor.System), "non-HTTP saves are attributed to the system actor");
        saved.UpdatedBy.Should().Be(nameof(EnumAuditActor.System));
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

        permission.Activate();
        context.Permissions.Update(permission);
        await context.SaveChangesAsync();

        await using var queryContext = CreateDbContext<IdentityDbContext>();
        PermissionEntity? updated = await queryContext.Permissions.FindAsync(permission.Id);

        updated.Should().NotBeNull();
        updated!.CreatedAt.Should().BeCloseTo(originalCreatedAt!.Value, TimeSpan.FromMicroseconds(1));
        updated.UpdatedAt.Should().BeAfter(originalUpdatedAt!.Value);
        updated.UpdatedBy.Should().Be(nameof(EnumAuditActor.System));
    }

    [Fact]
    public async Task SaveChanges_ViaApi_ShouldSetCreatedByToAuthenticatedUserId()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Permissions,
            new
            {
                Resource = "audit_api",
                Action = "create",
                Description = "Audit API test",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<IdentityDbContext>();
        PermissionEntity? saved = await context.Permissions.FirstOrDefaultAsync(p =>
            p.Resource == "audit_api" && p.Action == "create"
        );

        saved.Should().NotBeNull();
        saved!.CreatedBy.Should().Be(User.SuperAdminId.ToString());
    }
}
