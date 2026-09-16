using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="Slug"/> value object.
/// </summary>
public class SlugTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("gospel")]
    [InlineData("fally-ipupa")]
    [InlineData("116-le-focus")]
    [InlineData("a1")]
    [InlineData("fally-ipupa-eloko-oyo-lyrics")]
    public void Constructor_WithWellFormedValue_ShouldKeepItVerbatim(string value)
    {
        // Act
        Slug slug = new(value);

        // Assert
        slug.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Fally-Ipupa")]
    [InlineData("fally ipupa")]
    [InlineData("-fally")]
    [InlineData("fally-")]
    [InlineData("fally--ipupa")]
    [InlineData("fally_ipupa")]
    [InlineData("fally.ipupa")]
    [InlineData("élodie")]
    public void Constructor_WithMalformedValue_ShouldThrowInvalidSlug(string? value)
    {
        // Act
        Action act = () => new Slug(value!);

        // Assert
        act.Should().ThrowExactly<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.InvalidSlug);
    }

    #endregion

    #region TryFrom and IsWellFormed Tests

    [Fact]
    public void TryFrom_WithWellFormedValue_ShouldReturnTheSlug()
    {
        Slug? slug = Slug.TryFrom("fally-ipupa");

        slug.Should().NotBeNull();
        slug!.Value.Should().Be("fally-ipupa");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Fally Ipupa")]
    public void TryFrom_WithMalformedValue_ShouldReturnNullInsteadOfThrowing(string? value)
    {
        Slug.TryFrom(value).Should().BeNull();
    }

    [Theory]
    [InlineData("fally-ipupa", true)]
    [InlineData("Fally-Ipupa", false)]
    [InlineData(null, false)]
    public void IsWellFormed_ShouldReportWhetherTheValueIsASlug(string? value, bool expected)
    {
        Slug.IsWellFormed(value).Should().Be(expected);
    }

    #endregion

    #region Conversion and Equality Tests

    [Fact]
    public void ImplicitConversion_ToString_ShouldYieldTheUnderlyingValue()
    {
        Slug slug = new("fally-ipupa");

        string value = slug;

        value.Should().Be("fally-ipupa");
    }

    [Fact]
    public void ImplicitConversion_FromMalformedString_ShouldThrowInvalidSlug()
    {
        Action act = () =>
        {
            Slug _ = "Not A Slug";
        };

        act.Should().ThrowExactly<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.InvalidSlug);
    }

    [Fact]
    public void Equality_ShouldCompareByValue()
    {
        new Slug("fally-ipupa").Should().Be(new Slug("fally-ipupa"));
        new Slug("fally-ipupa").Should().NotBe(new Slug("ferre-gola"));
    }

    [Fact]
    public void ToString_ShouldReturnTheUnderlyingValue()
    {
        new Slug("fally-ipupa").ToString().Should().Be("fally-ipupa");
    }

    #endregion
}
