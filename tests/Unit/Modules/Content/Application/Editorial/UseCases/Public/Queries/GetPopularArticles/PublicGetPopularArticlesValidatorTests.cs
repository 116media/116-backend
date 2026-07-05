using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularArticlesValidator"/>.
/// </summary>
public class PublicGetPopularArticlesValidatorTests
{
    private readonly PublicGetPopularArticlesValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    #region Validate

    [Theory]
    [InlineData(PopularArticlesLimits.MinLimit)]
    [InlineData(PopularArticlesLimits.DefaultLimit)]
    [InlineData(PopularArticlesLimits.MaxLimit)]
    public void Validate_WithLimitWithinRange_ShouldNotHaveErrors(int limit)
    {
        // Arrange
        var query = new PublicGetPopularArticlesQuery(Limit: limit, CategoryId: null, ExcludeId: null);

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(PopularArticlesLimits.MinLimit - 1)]
    [InlineData(PopularArticlesLimits.MaxLimit + 1)]
    public void Validate_WithLimitOutOfRange_ShouldHaveError(int limit)
    {
        // Arrange
        var query = new PublicGetPopularArticlesQuery(Limit: limit, CategoryId: null, ExcludeId: null);

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Limit");
    }

    #endregion
}
