using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminConfiguration"/>.
/// </summary>
public class SuperAdminConfigurationTests
{
    #region Constants Tests

    [Fact]
    public void Email_ShouldBeWellFormedEmailAddress()
    {
        // Arrange & Act
        string email = SuperAdminConfiguration.Email;

        // Assert
        Assert.Contains("@", email);
        Assert.Contains(".", email);
        Assert.Equal("superadmin@116.com", email);
    }

    [Fact]
    public void Username_ShouldBeNonEmpty()
    {
        // Arrange & Act
        string username = SuperAdminConfiguration.Username;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(username));
        Assert.Equal("sigmacool", username);
    }

    [Fact]
    public void RoleDescription_ShouldDescribeRole()
    {
        // Arrange & Act
        string description = SuperAdminConfiguration.RoleDescription;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
        Assert.Contains("Super Administrator", description);
        Assert.Contains("complete system access", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PermissionResource_ShouldBeSystem()
    {
        // Arrange & Act
        string resource = SuperAdminConfiguration.PermissionResource;

        // Assert
        Assert.Equal("system", resource);
    }

    [Fact]
    public void PermissionAction_ShouldBeAll()
    {
        // Arrange & Act
        string action = SuperAdminConfiguration.PermissionAction;

        // Assert
        Assert.Equal("all", action);
    }

    [Fact]
    public void PermissionDescription_ShouldDescribeSystemAccess()
    {
        // Arrange & Act
        string description = SuperAdminConfiguration.PermissionDescription;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
        Assert.Contains("system access", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("all permissions", description, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region RoleName Property Tests

    [Fact]
    public void RoleName_ShouldMatchSuperAdminEnum()
    {
        // Arrange & Act
        string roleName = SuperAdminConfiguration.RoleName;

        // Assert
        Assert.Equal(nameof(EnumCoreUserRole.SuperAdmin), roleName);
        Assert.Equal("SuperAdmin", roleName);
    }

    [Fact]
    public void RoleName_ShouldBeNonEmpty()
    {
        // Arrange & Act
        string roleName = SuperAdminConfiguration.RoleName;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(roleName));
    }

    #endregion

    #region GetPassword Method Tests

    [Fact]
    public void GetPassword_WhenEnvironmentVariableNotSet_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string? originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", null);

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => SuperAdminConfiguration.GetPassword());
            Assert.Contains("DEFAULT_USER_PASSWORD", exception.Message);
            Assert.Contains("missing or empty", exception.Message);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", originalPassword);
        }
    }

    [Fact]
    public void GetPassword_WhenEnvironmentVariableIsEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string? originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "");

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => SuperAdminConfiguration.GetPassword());
            Assert.Contains("DEFAULT_USER_PASSWORD", exception.Message);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", originalPassword);
        }
    }

    [Fact]
    public void GetPassword_WhenEnvironmentVariableIsWhitespace_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string? originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "   ");

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => SuperAdminConfiguration.GetPassword());
            Assert.Contains("DEFAULT_USER_PASSWORD", exception.Message);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", originalPassword);
        }
    }

    [Fact]
    public void GetPassword_WhenEnvironmentVariableIsSet_ShouldReturnPassword()
    {
        // Arrange
        string? originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        const string testPassword = "TestPassword123!";
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", testPassword);

        try
        {
            // Act
            string result = SuperAdminConfiguration.GetPassword();

            // Assert
            Assert.Equal(testPassword, result);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", originalPassword);
        }
    }

    #endregion
}
