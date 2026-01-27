using _116.BuildingBlocks.Utils;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace _116.Unit.Tests.BuildingBlocks.Utils;

/// <summary>
/// Unit tests for <see cref="ApiVersionUrl"/>.
/// </summary>
public class ApiVersionUrlTests
{
    [Fact]
    public void Build_WithHttpContextContainingVersion_ShouldBuildCorrectUrl()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "1";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "public/users/123");

        // Assert
        result.Should().Be("/api/v1/public/users/123");
    }

    [Fact]
    public void Build_WithHttpContextContainingVersion2_ShouldBuildCorrectUrl()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "2";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "admin/sessions");

        // Assert
        result.Should().Be("/api/v2/admin/sessions");
    }

    [Fact]
    public void Build_WithPathHavingLeadingSlash_ShouldTrimSlash()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "1";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "/public/users/123");

        // Assert
        result.Should().Be("/api/v1/public/users/123");
    }

    [Fact]
    public void Build_WithPathWithoutLeadingSlash_ShouldBuildCorrectUrl()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "1";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "admin/roles");

        // Assert
        result.Should().Be("/api/v1/admin/roles");
    }

    [Fact]
    public void Build_WithHttpContextWithoutVersion_ShouldDefaultToVersion1()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        // No version in route data
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "public/auth/login");

        // Assert
        result.Should().Be("/api/v1/public/auth/login");
    }

    [Fact]
    public void Build_WithNullRouteData_ShouldDefaultToVersion1()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        // No routing feature set - GetRouteData will return null

        // Act
        var result = ApiVersionUrl.Build(httpContext, "public/users");

        // Assert
        result.Should().Be("/api/v1/public/users");
    }

    [Fact]
    public void Build_WithInvalidVersionString_ShouldDefaultToVersion1()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "invalid";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "admin/permissions");

        // Assert
        result.Should().Be("/api/v1/admin/permissions");
    }

    [Fact]
    public void Build_WithNullVersionValue_ShouldDefaultToVersion1()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = null!;
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "public/sessions");

        // Assert
        result.Should().Be("/api/v1/public/sessions");
    }

    [Fact]
    public void Build_WithVersion3_ShouldBuildCorrectUrl()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "3";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "admin/metrics");

        // Assert
        result.Should().Be("/api/v3/admin/metrics");
    }

    [Fact]
    public void Build_WithEmptyPath_ShouldBuildUrlWithEmptyPath()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "1";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "");

        // Assert
        result.Should().Be("/api/v1/");
    }

    [Fact]
    public void Build_WithComplexPath_ShouldBuildCorrectUrl()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "2";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "admin/users/123/roles/456");

        // Assert
        result.Should().Be("/api/v2/admin/users/123/roles/456");
    }

    [Fact]
    public void Build_WithNumericVersionObject_ShouldParseCorrectly()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = 2; // Integer instead of string
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "public/profile");

        // Assert
        result.Should().Be("/api/v2/public/profile");
    }

    [Fact]
    public void Build_WithZeroVersion_ShouldUseZero()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "0";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "test/endpoint");

        // Assert
        result.Should().Be("/api/v0/test/endpoint");
    }

    [Fact]
    public void Build_WithNegativeVersion_ShouldUseNegativeValue()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "-1";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "test/endpoint");

        // Assert
        result.Should().Be("/api/v-1/test/endpoint");
    }

    [Fact]
    public void Build_WithMultipleLeadingSlashes_ShouldTrimAll()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["version"] = "1";
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });

        // Act
        var result = ApiVersionUrl.Build(httpContext, "///public/users");

        // Assert
        result.Should().Be("/api/v1/public/users");
    }
}

internal class RoutingFeature : IRoutingFeature
{
    public RouteData? RouteData { get; set; }
}
