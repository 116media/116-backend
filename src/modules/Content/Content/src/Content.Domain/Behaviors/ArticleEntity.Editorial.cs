using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Editorial behaviour of <see cref="ArticleEntity" />. Its state lives in <c>Entities/ArticleEntity.cs</c>.
/// </summary>
public sealed partial class ArticleEntity
{
    /// <summary>
    /// Renames the article and its slug. Allowed while the article is editable — <c>Draft</c>,
    /// <c>PendingPayment</c>, <c>PendingReview</c> or <c>Rejected</c>; slug uniqueness is the
    /// handler's to enforce.
    /// </summary>
    /// <param name="title">The article title.</param>
    /// <param name="slug">The URL-safe slug. Uniqueness enforced by handler.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool Retitle(string title, string slug)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (Title == title && Slug == slug)
        {
            return false;
        }

        Title = title;
        Slug = slug;

        return true;
    }

    /// <summary>
    /// Revises the teaser and the rich-text body, declaring the body images the new body drops.
    /// </summary>
    /// <param name="headline">The short teaser text (100–300 chars; min enforced by validator).</param>
    /// <param name="body">The rich-text HTML body containing only Cloudinary URLs.</param>
    /// <param name="orphanedBodyImageStorageKeys">
    /// Storage keys of body images that drop out of the new body, computed by the handler
    /// against the pre-update image set. When non-empty the revision declares the orphaning
    /// so post-commit consumers can remove the rows and the remote assets.
    /// </param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool ReviseBody(string headline, string body, IReadOnlyList<string>? orphanedBodyImageStorageKeys = null)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (Headline == headline && Body == body)
        {
            return false;
        }

        Headline = headline;
        Body = body;

        if (orphanedBodyImageStorageKeys is { Count: > 0 })
        {
            AddDomainEvent(
                new ArticleBodyImagesOrphanedEvent(ArticleId: Id, StorageKeys: orphanedBodyImageStorageKeys)
            );
        }

        return true;
    }

    /// <summary>
    /// Moves the article to another category. Existence of the category is checked by the handler.
    /// </summary>
    /// <param name="categoryId">The category this article belongs to.</param>
    /// <returns><c>true</c> if the category changed; otherwise <c>false</c>.</returns>
    public bool Recategorize(Guid categoryId)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (CategoryId == categoryId)
        {
            return false;
        }

        CategoryId = categoryId;

        return true;
    }

    /// <summary>
    /// Assigns — or clears — the commission this article fulfils, together with its social boost flag.
    /// </summary>
    /// <param name="customerId">The B2B customer who commissioned this article. <c>null</c> for free content.</param>
    /// <param name="orderItemId">The order item this article fulfils. <c>null</c> for free content.</param>
    /// <param name="socialBoost">Whether this article is flagged for social media promotion.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool AssignCommission(Guid? customerId, Guid? orderItemId, bool socialBoost)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (CustomerId == customerId && OrderItemId == orderItemId && SocialBoost == socialBoost)
        {
            return false;
        }

        CustomerId = customerId;
        OrderItemId = orderItemId;
        SocialBoost = socialBoost;

        return true;
    }

    /// <summary>
    /// Sets the cover image file reference. Called by <c>UploadArticleImageCommandHandler</c>
    /// when a <c>Cover</c>-type image is uploaded.
    /// </summary>
    /// <param name="coverImageFileId">
    /// The FileEntity ID for the uploaded cover image, or null to clear it.
    /// </param>
    public void UpdateCoverImage(Guid? coverImageFileId)
    {
        CoverImageFileId = coverImageFileId;
    }

    /// <summary>
    /// Revises the SEO metadata. Unlike the editorial verbs this is allowed at any status, since
    /// search metadata is maintained after publication.
    /// </summary>
    /// <param name="metaTitle">Optional SEO meta title (max 70 chars). Falls back to <c>Title</c> if null.</param>
    /// <param name="metaDescription">Optional SEO meta description (max 160 chars).</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool ReviseSeo(string? metaTitle, string? metaDescription)
    {
        if (MetaTitle == metaTitle && MetaDescription == metaDescription)
        {
            return false;
        }

        MetaTitle = metaTitle;
        MetaDescription = metaDescription;

        return true;
    }

    /// <summary>
    /// Adds an image row to this article. The identifier is supplied by the caller because the
    /// storage public id is derived from it before the upload happens.
    /// </summary>
    /// <param name="id">The image row's identifier.</param>
    /// <param name="storageKey">The provider storage key of the uploaded asset.</param>
    /// <param name="url">The public delivery URL of the uploaded asset.</param>
    /// <param name="imageType">Whether the image is the cover or a body illustration.</param>
    /// <returns>The image row that was added.</returns>
    public ArticleImageEntity AddImage(Guid id, string storageKey, string url, EnumArticleImageType imageType)
    {
        ArticleImageEntity image = ArticleImageEntity.Create(
            id: id,
            articleId: Id,
            storageKey: storageKey,
            url: url,
            imageType: imageType
        );

        Images.Add(image);

        return image;
    }

    /// <summary>
    /// Removes the cover image row, returning it so the caller can release the stored asset,
    /// or null when the article has no cover.
    /// </summary>
    /// <returns>The removed cover row, or <c>null</c>.</returns>
    public ArticleImageEntity? RemoveCoverImage()
    {
        ArticleImageEntity? cover = Images.FirstOrDefault(image => image.ImageType == EnumArticleImageType.Cover);

        if (cover is not null)
        {
            Images.Remove(cover);
        }

        return cover;
    }

    /// <summary>
    /// Removes the body image rows carrying the given storage keys, returning them so the
    /// caller can release the stored assets.
    /// </summary>
    /// <param name="storageKeys">The storage keys of the rows to remove.</param>
    /// <returns>The removed rows.</returns>
    public IReadOnlyList<ArticleImageEntity> RemoveBodyImages(IEnumerable<string> storageKeys)
    {
        HashSet<string> keys = storageKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<ArticleImageEntity> removed = Images
            .Where(image => image.ImageType == EnumArticleImageType.Body && keys.Contains(image.StorageKey))
            .ToList();

        foreach (ArticleImageEntity image in removed)
        {
            Images.Remove(image);
        }

        return removed;
    }

    /// <summary>
    /// Replaces the tag set with the given ids, raising one <see cref="TagGraphChangedEvent" />
    /// per tag that joins or leaves. An identical set writes nothing and raises nothing.
    /// </summary>
    /// <param name="tagIds">The complete tag set this row should carry.</param>
    /// <returns><c>true</c> if the set changed; otherwise <c>false</c>.</returns>
    public bool ReplaceTags(IReadOnlyCollection<Guid> tagIds)
    {
        HashSet<Guid> desired = tagIds.ToHashSet();
        HashSet<Guid> current = Tags.Select(tag => tag.TagId).ToHashSet();

        if (desired.SetEquals(current))
        {
            return false;
        }

        foreach (ArticleTagEntity removed in Tags.Where(tag => !desired.Contains(tag.TagId)).ToList())
        {
            Tags.Remove(removed);
            AddDomainEvent(new TagGraphChangedEvent(TagId: removed.TagId));
        }

        foreach (Guid tagId in desired.Where(id => !current.Contains(id)))
        {
            Tags.Add(ArticleTagEntity.Create(id: Guid.NewGuid(), articleId: Id, tagId: tagId));
            AddDomainEvent(new TagGraphChangedEvent(TagId: tagId));
        }

        return true;
    }

    /// <summary>
    /// Replaces the credited-artist set with the given ids. An identical set writes nothing.
    /// </summary>
    /// <param name="artistIds">The complete artist set this article should credit.</param>
    /// <returns><c>true</c> if the set changed; otherwise <c>false</c>.</returns>
    public bool ReplaceArtists(IReadOnlyCollection<Guid> artistIds)
    {
        HashSet<Guid> desired = artistIds.ToHashSet();
        HashSet<Guid> current = Artists.Select(credit => credit.ArtistId).ToHashSet();

        if (desired.SetEquals(current))
        {
            return false;
        }

        foreach (ArticleArtistEntity removed in Artists.Where(credit => !desired.Contains(credit.ArtistId)).ToList())
        {
            Artists.Remove(removed);
        }

        foreach (Guid artistId in desired.Where(id => !current.Contains(id)))
        {
            Artists.Add(ArticleArtistEntity.Create(id: Guid.NewGuid(), articleId: Id, artistId: artistId));
        }

        return true;
    }
}
