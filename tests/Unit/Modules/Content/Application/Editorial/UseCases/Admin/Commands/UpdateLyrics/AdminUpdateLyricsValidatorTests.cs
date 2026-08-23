using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsValidator"/>.
/// </summary>
public class AdminUpdateLyricsValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUpdateLyricsValidator _validator;

    public AdminUpdateLyricsValidatorTests()
    {
        _validator = new AdminUpdateLyricsValidator(_i18n);
    }

    private static AdminUpdateLyricsCommand ValidCommand() =>
        new(
            Id: Guid.NewGuid().ToString(),
            CategoryId: Guid.NewGuid(),
            SongTitle: TestConstants.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Lyrics.ValidArtistName,
            Slug: TestConstants.Lyrics.ValidSlug,
            LyricsText: TestConstants.Lyrics.ValidLyricsText,
            Language: TestConstants.Lyrics.ValidLanguage,
            VideoId: null,
            CustomerId: null,
            OrderItemId: null
        );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Act
        ValidationResult result = await _validator.ValidateAsync(ValidCommand());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Id = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Id)
                && e.ErrorMessage == _i18n.Lyrics.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Id = "not-a-guid",
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Id)
                && e.ErrorMessage == _i18n.Lyrics.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region CategoryId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            CategoryId = Guid.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.CategoryId)
                && e.ErrorMessage == _i18n.Lyrics.Msg.CategoryIdRequired()
            );
    }

    #endregion

    #region SongTitle Validation Tests

    [Fact]
    public async Task Validate_WithEmptySongTitle_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            SongTitle = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.SongTitle)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SongTitleRequired()
            );
    }

    #endregion

    #region ArtistName Validation Tests

    [Fact]
    public async Task Validate_WithEmptyArtistName_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            ArtistName = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.ArtistName)
                && e.ErrorMessage == _i18n.Lyrics.Msg.ArtistNameRequired()
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Slug = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Slug = "Invalid-Slug",
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugInvalidFormat()
            );
    }

    #endregion

    #region LyricsText Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLyricsText_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            LyricsText = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.LyricsText)
                && e.ErrorMessage == _i18n.Lyrics.Msg.LyricsTextRequired()
            );
    }

    #endregion

    #region Language Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLanguage_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Language = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Language)
                && e.ErrorMessage == _i18n.Lyrics.Msg.LanguageRequired()
            );
    }

    #endregion

    #region Conditional OrderItemId/CustomerId Validation Tests

    [Fact]
    public async Task Validate_WithCustomerIdButNoOrderItemId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            CustomerId = Guid.NewGuid(),
            OrderItemId = null,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.OrderItemId)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.OrderItemIdRequired()
            );
    }

    [Fact]
    public async Task Validate_WithOrderItemIdButNoCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            CustomerId = null,
            OrderItemId = Guid.NewGuid(),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.CustomerId)
                && e.ErrorMessage == _i18n.Customer.Msg.CustomerIdRequired()
            );
    }

    #endregion
}
