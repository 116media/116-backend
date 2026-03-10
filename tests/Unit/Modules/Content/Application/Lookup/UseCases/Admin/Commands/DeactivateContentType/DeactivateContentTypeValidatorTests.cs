using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Unit tests for <see cref="DeactivateContentTypeValidator"/>.
/// </summary>
public class DeactivateContentTypeValidatorTests
{
    private readonly DeactivateContentTypeValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new DeactivateContentTypeCommand(Id: Guid.NewGuid());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldHaveError()
    {
        // Arrange
        var command = new DeactivateContentTypeCommand(Id: Guid.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(DeactivateContentTypeCommand.Id)
                && e.ErrorMessage == "Content type ID is required."
            );
    }

    #endregion
}
