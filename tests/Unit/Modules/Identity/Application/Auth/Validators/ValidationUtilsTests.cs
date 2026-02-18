using _116.Identity.Application.Auth.Validators;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="ValidationUtils"/>.
/// </summary>
public class ValidationUtilsTests
{
    #region ValidUrl

    [Fact]
    public void ValidUrl_WithValidHttpsUrl_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl("https://example.com");
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidUrl_WithValidHttpUrl_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl("http://example.com");
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidUrl_WithNullUrl_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl(null);
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidUrl_WithEmptyUrl_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl(string.Empty);
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidUrl_WithWhitespaceUrl_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl("   ");
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidUrl_WithRelativeUrl_ShouldReturnFalse()
    {
        bool result = ValidationUtils.ValidUrl("/relative/path");
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidUrl_WithPlainString_ShouldReturnFalse()
    {
        bool result = ValidationUtils.ValidUrl("not-a-url");
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidUrl_WithFtpScheme_ShouldReturnFalse()
    {
        bool result = ValidationUtils.ValidUrl("ftp://example.com");
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidUrl_WithUrlWithPath_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl("https://example.com/path/to/resource");
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidUrl_WithUrlWithQueryString_ShouldReturnTrue()
    {
        bool result = ValidationUtils.ValidUrl("https://example.com?foo=bar&baz=qux");
        result.Should().BeTrue();
    }

    #endregion

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
