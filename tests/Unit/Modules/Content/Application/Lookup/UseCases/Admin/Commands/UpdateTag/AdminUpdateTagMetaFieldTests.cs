using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Unit tests for <see cref="AdminUpdateTagMetaField"/>.
/// </summary>
public class AdminUpdateTagMetaFieldTests
{
    [Fact]
    public void AdminUpdateTag_ShouldHaveCorrectName()
    {
        // Arrange & Act
        string name = AdminUpdateTagMetaField.AdminUpdateTag.Name;

        // Assert
        name.Should().Be("AdminUpdateTag");
    }

    [Fact]
    public void AdminUpdateTag_ShouldHaveCorrectSummary()
    {
        // Arrange & Act
        string summary = AdminUpdateTagMetaField.AdminUpdateTag.Summary;

        // Assert
        summary.Should().Be("Update a content tag");
    }

    [Fact]
    public void AdminUpdateTag_ShouldHaveNonEmptyDescription()
    {
        // Arrange & Act
        string description = AdminUpdateTagMetaField.AdminUpdateTag.Description;

        // Assert
        description.Should().NotBeNullOrWhiteSpace();
    }
}
