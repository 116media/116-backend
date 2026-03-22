using _116.Content.Application.Shared.Validators;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Validators;

/// <summary>
/// Unit tests for <see cref="ValidationUtils"/>.
/// </summary>
public class ValidationUtilsTests
{
    #region GetPropertyValue

    private class SampleCommand
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void GetPropertyValue_WithExistingStringProperty_ShouldReturnValue()
    {
        var command = new SampleCommand { Name = "TestName" };
        string? result = ValidationUtils.GetPropertyValue(command, "Name");
        result.Should().Be("TestName");
    }

    [Fact]
    public void GetPropertyValue_WithNullStringProperty_ShouldReturnNull()
    {
        var command = new SampleCommand { Name = null };
        string? result = ValidationUtils.GetPropertyValue(command, "Name");
        result.Should().BeNull();
    }

    [Fact]
    public void GetPropertyValue_WithNonExistentProperty_ShouldReturnNull()
    {
        // Covers the null path of property?.GetValue() when the property is not found
        var command = new SampleCommand { Name = "TestName" };
        string? result = ValidationUtils.GetPropertyValue(command, "NonExistent");
        result.Should().BeNull();
    }

    [Fact]
    public void GetPropertyValue_WithNonStringProperty_ShouldReturnNull()
    {
        var command = new SampleCommand { Age = 30 };
        string? result = ValidationUtils.GetPropertyValue(command, "Age");
        result.Should().BeNull();
    }

    [Fact]
    public void GetPropertyValue_WithMultipleProperties_ShouldReturnCorrectValue()
    {
        var command = new SampleCommand { Name = "Alice", Email = "alice@example.com" };

        ValidationUtils.GetPropertyValue(command, "Name").Should().Be("Alice");
        ValidationUtils.GetPropertyValue(command, "Email").Should().Be("alice@example.com");
    }

    #endregion
}
