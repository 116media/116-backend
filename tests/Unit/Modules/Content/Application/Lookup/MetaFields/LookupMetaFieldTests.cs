using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;
using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;
using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;
using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.MetaFields;

/// <summary>
/// Tests that all Lookup MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class LookupMetaFieldTests
{
    #region ContentType MetaFields

    [Fact]
    public void ActivateContentTypeMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminActivateContentTypeMetaField.ActivateContentType;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void DeactivateContentTypeMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeactivateContentTypeMetaField.AdminDeactivateContentType;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void CreateContentTypeMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateContentTypeMetaField.CreateContentType;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContentTypeMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateContentTypeMetaField.AdminUpdateContentType;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region PricingTier MetaFields

    [Fact]
    public void ActivatePricingTierMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminActivatePricingTierMetaField.ActivatePricingTier;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void DeactivatePricingTierMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeactivatePricingTierMetaField.AdminDeactivatePricingTier;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void CreatePricingTierMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreatePricingTierMetaField.CreatePricingTier;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePricingTierMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdatePricingTierMetaField.AdminUpdatePricingTier;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region PromotionLevel MetaFields

    [Fact]
    public void ActivatePromotionLevelMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminActivatePromotionLevelMetaField.ActivatePromotionLevel;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void DeactivatePromotionLevelMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeactivatePromotionLevelMetaField.AdminDeactivatePromotionLevel;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void CreatePromotionLevelMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreatePromotionLevelMetaField.AdminCreatePromotionLevel;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePromotionLevelMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdatePromotionLevelMetaField.AdminUpdatePromotionLevel;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Tag MetaFields

    [Fact]
    public void CreateTagMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateTagMetaField.AdminCreateTag;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Admin Query MetaFields

    [Fact]
    public void GetAllContentTypesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllContentTypesMetaField.AdminGetAllContentTypes;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetAllPricingTiersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllPricingTiersMetaField.AdminGetAllPricingTiers;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetAllPromotionLevelsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllPromotionLevelsMetaField.AdminGetAllPromotionLevels;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Query MetaFields

    [Fact]
    public void GetActivePromotionLevelsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetActivePromotionLevelsMetaField.PublicGetActivePromotionLevels;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetAllTagsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetAllTagsMetaField.PublicGetAllTags;
        metadata.Should().NotBeNull();
    }

    #endregion
}
