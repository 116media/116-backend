using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateVideoRequest"/> instances in tests
/// with valid default values that satisfy the update video validator.
/// </summary>
public class AdminUpdateVideoRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _categoryId;
    private string _title;
    private string _slug;
    private string _description;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private bool _socialBoost;
    private string? _metaTitle;
    private string? _metaDescription;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateVideoRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminUpdateVideoRequestBuilder()
    {
        _categoryId = _faker.Random.Guid();
        _title = TestConstants.Video.ValidTitle;
        _slug = TestConstants.Video.ValidSlug;
        _description = TestConstants.Video.ValidDescription;
        _customerId = null;
        _orderItemId = null;
        _socialBoost = false;
        _metaTitle = null;
        _metaDescription = null;
    }

    /// <summary>
    /// Sets the category the video belongs to.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the video title.
    /// </summary>
    /// <param name="title">The video title.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the URL-safe slug for the video.
    /// </summary>
    /// <param name="slug">The video slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the video description.
    /// </summary>
    /// <param name="description">The video description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the optional B2B customer who commissioned the video.
    /// </summary>
    /// <param name="customerId">The customer identifier, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional order item the video fulfils.
    /// </summary>
    /// <param name="orderItemId">The order item identifier, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithOrderItemId(Guid? orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Sets whether the video is flagged for social media promotion.
    /// </summary>
    /// <param name="socialBoost">The social boost flag.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithSocialBoost(bool socialBoost)
    {
        _socialBoost = socialBoost;
        return this;
    }

    /// <summary>
    /// Sets the optional SEO meta title.
    /// </summary>
    /// <param name="metaTitle">The meta title, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithMetaTitle(string? metaTitle)
    {
        _metaTitle = metaTitle;
        return this;
    }

    /// <summary>
    /// Sets the optional SEO meta description.
    /// </summary>
    /// <param name="metaDescription">The meta description, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateVideoRequestBuilder WithMetaDescription(string? metaDescription)
    {
        _metaDescription = metaDescription;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateVideoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateVideoRequest instance.</returns>
    public AdminUpdateVideoRequest Build()
    {
        return new AdminUpdateVideoRequest(
            CategoryId: _categoryId,
            Title: _title,
            Slug: _slug,
            Description: _description,
            CustomerId: _customerId,
            OrderItemId: _orderItemId,
            SocialBoost: _socialBoost,
            MetaTitle: _metaTitle,
            MetaDescription: _metaDescription
        );
    }
}
