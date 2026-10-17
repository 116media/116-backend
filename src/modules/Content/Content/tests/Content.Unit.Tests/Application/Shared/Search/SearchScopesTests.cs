using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Search;

/// <summary>
/// Unit tests for <see cref="SearchScopes" />, the only place the API's scopes and the
/// stored content types meet.
/// </summary>
public class SearchScopesTests
{
    [Theory]
    [InlineData(EnumSearchScope.Articles, EnumCoreContentType.Article)]
    [InlineData(EnumSearchScope.Videos, EnumCoreContentType.Video)]
    [InlineData(EnumSearchScope.Shorts, EnumCoreContentType.Short)]
    public void ToContentType_ForANarrowedScope_ShouldReturnItsType(EnumSearchScope scope, EnumCoreContentType expected)
    {
        SearchScopes.ToContentType(scope).Should().Be(expected);
    }

    [Fact]
    public void ToContentType_ForAll_ShouldReturnNullBecauseItNarrowsToNoType()
    {
        SearchScopes.ToContentType(EnumSearchScope.All).Should().BeNull();
    }

    [Theory]
    [InlineData(EnumCoreContentType.Article, EnumSearchScope.Articles)]
    [InlineData(EnumCoreContentType.Video, EnumSearchScope.Videos)]
    [InlineData(EnumCoreContentType.Short, EnumSearchScope.Shorts)]
    public void ToScope_ForASearchableType_ShouldReturnItsScope(EnumCoreContentType type, EnumSearchScope expected)
    {
        SearchScopes.ToScope(type).Should().Be(expected);
    }

    [Theory]
    [InlineData(EnumCoreContentType.Lyrics)]
    [InlineData(EnumCoreContentType.Custom)]
    public void ToScope_ForATypeNoScopeExposes_ShouldReturnNull(EnumCoreContentType type)
    {
        SearchScopes.ToScope(type).Should().BeNull();
    }

    [Fact]
    public void ToContentType_ForEveryScopeButAll_ShouldRoundTripBackToThatScope()
    {
        EnumSearchScope[] narrowing =
        [
            .. Enum.GetValues<EnumSearchScope>().Where(scope => scope != EnumSearchScope.All),
        ];

        foreach (EnumSearchScope scope in narrowing)
        {
            EnumCoreContentType? type = SearchScopes.ToContentType(scope);

            type.Should().NotBeNull($"{scope} must map to a content type");
            SearchScopes.ToScope(type!.Value).Should().Be(scope);
        }
    }
}
