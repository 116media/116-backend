using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularVideosValidator"/>.
/// </summary>
public class PublicGetPopularVideosValidatorTests
{
    private readonly PublicGetPopularVideosValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    #region Validate

    [Theory]
    [InlineData(PopularVideosLimits.MinLimit)]
    [InlineData(PopularVideosLimits.DefaultLimit)]
    [InlineData(PopularVideosLimits.MaxLimit)]
    public void Validate_WithLimitWithinRange_ShouldNotHaveErrors(int limit)
    {
        // Arrange
        var query = new PublicGetPopularVideosQuery(Limit: limit, CategoryId: null, ExcludeId: null);

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(PopularVideosLimits.MinLimit - 1)]
    [InlineData(PopularVideosLimits.MaxLimit + 1)]
    public void Validate_WithLimitOutOfRange_ShouldHaveError(int limit)
    {
        // Arrange
        var query = new PublicGetPopularVideosQuery(Limit: limit, CategoryId: null, ExcludeId: null);

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Limit");
    }

    #endregion
}
