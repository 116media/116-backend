using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;

/// <summary>
/// Query for retrieving a page of published articles tagged to an artist, addressed by the
/// artist's slug. Returns the articles module's own summary DTO so the client renders its
/// existing article card — a bespoke news DTO would fork that card.
/// </summary>
/// <param name="Slug">The URL-safe slug of the artist profile.</param>
/// <param name="Page">Pagination parameters for the article list.</param>
public record PublicGetArtistArticlesQuery(string Slug, PaginatedRequest Page) : IQuery<PublicGetArtistArticlesResult>;

/// <summary>
/// Result of the <see cref="PublicGetArtistArticlesQuery" /> containing the paginated articles.
/// </summary>
/// <param name="Articles">The published articles tagged to the artist, newest first.</param>
public record PublicGetArtistArticlesResult(PaginatedResult<ArticleSummaryDto> Articles);
