using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using MapsterMapper;
using IdentityMappingRegistration = _116.Identity.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Identity.Mappers;

/// <summary>
/// Integration tests for <see cref="RoleMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class RoleMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(IdentityMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToRoleDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var entity = RoleFactory.Create("Editor", "Can edit content");
        seedContext.Roles.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        RoleEntity loaded = await readContext.Roles.FirstAsync(r => r.Id == entity.Id);

        RoleDto dto = loaded.ToRoleDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("Editor");
        dto.Description.Should().Be("Can edit content");
        dto.IsActive.Should().Be(loaded.IsActive);
        dto.IsDeleted.Should().Be(loaded.IsDeleted);
        dto.DeletedAt.Should().Be(loaded.DeletedAt);
    }

    [Fact]
    public async Task ToPermissionDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var entity = PermissionFactory.Create("articles", "read", "Read articles");
        seedContext.Permissions.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        PermissionEntity loaded = await readContext.Permissions.FirstAsync(p => p.Id == entity.Id);

        PermissionDto dto = loaded.ToPermissionDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Resource.Should().Be("articles");
        dto.Action.Should().Be("read");
        dto.Description.Should().Be("Read articles");
        dto.IsActive.Should().Be(loaded.IsActive);
        dto.IsDeleted.Should().Be(loaded.IsDeleted);
        dto.DeletedAt.Should().Be(loaded.DeletedAt);
    }

    [Fact]
    public async Task ToRoleWithPermissionsDto_ShouldMapRoleAndPermissions()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var role = RoleFactory.Create("Moderator", "Can moderate content");
        seedContext.Roles.Add(role);

        var permission1 = PermissionFactory.Create("posts", "delete", "Delete posts");
        var permission2 = PermissionFactory.Create("comments", "delete", "Delete comments");
        seedContext.Permissions.AddRange(permission1, permission2);

        var rp1 = RolePermissionFactory.Create(role.Id, permission1.Id);
        var rp2 = RolePermissionFactory.Create(role.Id, permission2.Id);
        seedContext.RolePermissions.AddRange(rp1, rp2);

        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        RoleEntity loaded = await readContext
            .Roles.Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == role.Id);

        RoleWithPermissionsDto dto = loaded.ToRoleWithPermissionsDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("Moderator");
        dto.Description.Should().Be("Can moderate content");
        dto.IsActive.Should().Be(loaded.IsActive);
        dto.Permissions.Should().HaveCount(2);
    }
}
