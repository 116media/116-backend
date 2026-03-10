using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Unit tests for <see cref="ActivateContentTypeValidator"/>.
/// </summary>
public class ActivateContentTypeValidatorTests
{
    private readonly ActivateContentTypeValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new ActivateContentTypeCommand(Id: Guid.NewGuid());

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
        var command = new ActivateContentTypeCommand(Id: Guid.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(ActivateContentTypeCommand.Id)
                && e.ErrorMessage == "Content type ID is required."
            );
    }

    #endregion
}
