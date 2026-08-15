using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="UnsupportedProviderExceptionHandler"/>. A provider with no registered
/// verifier maps to 400 with a localized detail naming the provider.
/// </summary>
public class UnsupportedProviderExceptionHandlerTests
{
    private readonly UnsupportedProviderExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnUnsupportedProviderExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(UnsupportedProviderException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn400WithLocalizedDetailNamingProvider()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        var i18n = context.RequestServices.GetRequiredService<ValidationErrorMessage>();
        var exception = new UnsupportedProviderException(EnumAuthProvider.Local);

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Be(i18n.UnsupportedProvider(EnumAuthProvider.Local.ToString()));
    }
}
