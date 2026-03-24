using System.Security.Claims;
using _116.Shared.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="HttpCurrentActor"/>.
/// </summary>
public class HttpCurrentActorTests
{
    private static HttpCurrentActor CreateActor(HttpContext? httpContext)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);
        return new HttpCurrentActor(accessor.Object);
    }

    private static ClaimsPrincipal AuthenticatedUser(string userId) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], authenticationType: "Bearer"));

    private static ClaimsPrincipal UnauthenticatedUser() => new(new ClaimsIdentity());

    #region HasHttpContext

    [Fact]
    public void HasHttpContext_WhenHttpContextIsNull_ShouldBeFalse()
    {
        var actor = CreateActor(null);
        actor.HasHttpContext.Should().BeFalse();
    }

    [Fact]
    public void HasHttpContext_WhenHttpContextPresent_ShouldBeTrue()
    {
        var actor = CreateActor(new DefaultHttpContext());
        actor.HasHttpContext.Should().BeTrue();
    }

    #endregion

    #region UserId

    [Fact]
    public void UserId_WhenHttpContextIsNull_ShouldBeNull()
    {
        var actor = CreateActor(null);
        actor.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenAuthenticatedWithClaim_ShouldReturnUserId()
    {
        var ctx = new DefaultHttpContext { User = AuthenticatedUser("user-abc") };
        var actor = CreateActor(ctx);
        actor.UserId.Should().Be("user-abc");
    }

    [Fact]
    public void UserId_WhenUnauthenticatedRequest_ShouldBeNull()
    {
        var ctx = new DefaultHttpContext { User = UnauthenticatedUser() };
        var actor = CreateActor(ctx);
        actor.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenAuthenticatedButNoNameIdentifierClaim_ShouldBeNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "x@x.com")], "Bearer"));
        var ctx = new DefaultHttpContext { User = principal };
        var actor = CreateActor(ctx);
        actor.UserId.Should().BeNull();
    }

    #endregion

    #region IsAuthenticated

    [Fact]
    public void IsAuthenticated_WhenHttpContextIsNull_ShouldBeFalse()
    {
        var actor = CreateActor(null);
        actor.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_WhenAuthenticatedUser_ShouldBeTrue()
    {
        var ctx = new DefaultHttpContext { User = AuthenticatedUser("user-abc") };
        var actor = CreateActor(ctx);
        actor.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenUnauthenticatedRequest_ShouldBeFalse()
    {
        var ctx = new DefaultHttpContext { User = UnauthenticatedUser() };
        var actor = CreateActor(ctx);
        actor.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_WhenIdentityIsNull_ShouldBeFalse()
    {
        // ClaimsPrincipal with no identities → Identity returns null
        var ctx = new DefaultHttpContext { User = new ClaimsPrincipal() };
        var actor = CreateActor(ctx);
        actor.IsAuthenticated.Should().BeFalse();
    }

    #endregion
}
