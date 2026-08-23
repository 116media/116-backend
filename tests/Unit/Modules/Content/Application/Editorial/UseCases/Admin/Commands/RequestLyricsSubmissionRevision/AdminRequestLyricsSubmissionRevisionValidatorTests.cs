using _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;

/// <summary>
/// Unit tests for <see cref="AdminRequestLyricsSubmissionRevisionValidator"/>.
/// </summary>
public class AdminRequestLyricsSubmissionRevisionValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminRequestLyricsSubmissionRevisionValidator _validator;

    public AdminRequestLyricsSubmissionRevisionValidatorTests()
    {
        _validator = new AdminRequestLyricsSubmissionRevisionValidator(_i18n);
    }

    private static AdminRequestLyricsSubmissionRevisionCommand BuildValidCommand(string? note = null) =>
        new(Id: Guid.NewGuid(), Note: note ?? TestConstants.Lyrics.ValidRejectionReason, ReviewerId: Guid.NewGuid());

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand();

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Note Validation Tests

    [Fact]
    public async Task Validate_WithEmptyNote_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(note: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRequestLyricsSubmissionRevisionCommand.Note)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNoteExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(note: new string('a', TestConstants.Lyrics.RejectionReasonMaxLength + 1));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRequestLyricsSubmissionRevisionCommand.Note)
                && e.ErrorMessage
                    == _i18n.Lyrics.Msg.RejectionReasonTooLong(TestConstants.Lyrics.RejectionReasonMaxLength)
            );
    }

    #endregion
}
