using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminAddOrderItemRequest"/> instances in tests.
/// </summary>
public class AdminAddOrderItemRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private EnumCoreContentType _contentKind;
    private string _categoryId;
    private Guid? _promotionLevelId;
    private bool _socialBoost;
    private bool _isBonus;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAddOrderItemRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminAddOrderItemRequestBuilder()
    {
        _contentKind = EnumCoreContentType.Article;
        _categoryId = _faker.Random.Guid().ToString();
        _promotionLevelId = null;
        _socialBoost = false;
        _isBonus = false;
    }

    /// <summary>
    /// Sets the kind of content being commissioned.
    /// </summary>
    /// <param name="contentKind">The content kind (Article or Video).</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddOrderItemRequestBuilder WithContentKind(EnumCoreContentType contentKind)
    {
        _contentKind = contentKind;
        return this;
    }

    /// <summary>
    /// Sets the identifier of the category for this content item.
    /// </summary>
    /// <param name="categoryId">The category identifier as a string GUID.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddOrderItemRequestBuilder WithCategoryId(string categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the optional homepage promotion level for this content item.
    /// </summary>
    /// <param name="promotionLevelId">The promotion level identifier, or null for none.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddOrderItemRequestBuilder WithPromotionLevelId(Guid? promotionLevelId)
    {
        _promotionLevelId = promotionLevelId;
        return this;
    }

    /// <summary>
    /// Sets whether social media promotion is requested for this content item.
    /// </summary>
    /// <param name="socialBoost">True to request social media promotion.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddOrderItemRequestBuilder WithSocialBoost(bool socialBoost)
    {
        _socialBoost = socialBoost;
        return this;
    }

    /// <summary>
    /// Sets whether this is a bonus or complimentary content item.
    /// </summary>
    /// <param name="isBonus">True to mark the item as a bonus.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAddOrderItemRequestBuilder WithIsBonus(bool isBonus)
    {
        _isBonus = isBonus;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAddOrderItemRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAddOrderItemRequest instance.</returns>
    public AdminAddOrderItemRequest Build()
    {
        return new AdminAddOrderItemRequest(
            ContentKind: _contentKind,
            CategoryId: _categoryId,
            PromotionLevelId: _promotionLevelId,
            SocialBoost: _socialBoost,
            IsBonus: _isBonus
        );
    }
}
