using _116.Core.Application.Shared.DTOs;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="UserMapper"/>.
/// </summary>
public class UserMapperTests
{
    public UserMapperTests()
    {
        UserMapper.Configure();
    }

    [Fact]
    public void Configure_ShouldNotThrowException()
    {
        // Act & Assert
        var act = () => UserMapper.Configure();
        act.Should().NotThrow();
    }

    [Fact]
    public void ToUserResponseDto_WithAllParameters_ShouldMapCorrectly()
    {
        // Arrange
        UserEntity user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUserName("testuser")
            .AsActive()
            .AsVerified()
            .Build();

        var roles = new List<RoleDto>
        {
            new(Guid.NewGuid(), "Admin", "Administrator", true, false, null),
            new(Guid.NewGuid(), "User", "Regular user", true, false, null),
        };

        var permissions = new List<PermissionDto>
        {
            new(Guid.NewGuid(), "users", "read", "Read users", true, false, null),
            new(Guid.NewGuid(), "users", "write", "Write users", true, false, null),
        };

        var avatar = new FileDto(
            Guid.NewGuid(),
            "avatar.jpg",
            "my-avatar.jpg",
            "image/jpeg",
            "https://example.com/avatar.jpg",
            1024,
            false
        );

        // Act
        UserResponseDto result = user.ToUserResponseDto(roles, permissions, avatar);

        // Assert
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.UserName.Should().Be(user.UserName);
        result.IsActive.Should().Be(user.IsActive);
        result.IsVerified.Should().Be(user.IsVerified);
        result.AuthProvider.Should().Be(user.AuthProvider.ToString());
        result.Roles.Should().HaveCount(2);
        result.Permissions.Should().HaveCount(2);
        result.Avatar.Should().NotBeNull();
        result.Avatar!.Id.Should().Be(avatar.Id);
    }

    [Fact]
    public void ToUserResponseDto_WithNullAvatar_ShouldMapCorrectly()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithEmail("test@example.com").WithUserName("testuser").Build();

        var roles = new List<RoleDto>();
        var permissions = new List<PermissionDto>();

        // Act
        UserResponseDto result = user.ToUserResponseDto(roles, permissions, null);

        // Assert
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.UserName.Should().Be(user.UserName);
        result.Avatar.Should().BeNull();
        result.Roles.Should().BeEmpty();
        result.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void ToUserResponseDto_WithEmptyCollections_ShouldMapCorrectly()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithEmail("test@example.com").WithUserName("testuser").Build();

        var roles = new List<RoleDto>();
        var permissions = new List<PermissionDto>();

        // Act
        UserResponseDto result = user.ToUserResponseDto(roles, permissions);

        // Assert
        result.Roles.Should().BeEmpty();
        result.Permissions.Should().BeEmpty();
        result.Avatar.Should().BeNull();
    }

    [Fact]
    public void ToUserResponseDto_ShouldMapAuthProviderAsString()
    {
        // Arrange
        UserEntity user = new UserBuilder().WithAuthProvider(EnumAuthProvider.Google).Build();

        var roles = new List<RoleDto>();
        var permissions = new List<PermissionDto>();

        // Act
        UserResponseDto result = user.ToUserResponseDto(roles, permissions);

        // Assert
        result.AuthProvider.Should().Be("Google");
    }

    [Fact]
    public void ToUserResponseDto_ShouldMapAccountStatus()
    {
        // Arrange
        UserEntity activeUser = new UserBuilder().AsActive().AsVerified().Build();

        var roles = new List<RoleDto>();
        var permissions = new List<PermissionDto>();

        // Act
        UserResponseDto result = activeUser.ToUserResponseDto(roles, permissions);

        // Assert
        result.IsActive.Should().BeTrue();
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void ToUserResponseDto_ShouldMapPhoneNumberFields()
    {
        // Arrange
        UserEntity user = new UserBuilder().Build();
        user.UpdatePhoneNumber("United States", "US", "+1", "+1234567890", "***-***-7890");

        var roles = new List<RoleDto>();
        var permissions = new List<PermissionDto>();

        // Act
        UserResponseDto result = user.ToUserResponseDto(roles, permissions);

        // Assert
        result.FullPhoneNumber.Should().Be("+1234567890");
        result.PartialPhoneNumber.Should().Be("***-***-7890");
        result.CountryDialCode.Should().Be("+1");
        result.CountryName.Should().Be("United States");
        result.CountryIsoCode.Should().Be("US");
    }

    [Fact]
    public void ToFileDto_WithValidFileEntity_ShouldMapCorrectly()
    {
        // Arrange
        FileEntity fileEntity = new FileBuilder()
            .WithFileName("test.jpg")
            .WithOriginalFileName("my-test.jpg")
            .WithMimeType("image/jpeg")
            .WithStorageUrl("https://example.com/test.jpg")
            .WithSizeInBytes(2048)
            .Build();

        // Act
        FileDto? result = fileEntity.ToFileDto();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(fileEntity.Id);
        result.FileName.Should().Be(fileEntity.FileName);
        result.OriginalFileName.Should().Be(fileEntity.OriginalFileName);
        result.MimeType.Should().Be(fileEntity.MimeType);
        result.StorageUrl.Should().Be(fileEntity.StorageUrl);
        result.SizeInBytes.Should().Be(fileEntity.SizeInBytes);
    }

    [Fact]
    public void ToFileDto_WithNullFileEntity_ShouldReturnNull()
    {
        // Arrange
        FileEntity? fileEntity = null;

        // Act
        FileDto? result = fileEntity.ToFileDto();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToUserResponseDto_ShouldMapTimestamps()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-10);
        DateTime updatedAt = DateTime.UtcNow;

        UserEntity user = new UserBuilder().Build();

        // Set timestamps directly
        user.CreatedAt = createdAt;
        user.UpdatedAt = updatedAt;

        var roles = new List<RoleDto>();
        var permissions = new List<PermissionDto>();

        // Act
        UserResponseDto result = user.ToUserResponseDto(roles, permissions);

        // Assert
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }
}
