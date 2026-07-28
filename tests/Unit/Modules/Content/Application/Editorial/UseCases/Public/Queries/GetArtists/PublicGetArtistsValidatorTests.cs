using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Unit tests for <see cref="PublicGetArtistsValidator"/>.
/// </summary>
public class PublicGetArtistsValidatorTests
{
    private readonly PublicGetArtistsValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static PublicGetArtistsQuery Query(string? letter = null, string? search = null) =>
        new(new PaginatedRequest(0, 30), letter, search);

    [Fact]
    public void Validate_WithNoFilters_ShouldPass()
    {
        _validator.TestValidate(Query()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Z")]
    [InlineData("#")]
    public void Validate_WithValidLetter_ShouldPass(string letter)
    {
        _validator.TestValidate(Query(letter: letter)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSearch_ShouldPass()
    {
        _validator.TestValidate(Query(search: "fally")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithLetterAndSearchTogether_ShouldFail()
    {
        // The two filters are mutually exclusive — a silent precedence would let a broken
        // client render plausible-but-wrong results forever.
        _validator.TestValidate(Query(letter: "F", search: "fally")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithSingleCharacterSearch_ShouldFail()
    {
        _validator.TestValidate(Query(search: "f")).ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("AB")]
    [InlineData("1")]
    [InlineData("é")]
    public void Validate_WithInvalidLetter_ShouldFail(string letter)
    {
        _validator.TestValidate(Query(letter: letter)).ShouldHaveValidationErrorFor(x => x.Letter);
    }
}
