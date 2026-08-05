using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateArticleRequest"/> instances in tests
/// with valid default values that satisfy the update article validator.
/// </summary>
public class AdminUpdateArticleRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _categoryId;
    private string _title;
    private string _slug;
    private string _headline;
    private string _body;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private bool _socialBoost;
    private string? _metaTitle;
    private string? _metaDescription;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateArticleRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminUpdateArticleRequestBuilder()
    {
        _categoryId = _faker.Random.Guid();
        _title = TestConstants.Article.ValidTitle;
        _slug = TestConstants.Article.ValidSlug;
        _headline = TestConstants.Article.ValidHeadline;
        _body = TestConstants.Article.ValidBody;
        _customerId = null;
        _orderItemId = null;
        _socialBoost = false;
        _metaTitle = null;
        _metaDescription = null;
    }

    /// <summary>
    /// Sets the category the article belongs to.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the article title.
    /// </summary>
    /// <param name="title">The article title.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the URL-safe slug for the article.
    /// </summary>
    /// <param name="slug">The article slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the short teaser headline text.
    /// </summary>
    /// <param name="headline">The headline text.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithHeadline(string headline)
    {
        _headline = headline;
        return this;
    }

    /// <summary>
    /// Sets the rich-text HTML body.
    /// </summary>
    /// <param name="body">The article body.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Sets the optional B2B customer who commissioned the article.
    /// </summary>
    /// <param name="customerId">The customer identifier, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional order item the article fulfils.
    /// </summary>
    /// <param name="orderItemId">The order item identifier, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithOrderItemId(Guid? orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Sets whether the article is flagged for social media promotion.
    /// </summary>
    /// <param name="socialBoost">The social boost flag.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithSocialBoost(bool socialBoost)
    {
        _socialBoost = socialBoost;
        return this;
    }

    /// <summary>
    /// Sets the optional SEO meta title.
    /// </summary>
    /// <param name="metaTitle">The meta title, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithMetaTitle(string? metaTitle)
    {
        _metaTitle = metaTitle;
        return this;
    }

    /// <summary>
    /// Sets the optional SEO meta description.
    /// </summary>
    /// <param name="metaDescription">The meta description, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArticleRequestBuilder WithMetaDescription(string? metaDescription)
    {
        _metaDescription = metaDescription;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateArticleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateArticleRequest instance.</returns>
    public AdminUpdateArticleRequest Build()
    {
        return new AdminUpdateArticleRequest(
            CategoryId: _categoryId,
            Title: _title,
            Slug: _slug,
            Headline: _headline,
            Body: _body,
            CustomerId: _customerId,
            OrderItemId: _orderItemId,
            SocialBoost: _socialBoost,
            MetaTitle: _metaTitle,
            MetaDescription: _metaDescription
        );
    }
}
