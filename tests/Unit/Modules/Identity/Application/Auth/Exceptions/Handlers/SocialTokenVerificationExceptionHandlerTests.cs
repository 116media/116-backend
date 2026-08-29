using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="SocialTokenVerificationExceptionHandler"/>. A provider token that failed
/// verification maps to 401 with the localized, non-revealing detail.
/// </summary>
public class SocialTokenVerificationExceptionHandlerTests
{
    private readonly SocialTokenVerificationExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnSocialTokenVerificationExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(SocialTokenVerificationException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn401WithLocalizedDetail()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        var i18n = context.RequestServices.GetRequiredService<AuthenticationErrorMessage>();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(new SocialTokenVerificationException(), context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Detail.Should().Be(i18n.InvalidProviderToken());
    }
}
