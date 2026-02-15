using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
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
        email.Should().Contain("@");
        email.Should().Contain(".");
        email.Should().Be("superadmin@116.com");
    }

    [Fact]
    public void Username_ShouldBeNonEmpty()
    {
        // Arrange & Act
        string username = SuperAdminConfiguration.Username;

        // Assert
        username.Should().NotBeNullOrWhiteSpace();
        username.Should().Be("sigmacool");
    }

    [Fact]
    public void RoleDescription_ShouldDescribeRole()
    {
        // Arrange & Act
        string description = SuperAdminConfiguration.RoleDescription;

        // Assert
        description.Should().NotBeNullOrWhiteSpace();
        description.Should().Contain("Super Administrator");
        Assert.Contains("complete system access", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PermissionResource_ShouldBeSystem()
    {
        // Arrange & Act
        string resource = SuperAdminConfiguration.PermissionResource;

        // Assert
        resource.Should().Be("system");
    }

    [Fact]
    public void PermissionAction_ShouldBeAll()
    {
        // Arrange & Act
        string action = SuperAdminConfiguration.PermissionAction;

        // Assert
        action.Should().Be("all");
    }

    [Fact]
    public void PermissionDescription_ShouldDescribeSystemAccess()
    {
        // Arrange & Act
        string description = SuperAdminConfiguration.PermissionDescription;

        // Assert
        description.Should().NotBeNullOrWhiteSpace();
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
        roleName.Should().Be(nameof(EnumCoreUserRole.SuperAdmin));
        roleName.Should().Be("SuperAdmin");
    }

    [Fact]
    public void RoleName_ShouldBeNonEmpty()
    {
        // Arrange & Act
        string roleName = SuperAdminConfiguration.RoleName;

        // Assert
        roleName.Should().NotBeNullOrWhiteSpace();
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
            Action act = () => SuperAdminConfiguration.GetPassword();
            ExceptionAssertions<InvalidOperationException>? exception = act.Should()
                .ThrowExactly<InvalidOperationException>();
            exception.Which.Message.Should().Contain("DEFAULT_USER_PASSWORD");
            exception.Which.Message.Should().Contain("missing or empty");
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
            Action act = () => SuperAdminConfiguration.GetPassword();
            ExceptionAssertions<InvalidOperationException>? exception = act.Should()
                .ThrowExactly<InvalidOperationException>();
            exception.Which.Message.Should().Contain("DEFAULT_USER_PASSWORD");
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
            Action act = () => SuperAdminConfiguration.GetPassword();
            ExceptionAssertions<InvalidOperationException>? exception = act.Should()
                .ThrowExactly<InvalidOperationException>();
            exception.Which.Message.Should().Contain("DEFAULT_USER_PASSWORD");
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
            result.Should().Be(testPassword);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", originalPassword);
        }
    }

    #endregion
}
