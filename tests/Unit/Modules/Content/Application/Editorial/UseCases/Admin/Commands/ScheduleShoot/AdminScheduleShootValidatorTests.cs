using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Unit tests for <see cref="AdminScheduleShootValidator"/>.
/// </summary>
public class AdminScheduleShootValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminScheduleShootValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminScheduleShootValidatorTests"/>.
    /// </summary>
    public AdminScheduleShootValidatorTests()
    {
        _validator = new(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminScheduleShootCommand(
            VideoId: Guid.NewGuid().ToString(),
            ShootingScheduledAt: DateTimeOffset.UtcNow.AddDays(7)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

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
        var command = new AdminScheduleShootCommand(
            VideoId: string.Empty,
            ShootingScheduledAt: DateTimeOffset.UtcNow.AddDays(7)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminScheduleShootCommand.VideoId)
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["VideoIdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminScheduleShootCommand(
            VideoId: "not-a-guid",
            ShootingScheduledAt: DateTimeOffset.UtcNow.AddDays(7)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminScheduleShootCommand.VideoId)
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region ShootingScheduledAt Validation Tests

    [Fact]
    public async Task Validate_WithPastShootingDate_ShouldHaveError()
    {
        // Arrange
        var command = new AdminScheduleShootCommand(
            VideoId: Guid.NewGuid().ToString(),
            ShootingScheduledAt: DateTimeOffset.UtcNow.AddDays(-1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminScheduleShootCommand.ShootingScheduledAt)
                && e.ErrorMessage == _i18n.Video.Msg.ShootingScheduledDateMustBeInFuture()
            );
    }

    #endregion
}
