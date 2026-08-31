using System.Reflection;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;
using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;
using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;
using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;
using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Enums;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;
using _116.Identity.Application.Shared.Cache;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Decorators;

/// <summary>
/// Contract tests over every <see cref="ICacheableRequest"/>: each keyed parameter must change
/// the cache key, and conditional queries must decline per-user or free-text arguments.
/// </summary>
public class CacheableQueryContractTests
{
    /// <summary>
    /// The expected number of cacheable queries; a new one fails this until it gets an entry.
    /// </summary>
    private const int ExpectedCacheableQueryCount = 33;

    private static readonly Assembly[] QueryAssemblies =
    [
        typeof(PublicGetPopularArticlesQuery).Assembly,
        typeof(AdminGetAllRolesQuery).Assembly,
    ];

    private static readonly PaginatedRequest Page = new(pageIndex: 0, pageSize: 10);
    private static readonly PaginatedRequest OtherPage = new(pageIndex: 1, pageSize: 10);
    private static readonly Guid SomeId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherId = new("22222222-2222-2222-2222-222222222222");

    /// <summary>
    /// One entry per cacheable query: a baseline instance, variants that must every one
    /// produce a distinct key, and instances that must decline the cache.
    /// </summary>
    private static readonly Dictionary<Type, QueryContract> Contracts = new()
    {
        [typeof(PublicGetActiveCategoriesQuery)] = new(
            () => new PublicGetActiveCategoriesQuery(null),
            [() => new PublicGetActiveCategoriesQuery(SomeId)],
            []
        ),
        [typeof(AdminGetAllCategoriesQuery)] = new(
            () => new AdminGetAllCategoriesQuery(Page),
            [
                () => new AdminGetAllCategoriesQuery(OtherPage),
                () => new AdminGetAllCategoriesQuery(Page, IsActive: true),
                () => new AdminGetAllCategoriesQuery(Page, IsFree: true),
            ],
            []
        ),
        [typeof(PublicGetExclusiveCategoryQuery)] = new(
            () => new PublicGetExclusiveCategoryQuery(Page),
            [() => new PublicGetExclusiveCategoryQuery(OtherPage)],
            []
        ),
        [typeof(PublicGetArticlePromotionFeedQuery)] = new(
            () => new PublicGetArticlePromotionFeedQuery(StripSize: 4),
            [() => new PublicGetArticlePromotionFeedQuery(StripSize: 6)],
            [() => new PublicGetArticlePromotionFeedQuery(StripSize: 4, CurrentUserId: SomeId)]
        ),
        [typeof(PublicGetArtistArticlesQuery)] = new(
            () => new PublicGetArtistArticlesQuery("slug-a", Page),
            [
                () => new PublicGetArtistArticlesQuery("slug-b", Page),
                () => new PublicGetArtistArticlesQuery("slug-a", OtherPage),
            ],
            []
        ),
        [typeof(PublicGetArtistBySlugQuery)] = new(
            () => new PublicGetArtistBySlugQuery("slug-a", Page, Page),
            [
                () => new PublicGetArtistBySlugQuery("slug-b", Page, Page),
                () => new PublicGetArtistBySlugQuery("slug-a", OtherPage, Page),
                () => new PublicGetArtistBySlugQuery("slug-a", Page, OtherPage),
            ],
            []
        ),
        [typeof(PublicGetArtistReleasesQuery)] = new(
            () => new PublicGetArtistReleasesQuery("slug-a", default, Page),
            [
                () => new PublicGetArtistReleasesQuery("slug-b", default, Page),
                () => new PublicGetArtistReleasesQuery("slug-a", default, OtherPage),
            ],
            []
        ),
        [typeof(PublicGetArtistsQuery)] = new(
            () => new PublicGetArtistsQuery(Page, Letter: null, Search: null),
            [
                () => new PublicGetArtistsQuery(OtherPage, Letter: null, Search: null),
                () => new PublicGetArtistsQuery(Page, Letter: "K", Search: null),
            ],
            [() => new PublicGetArtistsQuery(Page, Letter: null, Search: "fally")]
        ),
        [typeof(PublicGetLyricsTranslationsQuery)] = new(
            () => new PublicGetLyricsTranslationsQuery(SomeId),
            [() => new PublicGetLyricsTranslationsQuery(OtherId)],
            []
        ),
        [typeof(PublicGetPopularArticlesQuery)] = new(
            () => new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null),
            [
                () => new PublicGetPopularArticlesQuery(Limit: 7, CategoryId: null, ExcludeId: null),
                () => new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: SomeId, ExcludeId: null),
                () => new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: SomeId),
            ],
            []
        ),
        [typeof(PublicGetPopularVideosQuery)] = new(
            () => new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: null),
            [
                () => new PublicGetPopularVideosQuery(Limit: 7, CategoryId: null, ExcludeId: null),
                () => new PublicGetPopularVideosQuery(Limit: 5, CategoryId: SomeId, ExcludeId: null),
                () => new PublicGetPopularVideosQuery(Limit: 5, CategoryId: null, ExcludeId: SomeId),
            ],
            []
        ),
        [typeof(PublicGetPromotedArticlesQuery)] = new(
            () => new PublicGetPromotedArticlesQuery(),
            [],
            [() => new PublicGetPromotedArticlesQuery(CurrentUserId: SomeId)]
        ),
        [typeof(PublicGetPromotedVideosQuery)] = new(() => new PublicGetPromotedVideosQuery(), [], []),
        [typeof(PublicGetPublicShortsQuery)] = new(
            () => new PublicGetPublicShortsQuery(Page, Search: null),
            [() => new PublicGetPublicShortsQuery(OtherPage, Search: null)],
            [
                () => new PublicGetPublicShortsQuery(Page, Search: "drill"),
                () => new PublicGetPublicShortsQuery(Page, Search: null, CurrentUserId: SomeId),
            ]
        ),
        [typeof(PublicGetPublishedArticlesQuery)] = new(
            () => new PublicGetPublishedArticlesQuery(Page, Search: null, CategoryId: null, TagSlug: null),
            [
                () => new PublicGetPublishedArticlesQuery(OtherPage, Search: null, CategoryId: null, TagSlug: null),
                () => new PublicGetPublishedArticlesQuery(Page, Search: null, CategoryId: SomeId, TagSlug: null),
                () => new PublicGetPublishedArticlesQuery(Page, Search: null, CategoryId: null, TagSlug: "afro"),
            ],
            [
                () => new PublicGetPublishedArticlesQuery(Page, Search: "term", CategoryId: null, TagSlug: null),
                () =>
                    new PublicGetPublishedArticlesQuery(
                        Page,
                        Search: null,
                        CategoryId: null,
                        TagSlug: null,
                        CurrentUserId: SomeId
                    ),
            ]
        ),
        [typeof(PublicGetPublishedLyricsQuery)] = new(
            () => new PublicGetPublishedLyricsQuery(Page, Search: null, Language: null, CategoryId: null, Sort: null),
            [
                () =>
                    new PublicGetPublishedLyricsQuery(
                        OtherPage,
                        Search: null,
                        Language: null,
                        CategoryId: null,
                        Sort: null
                    ),
                () =>
                    new PublicGetPublishedLyricsQuery(Page, Search: null, Language: "fr", CategoryId: null, Sort: null),
                () =>
                    new PublicGetPublishedLyricsQuery(
                        Page,
                        Search: null,
                        Language: null,
                        CategoryId: SomeId,
                        Sort: null
                    ),
                () =>
                    new PublicGetPublishedLyricsQuery(
                        Page,
                        Search: null,
                        Language: null,
                        CategoryId: null,
                        Sort: "views"
                    ),
            ],
            [
                () =>
                    new PublicGetPublishedLyricsQuery(
                        Page,
                        Search: "term",
                        Language: null,
                        CategoryId: null,
                        Sort: null
                    ),
                () =>
                    new PublicGetPublishedLyricsQuery(
                        Page,
                        Search: null,
                        Language: null,
                        CategoryId: null,
                        Sort: null,
                        CurrentUserId: SomeId
                    ),
            ]
        ),
        [typeof(PublicGetPublishedVideosQuery)] = new(
            () => new PublicGetPublishedVideosQuery(Page, Search: null, CategoryId: null, TagSlug: null),
            [
                () => new PublicGetPublishedVideosQuery(OtherPage, Search: null, CategoryId: null, TagSlug: null),
                () => new PublicGetPublishedVideosQuery(Page, Search: null, CategoryId: SomeId, TagSlug: null),
                () => new PublicGetPublishedVideosQuery(Page, Search: null, CategoryId: null, TagSlug: "afro"),
            ],
            [() => new PublicGetPublishedVideosQuery(Page, Search: "term", CategoryId: null, TagSlug: null)]
        ),
        [typeof(PublicGetShortsFeedQuery)] = new(
            () => new PublicGetShortsFeedQuery(Cursor: null, PageSize: 10),
            [() => new PublicGetShortsFeedQuery(Cursor: null, PageSize: 20)],
            [
                () => new PublicGetShortsFeedQuery(Cursor: "abc", PageSize: 10),
                () => new PublicGetShortsFeedQuery(Cursor: null, PageSize: 10, CurrentUserId: SomeId),
            ]
        ),
        [typeof(PublicGetSimilarLyricsQuery)] = new(
            () => new PublicGetSimilarLyricsQuery(SomeId),
            [() => new PublicGetSimilarLyricsQuery(OtherId)],
            [() => new PublicGetSimilarLyricsQuery(SomeId, CurrentUserId: OtherId)]
        ),
        [typeof(PublicGetTranslationRevisionsQuery)] = new(
            () => new PublicGetTranslationRevisionsQuery(SomeId),
            [() => new PublicGetTranslationRevisionsQuery(OtherId)],
            []
        ),
        [typeof(PublicGetVideoFeedQuery)] = new(() => new PublicGetVideoFeedQuery(), [], []),
        [typeof(PublicGetVideoPromotionFeedQuery)] = new(
            () => new PublicGetVideoPromotionFeedQuery(StripSize: 4),
            [() => new PublicGetVideoPromotionFeedQuery(StripSize: 6)],
            []
        ),
        [typeof(AdminGetAllContentTypesQuery)] = new(
            () => new AdminGetAllContentTypesQuery(),
            [],
            [() => new AdminGetAllContentTypesQuery(Search: "art")]
        ),
        [typeof(AdminGetAllPricingTiersQuery)] = new(
            () => new AdminGetAllPricingTiersQuery(),
            [],
            [() => new AdminGetAllPricingTiersQuery(Search: "gold")]
        ),
        [typeof(AdminGetAllPromotionLevelsQuery)] = new(
            () => new AdminGetAllPromotionLevelsQuery(),
            [],
            [() => new AdminGetAllPromotionLevelsQuery(Search: "spot")]
        ),
        [typeof(PublicGetActivePromotionLevelsQuery)] = new(() => new PublicGetActivePromotionLevelsQuery(), [], []),
        [typeof(PublicGetAllContentTypesQuery)] = new(() => new PublicGetAllContentTypesQuery(), [], []),
        [typeof(PublicGetAllTagsQuery)] = new(
            () => new PublicGetAllTagsQuery(),
            [
                () => new PublicGetAllTagsQuery(Limit: 50),
                () => new PublicGetAllTagsQuery(ContentType: EnumCoreContentType.Article),
            ],
            [() => new PublicGetAllTagsQuery(Search: "afro")]
        ),
        [typeof(PublicGetPopularTagsQuery)] = new(
            () => new PublicGetPopularTagsQuery(),
            [
                () => new PublicGetPopularTagsQuery(Limit: 10),
                () => new PublicGetPopularTagsQuery(ContentType: EnumCoreContentType.Video),
            ],
            []
        ),
        [typeof(AdminGetAllPermissionsQuery)] = new(
            () => new AdminGetAllPermissionsQuery(Page),
            [
                () => new AdminGetAllPermissionsQuery(OtherPage),
                () => new AdminGetAllPermissionsQuery(Page, IsActive: true),
                () => new AdminGetAllPermissionsQuery(Page, IsDeleted: true),
            ],
            [() => new AdminGetAllPermissionsQuery(Page, Search: "user")]
        ),
        [typeof(AdminGetAllRolesQuery)] = new(
            () => new AdminGetAllRolesQuery(Page),
            [
                () => new AdminGetAllRolesQuery(OtherPage),
                () => new AdminGetAllRolesQuery(Page, IsActive: true),
                () => new AdminGetAllRolesQuery(Page, IsDeleted: true),
            ],
            [() => new AdminGetAllRolesQuery(Page, Search: "admin")]
        ),
        [typeof(AdminGetPermissionByIdQuery)] = new(
            () => new AdminGetPermissionByIdQuery(SomeId),
            [() => new AdminGetPermissionByIdQuery(OtherId)],
            []
        ),
        [typeof(AdminGetRoleByIdQuery)] = new(
            () => new AdminGetRoleByIdQuery(SomeId),
            [() => new AdminGetRoleByIdQuery(OtherId)],
            []
        ),
    };

    /// <summary>
    /// The declared lifetime of every cacheable query, so a TTL edit is a deliberate act.
    /// </summary>
    private static readonly Dictionary<Type, TimeSpan> ExpectedTtls = new()
    {
        [typeof(PublicGetActiveCategoriesQuery)] = TimeSpan.FromMinutes(30),
        [typeof(AdminGetAllCategoriesQuery)] = TimeSpan.FromMinutes(30),
        [typeof(PublicGetExclusiveCategoryQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetArticlePromotionFeedQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetArtistArticlesQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetArtistBySlugQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetArtistReleasesQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetArtistsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetLyricsTranslationsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPopularArticlesQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPopularVideosQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPromotedArticlesQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPromotedVideosQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPublicShortsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPublishedArticlesQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPublishedLyricsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPublishedVideosQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetShortsFeedQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetSimilarLyricsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetTranslationRevisionsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetVideoFeedQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetVideoPromotionFeedQuery)] = TimeSpan.FromMinutes(10),
        [typeof(AdminGetAllContentTypesQuery)] = TimeSpan.FromMinutes(30),
        [typeof(AdminGetAllPricingTiersQuery)] = TimeSpan.FromMinutes(30),
        [typeof(AdminGetAllPromotionLevelsQuery)] = TimeSpan.FromMinutes(30),
        [typeof(PublicGetActivePromotionLevelsQuery)] = TimeSpan.FromMinutes(30),
        [typeof(PublicGetAllContentTypesQuery)] = TimeSpan.FromMinutes(30),
        [typeof(PublicGetAllTagsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(PublicGetPopularTagsQuery)] = TimeSpan.FromMinutes(10),
        [typeof(AdminGetAllPermissionsQuery)] = TimeSpan.FromMinutes(30),
        [typeof(AdminGetAllRolesQuery)] = TimeSpan.FromMinutes(30),
        [typeof(AdminGetPermissionByIdQuery)] = TimeSpan.FromMinutes(30),
        [typeof(AdminGetRoleByIdQuery)] = TimeSpan.FromMinutes(30),
    };

    /// <summary>
    /// The declared eviction tags of every cacheable query, so a tag edit is a deliberate act.
    /// </summary>
    private static readonly Dictionary<Type, string[]> ExpectedTags = new()
    {
        [typeof(PublicGetActiveCategoriesQuery)] = [ContentCacheTags.Lookups],
        [typeof(AdminGetAllCategoriesQuery)] = [ContentCacheTags.Lookups],
        [typeof(PublicGetExclusiveCategoryQuery)] = [ContentCacheTags.Lookups, ContentCacheTags.Videos],
        [typeof(PublicGetArticlePromotionFeedQuery)] = [ContentCacheTags.Articles],
        [typeof(PublicGetArtistArticlesQuery)] = [ContentCacheTags.Artists, ContentCacheTags.Articles],
        [typeof(PublicGetArtistBySlugQuery)] =
        [
            ContentCacheTags.Artists,
            ContentCacheTags.Lyrics,
            ContentCacheTags.Videos,
        ],
        [typeof(PublicGetArtistReleasesQuery)] = [ContentCacheTags.Artists],
        [typeof(PublicGetArtistsQuery)] = [ContentCacheTags.Artists],
        [typeof(PublicGetLyricsTranslationsQuery)] = [ContentCacheTags.Lyrics],
        [typeof(PublicGetPopularArticlesQuery)] = [ContentCacheTags.PopularArticles],
        [typeof(PublicGetPopularVideosQuery)] = [ContentCacheTags.PopularVideos],
        [typeof(PublicGetPromotedArticlesQuery)] = [ContentCacheTags.Articles],
        [typeof(PublicGetPromotedVideosQuery)] = [ContentCacheTags.Videos],
        [typeof(PublicGetPublicShortsQuery)] = [ContentCacheTags.Shorts],
        [typeof(PublicGetPublishedArticlesQuery)] = [ContentCacheTags.Articles],
        [typeof(PublicGetPublishedLyricsQuery)] = [ContentCacheTags.Lyrics],
        [typeof(PublicGetPublishedVideosQuery)] = [ContentCacheTags.Videos],
        [typeof(PublicGetShortsFeedQuery)] = [ContentCacheTags.Shorts],
        [typeof(PublicGetSimilarLyricsQuery)] = [ContentCacheTags.Lyrics],
        [typeof(PublicGetTranslationRevisionsQuery)] = [ContentCacheTags.Lyrics],
        [typeof(PublicGetVideoFeedQuery)] = [ContentCacheTags.Videos, ContentCacheTags.Lookups],
        [typeof(PublicGetVideoPromotionFeedQuery)] = [ContentCacheTags.Videos],
        [typeof(AdminGetAllContentTypesQuery)] = [ContentCacheTags.Lookups],
        [typeof(AdminGetAllPricingTiersQuery)] = [ContentCacheTags.Lookups],
        [typeof(AdminGetAllPromotionLevelsQuery)] = [ContentCacheTags.Lookups],
        [typeof(PublicGetActivePromotionLevelsQuery)] = [ContentCacheTags.Lookups],
        [typeof(PublicGetAllContentTypesQuery)] = [ContentCacheTags.Lookups],
        [typeof(PublicGetAllTagsQuery)] = [ContentCacheTags.Tags],
        [typeof(PublicGetPopularTagsQuery)] = [ContentCacheTags.Tags],
        [typeof(AdminGetAllPermissionsQuery)] = [IdentityCacheTags.Lookups],
        [typeof(AdminGetAllRolesQuery)] = [IdentityCacheTags.Lookups],
        [typeof(AdminGetPermissionByIdQuery)] = [IdentityCacheTags.Lookups],
        [typeof(AdminGetRoleByIdQuery)] = [IdentityCacheTags.Lookups],
    };

    /// <summary>
    /// Discovers every concrete cacheable query in the scanned assemblies.
    /// </summary>
    /// <returns>One row per discovered query type, ordered by name for stable output.</returns>
    public static TheoryData<Type> QueryTypes() => new(DiscoverQueryTypes());

    private static IReadOnlyList<Type> DiscoverQueryTypes()
    {
        return
        [
            .. QueryAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type is { IsAbstract: false, IsInterface: false })
                .Where(type => typeof(ICacheableRequest).IsAssignableFrom(type))
                .OrderBy(type => type.Name),
        ];
    }

    private static QueryContract ContractFor(Type queryType)
    {
        Contracts.Should().ContainKey(queryType, $"{queryType.Name} needs a contract entry");
        return Contracts[queryType];
    }

    [Fact]
    public void QueryTypes_ShouldDiscoverEveryCacheableQuery()
    {
        // Assert
        DiscoverQueryTypes().Should().HaveCount(ExpectedCacheableQueryCount);
        Contracts.Should().HaveCount(ExpectedCacheableQueryCount);
    }

    [Theory]
    [MemberData(nameof(QueryTypes))]
    public void CacheKey_WithSameArguments_ShouldBeStable(Type queryType)
    {
        // Arrange
        QueryContract contract = ContractFor(queryType);

        // Assert
        contract.Baseline().CacheKey.Should().Be(contract.Baseline().CacheKey, queryType.Name);
    }

    [Theory]
    [MemberData(nameof(QueryTypes))]
    public void CacheKey_WithEveryKeyedParameterChanged_ShouldDiffer(Type queryType)
    {
        // Arrange
        QueryContract contract = ContractFor(queryType);
        string baselineKey = contract.Baseline().CacheKey;

        // Assert
        foreach (Func<ICacheableRequest> variant in contract.DistinctKeyVariants)
        {
            variant().CacheKey.Should().NotBe(baselineKey, queryType.Name);
        }
    }

    [Theory]
    [MemberData(nameof(QueryTypes))]
    public void IsCacheable_WithPerUserOrUnboundedArguments_ShouldDecline(Type queryType)
    {
        // Arrange
        QueryContract contract = ContractFor(queryType);

        // Assert
        foreach (Func<ICacheableRequest> declining in contract.NotCacheable)
        {
            ((IConditionallyCacheableRequest)declining()).IsCacheable.Should().BeFalse(queryType.Name);
        }

        if (contract.Baseline() is IConditionallyCacheableRequest conditionalBaseline)
        {
            conditionalBaseline.IsCacheable.Should().BeTrue(queryType.Name);
        }
    }

    [Theory]
    [MemberData(nameof(QueryTypes))]
    public void Ttl_ShouldMatchTheContract(Type queryType)
    {
        // Assert
        ExpectedTtls.Should().ContainKey(queryType, $"{queryType.Name} needs a ttl entry");
        ContractFor(queryType).Baseline().Ttl.Should().Be(ExpectedTtls[queryType], queryType.Name);
    }

    [Theory]
    [MemberData(nameof(QueryTypes))]
    public void CacheTags_ShouldMatchTheContract(Type queryType)
    {
        // Assert
        ExpectedTags.Should().ContainKey(queryType, $"{queryType.Name} needs a tag entry");
        ContractFor(queryType).Baseline().CacheTags.Should().BeEquivalentTo(ExpectedTags[queryType], queryType.Name);
    }

    /// <summary>
    /// The cache-contract expectations for one query type.
    /// </summary>
    /// <param name="Baseline">Builds the reference instance.</param>
    /// <param name="DistinctKeyVariants">Each must produce a key different from the baseline's.</param>
    /// <param name="NotCacheable">Each must decline the cache.</param>
    private sealed record QueryContract(
        Func<ICacheableRequest> Baseline,
        IReadOnlyList<Func<ICacheableRequest>> DistinctKeyVariants,
        IReadOnlyList<Func<ICacheableRequest>> NotCacheable
    );
}
