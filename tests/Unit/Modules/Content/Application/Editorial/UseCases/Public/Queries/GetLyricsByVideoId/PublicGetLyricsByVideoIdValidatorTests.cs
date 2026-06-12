using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsByVideoIdValidator"/>.
/// </summary>
public class PublicGetLyricsByVideoIdValidatorTests
{
    private readonly VideoErrorMessage _i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();
    private readonly PublicGetLyricsByVideoIdValidator _validator;

    public PublicGetLyricsByVideoIdValidatorTests()
    {
        _validator = new PublicGetLyricsByVideoIdValidator(_i18n);
    }

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region VideoId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyVideoId_ShouldHaveError()
    {
        // Arrange
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicGetLyricsByVideoIdQuery.VideoId)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicGetLyricsByVideoIdQuery.VideoId)
                && e.ErrorMessage == _i18n.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>(culture);
        var validator = new PublicGetLyricsByVideoIdValidator(i18n);
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicGetLyricsByVideoIdQuery.VideoId)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
