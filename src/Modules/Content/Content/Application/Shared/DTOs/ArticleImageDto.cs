namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a single image asset associated with an article.
/// </summary>
/// <param name="Id">The unique identifier of the image record.</param>
/// <param name="Url">The publicly accessible URL of the image.</param>
/// <param name="StorageKey">The provider-agnostic storage key used for CDN asset management.</param>
/// <param name="ImageType">The image type: <c>Cover</c> or <c>Body</c>.</param>
public record ArticleImageDto(Guid Id, string Url, string StorageKey, string ImageType);
