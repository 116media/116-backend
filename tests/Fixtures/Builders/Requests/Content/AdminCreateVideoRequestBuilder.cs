using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateVideoRequest"/> instances in tests
/// with valid default values that satisfy the create video validator.
/// </summary>
public class AdminCreateVideoRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _categoryId;
    private string _title;
    private string _slug;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private string _description;
    private DateTimeOffset? _shootingScheduledAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateVideoRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminCreateVideoRequestBuilder()
    {
        _categoryId = _faker.Random.Guid();
        _title = TestConstants.Video.ValidTitle;
        _slug = TestConstants.Video.ValidSlug;
        _customerId = null;
        _orderItemId = null;
        _description = TestConstants.Video.ValidDescription;
        _shootingScheduledAt = null;
    }

    /// <summary>
    /// Sets the category the video belongs to.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the video display title.
    /// </summary>
    /// <param name="title">The video title.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the URL-safe slug for the video.
    /// </summary>
    /// <param name="slug">The video slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the optional B2B customer who commissioned the video.
    /// </summary>
    /// <param name="customerId">The customer identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional order item the video fulfils.
    /// </summary>
    /// <param name="orderItemId">The order item identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithOrderItemId(Guid? orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Sets the video description.
    /// </summary>
    /// <param name="description">The video description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the optional shooting scheduled date.
    /// </summary>
    /// <param name="shootingScheduledAt">The shooting scheduled date, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateVideoRequestBuilder WithShootingScheduledAt(DateTimeOffset? shootingScheduledAt)
    {
        _shootingScheduledAt = shootingScheduledAt;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateVideoRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateVideoRequest instance.</returns>
    public AdminCreateVideoRequest Build()
    {
        return new AdminCreateVideoRequest(
            CategoryId: _categoryId,
            Title: _title,
            Slug: _slug,
            CustomerId: _customerId,
            OrderItemId: _orderItemId,
            Description: _description,
            ShootingScheduledAt: _shootingScheduledAt
        );
    }
}
