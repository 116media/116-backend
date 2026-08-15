using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="NotFoundExceptionHandler"/>.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the status and the entity-name detail branch are
/// asserted here.
/// </summary>
public class NotFoundExceptionHandlerTests
{
    private readonly NotFoundExceptionHandler _handler = new();
    private readonly SharedExceptionMessage i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturn404StatusCode()
    {
        // Arrange
        NotFoundException exception = new("Not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region Friendly Localized Message Tests

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldUseFriendlyLocalizedMessage()
    {
        // Arrange
        NotFoundException exception = new("UserEntity", (object)"abc-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_ShouldUseFriendlyLocalizedMessage()
    {
        // Arrange
        NotFoundException exception = new("UserEntity", "email", "test@test.com");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
    }

    [Fact]
    public void CreateProblemDetails_WithStringOnlyConstructor_ShouldUseExceptionMessage()
    {
        // Arrange
        string customMessage = "Custom not found message";
        NotFoundException exception = new(customMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(customMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldNotLeakEntityNameOrKeyValue()
    {
        // Arrange
        NotFoundException exception = new("SessionEntity", (object)"session-uuid");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("Session"));
        problemDetails.Detail.Should().NotContain("session-uuid");
        problemDetails.Detail.Should().NotContain("SessionEntity");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_ShouldNotLeakKeyNameOrValue()
    {
        // Arrange
        NotFoundException exception = new("PermissionEntity", "resource", "articles.read");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("Permission"));
        problemDetails.Detail.Should().NotContain("resource");
        problemDetails.Detail.Should().NotContain("articles.read");
    }

    [Fact]
    public void CreateProblemDetails_WithUnmappedEntity_ShouldFallBackToGenericLabel()
    {
        // Arrange
        NotFoundException exception = new("SomethingObscureEntity", (object)"xyz-789");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("SomethingObscure"));
        problemDetails.Detail.Should().NotContain("SomethingObscure");
        problemDetails.Detail.Should().NotContain("xyz-789");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_InFrench_ShouldReturnFrenchFriendlyMessage()
    {
        // Arrange
        string enDetail = i18n.EntityNotFound("User");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("UserEntity", (object)"abc-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
        problemDetails.Detail.Should().NotContain("abc-123");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_InFrench_ShouldReturnFrenchFriendlyMessage()
    {
        // Arrange
        string enDetail = i18n.EntityNotFound("User");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("UserEntity", "email", "test@test.com");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
        problemDetails.Detail.Should().NotContain("test@test.com");
    }

    #endregion

    #region Entity label mapping

    [Theory]
    [InlineData("UserEntity")]
    [InlineData("SessionEntity")]
    [InlineData("RoleEntity")]
    [InlineData("ArticleEntity")]
    [InlineData("VideoEntity")]
    [InlineData("LyricsEntity")]
    [InlineData("CategoryEntity")]
    public void CreateProblemDetails_ForEachMappedEntity_UsesItsFriendlyLabelWithoutLeakingTheName(string entityName)
    {
        // Arrange
        NotFoundException exception = new(entityName, (object)"key-value-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        string cleaned = entityName.Replace("Entity", "");
        problemDetails.Detail.Should().Be(i18n.EntityNotFound(cleaned));
        problemDetails.Detail.Should().NotContain(entityName);
        problemDetails.Detail.Should().NotContain("key-value-123");
    }

    [Fact]
    public void CreateProblemDetails_ForDifferentMappedEntities_ProducesDistinctFriendlyLabels()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        string userDetail = _handler
            .CreateProblemDetails(new NotFoundException("UserEntity", (object)"1"), context)
            .Detail!;
        string articleDetail = _handler
            .CreateProblemDetails(new NotFoundException("ArticleEntity", (object)"1"), context)
            .Detail!;

        // Assert
        userDetail.Should().NotBe(articleDetail);
    }

    [Fact]
    public void CreateProblemDetails_ForUnmappedEntity_InFrench_FallsBackToLocalizedGenericLabel()
    {
        // Arrange
        string enDetail = i18n.EntityNotFound("SomethingObscure");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("SomethingObscureEntity", (object)"xyz-789");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("SomethingObscure"));
        problemDetails.Detail.Should().NotContain("SomethingObscure");
        problemDetails.Detail.Should().NotContain("xyz-789");
    }

    #endregion
}
