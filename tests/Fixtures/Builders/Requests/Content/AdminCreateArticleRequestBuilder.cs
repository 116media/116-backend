using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateArticleRequest"/> instances in tests
/// with valid default values that satisfy the create article validator.
/// </summary>
public class AdminCreateArticleRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _categoryId;
    private string _title;
    private string _slug;
    private Guid? _customerId;
    private Guid? _orderItemId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateArticleRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminCreateArticleRequestBuilder()
    {
        _categoryId = _faker.Random.Guid();
        _title = TestConstants.Article.ValidTitle;
        _slug = TestConstants.Article.ValidSlug;
        _customerId = null;
        _orderItemId = null;
    }

    /// <summary>
    /// Sets the category the article belongs to.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArticleRequestBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the article display title.
    /// </summary>
    /// <param name="title">The article title.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArticleRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the URL-safe slug for the article.
    /// </summary>
    /// <param name="slug">The article slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArticleRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the optional B2B customer who commissioned the article.
    /// </summary>
    /// <param name="customerId">The customer identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArticleRequestBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    /// <summary>
    /// Sets the optional order item the article fulfils.
    /// </summary>
    /// <param name="orderItemId">The order item identifier, or null for free content.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArticleRequestBuilder WithOrderItemId(Guid? orderItemId)
    {
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateArticleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateArticleRequest instance.</returns>
    public AdminCreateArticleRequest Build()
    {
        return new AdminCreateArticleRequest(
            CategoryId: _categoryId,
            Title: _title,
            Slug: _slug,
            CustomerId: _customerId,
            OrderItemId: _orderItemId
        );
    }
}
