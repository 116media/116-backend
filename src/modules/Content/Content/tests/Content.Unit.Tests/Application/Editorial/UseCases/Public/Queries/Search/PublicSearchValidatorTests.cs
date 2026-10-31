using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.UseCases.Public.Queries.Search;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Unit tests for <see cref="PublicSearchValidator" />.
/// </summary>
public class PublicSearchValidatorTests
{
    private readonly PublicSearchValidator _validator = new();

    [Theory]
    [InlineData("", false)]
    [InlineData("a", false)]
    [InlineData("ab", true)]
    [InlineData("fally ipupa", true)]
    public void Validate_ForQueryLength_ShouldMatchExpectation(string term, bool expected)
    {
        // Arrange
        var query = new PublicSearchQuery(new PaginatedRequest(0, 20), term);

        // Act
        TestValidationResult<PublicSearchQuery> result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(120, true)]
    [InlineData(121, false)]
    public void Validate_AtTheLengthCeiling_ShouldMatchExpectation(int length, bool expected)
    {
        // Arrange
        var query = new PublicSearchQuery(new PaginatedRequest(0, 20), new string('a', length));

        // Act
        TestValidationResult<PublicSearchQuery> result = _validator.TestValidate(query);

        // Assert
        result.IsValid.Should().Be(expected);
    }

    [Fact]
    public void Validate_WithScopeOutsideTheEnum_ShouldBeInvalid()
    {
        // Arrange
        var query = new PublicSearchQuery(new PaginatedRequest(0, 20), "rumba", (EnumSearchScope)42);

        // Act
        TestValidationResult<PublicSearchQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(search => search.Scope);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(20, 20)]
    [InlineData(500, 100)]
    public void Construct_ForPageSize_ShouldClampBeforeValidation(int requested, int expected)
    {
        // Arrange
        var query = new PublicSearchQuery(new PaginatedRequest(0, requested), "rumba");

        // Act
        TestValidationResult<PublicSearchQuery> result = _validator.TestValidate(query);

        // Assert
        query.PaginatedRequest.PageSize.Should().Be(expected);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Construct_WithNegativePageIndex_ShouldClampToZero()
    {
        // Arrange
        var query = new PublicSearchQuery(new PaginatedRequest(-1, 20), "rumba");

        // Act
        TestValidationResult<PublicSearchQuery> result = _validator.TestValidate(query);

        // Assert
        query.PaginatedRequest.PageIndex.Should().Be(0);
        result.IsValid.Should().BeTrue();
    }
}
