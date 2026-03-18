using _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Unit tests for <see cref="PublicRenamePlaylistValidator"/>.
/// </summary>
public class PublicRenamePlaylistValidatorTests
{
    private readonly PublicRenamePlaylistValidator _validator = new();

    #region Valid Cases

    [Fact]
    public async Task Validate_WhenNameIsValid_ShouldPass()
    {
        // Arrange
        var command = new PublicRenamePlaylistCommand(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: TestConstants.Content.Interactions.ValidPlaylistName
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Validate_WhenNameIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new PublicRenamePlaylistCommand(Id: Guid.NewGuid(), UserId: Guid.NewGuid(), Name: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRenamePlaylistCommand.Name));
    }

    [Fact]
    public async Task Validate_WhenNameIsWhiteSpace_ShouldFail()
    {
        // Arrange
        var command = new PublicRenamePlaylistCommand(Id: Guid.NewGuid(), UserId: Guid.NewGuid(), Name: "   ");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenNameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var command = new PublicRenamePlaylistCommand(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: new string('a', ContentConstants.MaxPlaylistNameLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion
}
