using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="VisitorPermissions"/> static class.
/// </summary>
public class VisitorPermissionsTests
{
    #region Content Permissions Tests

    [Fact]
    public void Content_ShouldContain3Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Content;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(3, permissions.Length);
    }

    [Fact]
    public void Content_ShouldContainArticlesReadPermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Content;

        // Assert
        permissions.Should().Contain(p => p.Resource == "articles" && p.Action == "read");
    }

    [Fact]
    public void Content_ShouldContainVideosReadPermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Content;

        // Assert
        permissions.Should().Contain(p => p.Resource == "videos" && p.Action == "read");
    }

    [Fact]
    public void Content_ShouldContainContentsReadPermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Content;

        // Assert
        permissions.Should().Contain(p => p.Resource == "contents" && p.Action == "read");
    }

    #endregion

    #region Profile Permissions Tests

    [Fact]
    public void Profile_ShouldContain2Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Profile;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(2, permissions.Length);
    }

    [Fact]
    public void Profile_ShouldContainOwnProfileReadPermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Profile;

        // Assert
        permissions.Should().Contain(p => p.Resource == "own_profile" && p.Action == "read");
    }

    [Fact]
    public void Profile_ShouldContainOwnProfileUpdatePermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Profile;

        // Assert
        permissions.Should().Contain(p => p.Resource == "own_profile" && p.Action == "update");
    }

    #endregion

    #region Likes Permissions Tests

    [Fact]
    public void Likes_ShouldContain3Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Likes;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(3, permissions.Length);
    }

    #endregion

    #region Comments Permissions Tests

    [Fact]
    public void Comments_ShouldContain4Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Comments;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(4, permissions.Length);
    }

    #endregion

    #region Bookmarks Permissions Tests

    [Fact]
    public void Bookmarks_ShouldContain4Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Bookmarks;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(4, permissions.Length);
    }

    #endregion

    #region Navigation Permissions Tests

    [Fact]
    public void Navigation_ShouldContain2Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Navigation;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(2, permissions.Length);
    }

    [Fact]
    public void Navigation_ShouldContainTagsReadPermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Navigation;

        // Assert
        permissions.Should().Contain(p => p.Resource == "tags" && p.Action == "read");
    }

    [Fact]
    public void Navigation_ShouldContainCategoriesReadPermission()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Navigation;

        // Assert
        permissions.Should().Contain(p => p.Resource == "categories" && p.Action == "read");
    }

    #endregion

    #region Playlists Permissions Tests

    [Fact]
    public void Playlists_ShouldContain4Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Playlists;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(4, permissions.Length);
    }

    #endregion

    #region Ads Permissions Tests

    [Fact]
    public void Ads_ShouldContain2Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Ads;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(2, permissions.Length);
    }

    #endregion

    #region Rates Permissions Tests

    [Fact]
    public void Rates_ShouldContain2Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Rates;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(2, permissions.Length);
    }

    #endregion

    #region Shares Permissions Tests

    [Fact]
    public void Shares_ShouldContain3Permissions()
    {
        // Act
        PermissionEntity[] permissions = VisitorPermissions.Shares;

        // Assert
        permissions.Should().NotBeNull();
        Assert.Equal(3, permissions.Length);
    }

    #endregion

    #region GetAllPermissions Tests

    [Fact]
    public void GetAllPermissions_ShouldReturn29Permissions()
    {
        // Act
        PermissionEntity[] allPermissions = VisitorPermissions.GetAllPermissions();

        // Assert
        allPermissions.Should().NotBeNull();
        Assert.Equal(29, allPermissions.Length);
    }

    [Fact]
    public void GetAllPermissions_ShouldContainAllCategories()
    {
        // Act
        PermissionEntity[] allPermissions = VisitorPermissions.GetAllPermissions();

        // Assert - check that permissions from each category are present
        allPermissions.Should().Contain(p => p.Resource == "articles");
        allPermissions.Should().Contain(p => p.Resource == "own_profile");
        allPermissions.Should().Contain(p => p.Resource == "likes");
        allPermissions.Should().Contain(p => p.Resource == "comments");
        allPermissions.Should().Contain(p => p.Resource == "bookmarks");
        allPermissions.Should().Contain(p => p.Resource == "tags");
        allPermissions.Should().Contain(p => p.Resource == "playlists");
        allPermissions.Should().Contain(p => p.Resource == "ads_banners");
        allPermissions.Should().Contain(p => p.Resource == "rates");
        allPermissions.Should().Contain(p => p.Resource == "shares");
    }

    [Fact]
    public void GetAllPermissions_ShouldNotContainDuplicates()
    {
        // Act
        PermissionEntity[] allPermissions = VisitorPermissions.GetAllPermissions();

        // Assert - check unique count matches total count
        int uniqueCount = allPermissions.Distinct().Count();
        Assert.Equal(allPermissions.Length, uniqueCount);
    }

    [Fact]
    public void GetAllPermissions_AllPermissionsShouldHaveValidResourceAndAction()
    {
        // Act
        PermissionEntity[] allPermissions = VisitorPermissions.GetAllPermissions();

        // Assert
        Assert.All(
            allPermissions,
            permission =>
            {
                permission.Resource.Should().NotBeNull();
                Assert.NotEmpty(permission.Resource);
                permission.Action.Should().NotBeNull();
                Assert.NotEmpty(permission.Action);
            }
        );
    }

    [Fact]
    public void GetAllPermissions_AllPermissionsShouldHaveDescriptions()
    {
        // Act
        PermissionEntity[] allPermissions = VisitorPermissions.GetAllPermissions();

        // Assert
        Assert.All(
            allPermissions,
            permission =>
            {
                permission.Description.Should().NotBeNull();
                Assert.NotEmpty(permission.Description);
            }
        );
    }

    #endregion
}
