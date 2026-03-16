using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Tracks every image asset associated with an article — both cover images and inline body images.
/// Created when the rich-text editor uploads an image during article authoring (step 2).
/// Deleted by the update handler (image diff) and by the background abandoned-draft cleanup job.
/// <para>
/// Uses <see cref="Entity{T}" /> instead of <see cref="Aggregate{T}" /> because images are
/// never updated — they are only created and deleted. Domain events are not needed here.
/// </para>
/// </summary>
public class ArticleImageEntity : Entity<Guid>
{
    /// <summary>
    /// The identifier of the article this image belongs to.
    /// </summary>
    public Guid ArticleId { get; private set; }

    /// <summary>
    /// Provider-agnostic storage identifier for this image asset (e.g., Cloudinary public ID,
    /// AWS S3 key, Bunny CDN path). Named <c>StorageKey</c> rather than
    /// <c>CloudinaryPublicId</c> so the entity remains meaningful if the CDN provider changes.
    /// Passed to <c>ICloudinaryService.DeleteImageAsync()</c> when this image is removed.
    /// </summary>
    public string StorageKey { get; private set; } = null!;

    /// <summary>
    /// The publicly accessible HTTPS URL for this image.
    /// </summary>
    public string Url { get; private set; } = null!;

    /// <summary>
    /// Whether this is the article's cover image or an image embedded in the body.
    /// </summary>
    public EnumArticleImageType ImageType { get; private set; }

    /// <summary>
    /// The article this image belongs to.
    /// </summary>
    public ArticleEntity Article { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ArticleImageEntity() { }

    /// <summary>
    /// Creates a new article image tracking record.
    /// </summary>
    /// <param name="id">The unique identifier for this image record.</param>
    /// <param name="articleId">The identifier of the owning article.</param>
    /// <param name="storageKey">
    /// The provider-agnostic storage key (e.g., Cloudinary public ID) used
    /// to delete this asset from media storage when the image is removed.
    /// </param>
    /// <param name="url">The publicly accessible URL of the uploaded image.</param>
    /// <param name="imageType">Whether this is a cover or body image.</param>
    /// <returns>A new <see cref="ArticleImageEntity" /> instance.</returns>
    public static ArticleImageEntity Create(
        Guid id,
        Guid articleId,
        string storageKey,
        string url,
        EnumArticleImageType imageType
    )
    {
        return new ArticleImageEntity
        {
            Id = id,
            ArticleId = articleId,
            StorageKey = storageKey,
            Url = url,
            ImageType = imageType,
        };
    }
}
