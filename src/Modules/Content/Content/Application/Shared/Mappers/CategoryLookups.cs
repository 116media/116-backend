using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// The rows a category projection names but does not own — resolved in one batch per request so
/// the mapper reads names without a navigation or a per-row query.
/// </summary>
/// <param name="Posters">Category poster files, keyed by file id.</param>
/// <param name="ContentTypes">The content types categories are classified under, keyed by id.</param>
/// <param name="PricingTiers">The tiers category pricing rows price, keyed by id.</param>
public record CategoryLookups(
    IReadOnlyDictionary<Guid, FileReferenceDto> Posters,
    IReadOnlyDictionary<Guid, ContentTypeEntity> ContentTypes,
    IReadOnlyDictionary<Guid, PricingTierEntity> PricingTiers
);
