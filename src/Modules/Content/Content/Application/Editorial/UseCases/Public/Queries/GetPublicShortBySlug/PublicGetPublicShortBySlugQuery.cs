using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Query for retrieving a single active short video by its URL slug for public consumption.
/// </summary>
/// <param name="Slug">The URL-safe slug of the short video to retrieve.</param>
public record PublicGetPublicShortBySlugQuery(string Slug) : IQuery<PublicGetPublicShortBySlugResult>;

/// <summary>
/// Result of the <see cref="PublicGetPublicShortBySlugQuery" /> containing the short video details.
/// </summary>
/// <param name="ShortVideo">The short video information.</param>
public record PublicGetPublicShortBySlugResult(ShortVideoDto ShortVideo);
