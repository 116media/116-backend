using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ArticleImageEntity"/> instances in tests.
/// For test code, prefer using ArticleImageFactory instead of direct Builder usage.
/// </summary>
internal class ArticleImageBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _articleId;
    private string _storageKey = TestConstants.Content.Editorial.ArticleImage.ValidStorageKey;
    private string _url = TestConstants.Content.Editorial.ArticleImage.ValidUrl;
    private EnumArticleImageType _imageType = EnumArticleImageType.Body;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleImageBuilder"/> class with a required article ID.
    /// </summary>
    public ArticleImageBuilder(Guid articleId)
    {
        _articleId = articleId;
    }

    /// <summary>Sets the image record ID.</summary>
    public ArticleImageBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the article ID.</summary>
    public ArticleImageBuilder WithArticleId(Guid articleId)
    {
        _articleId = articleId;
        return this;
    }

    /// <summary>Sets the storage key.</summary>
    public ArticleImageBuilder WithStorageKey(string storageKey)
    {
        _storageKey = storageKey;
        return this;
    }

    /// <summary>Sets the public URL.</summary>
    public ArticleImageBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    /// <summary>Marks the image as a cover image.</summary>
    public ArticleImageBuilder AsCover()
    {
        _imageType = EnumArticleImageType.Cover;
        return this;
    }

    /// <summary>Marks the image as a body image.</summary>
    public ArticleImageBuilder AsBody()
    {
        _imageType = EnumArticleImageType.Body;
        return this;
    }

    /// <summary>Builds the <see cref="ArticleImageEntity"/> instance.</summary>
    public ArticleImageEntity Build()
    {
        return ArticleImageEntity.Create(
            id: _id,
            articleId: _articleId,
            storageKey: _storageKey,
            url: _url,
            imageType: _imageType
        );
    }
}
