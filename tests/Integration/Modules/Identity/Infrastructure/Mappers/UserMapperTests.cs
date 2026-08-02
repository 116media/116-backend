using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using MapsterMapper;
using IdentityMappingRegistration = _116.Identity.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Identity.Mappers;

/// <summary>
/// Integration tests for <see cref="UserMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class UserMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(IdentityMappingRegistration.CreateConfiguration());

    /// <summary>
    /// Verifies that ToUserResponseDto maps all fields with empty roles and permissions.
    /// </summary>
    [Fact]
    public async Task ToUserResponseDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var entity = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        UserEntity loaded = await readContext.Users.FirstAsync(u => u.Id == entity.Id);

        UserResponseDto dto = loaded.ToUserResponseDto(
            _mapper,
            roles: Array.Empty<RoleDto>(),
            permissions: Array.Empty<PermissionDto>()
        );

        dto.Id.Should().Be(loaded.Id);
        dto.Email.Should().Be(loaded.Email);
        dto.UserName.Should().Be(loaded.UserName);
        dto.AuthProvider.Should().Be(loaded.AuthProvider);
        dto.IsVerified.Should().BeTrue();
        dto.IsActive.Should().BeTrue();
        dto.Roles.Should().BeEmpty();
        dto.Permissions.Should().BeEmpty();
        dto.Avatar.Should().BeNull();
    }

    /// <summary>
    /// Verifies that ToUserResponseDto includes provided roles and permissions collections.
    /// </summary>
    [Fact]
    public async Task ToUserResponseDto_WithRolesAndPermissions_ShouldIncludeThem()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var entity = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        UserEntity loaded = await readContext.Users.FirstAsync(u => u.Id == entity.Id);

        var roles = new List<RoleDto> { new(Guid.NewGuid(), "Admin", "Administrator role", true, false, null) };

        var permissions = new List<PermissionDto>
        {
            new(Guid.NewGuid(), "users", "read", "Read users", true, false, null),
        };

        UserResponseDto dto = loaded.ToUserResponseDto(_mapper, roles, permissions);

        dto.Roles.Should().ContainSingle();
        dto.Permissions.Should().ContainSingle();
        dto.Roles.First().Name.Should().Be("Admin");
        dto.Permissions.First().Resource.Should().Be("users");
    }
}
