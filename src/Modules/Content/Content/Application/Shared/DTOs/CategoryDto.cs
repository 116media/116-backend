namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a content category with its pricing configuration.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
/// <param name="ContentTypeId">The unique identifier of the associated content type.</param>
/// <param name="ContentTypeName">The display name of the associated content type.</param>
/// <param name="Name">The display name of the category.</param>
/// <param name="Slug">The URL-safe slug of the category.</param>
/// <param name="IsFree">Whether content in this category requires no payment.</param>
/// <param name="IsActive">Whether the category is currently active.</param>
/// <param name="Pricing">The pricing tiers configured for this category.</param>
public record CategoryDto(
    Guid Id,
    Guid ContentTypeId,
    string ContentTypeName,
    string Name,
    string Slug,
    bool IsFree,
    bool IsActive,
    IReadOnlyList<CategoryPricingDto> Pricing
);
