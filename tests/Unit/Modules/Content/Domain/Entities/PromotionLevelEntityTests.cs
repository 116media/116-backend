using _116.Content.Domain.Entities;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="PromotionLevelEntity"/> domain behaviour.
/// </summary>
public class PromotionLevelEntityTests
{
    #region Create

    [Fact]
    public void Create_WithValidArguments_ShouldReturnEntity()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();

        promoLevel.Should().NotBeNull();
        promoLevel.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowBadRequestException()
    {
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => PromotionLevelEntity.Create(Guid.NewGuid(), "   ", 7, 50m, 1);

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PromotionLevelNameRequired);
    }

    [Fact]
    public void Create_WithZeroDurationDays_ShouldThrowBadRequestException()
    {
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => PromotionLevelEntity.Create(Guid.NewGuid(), "Valid Name", 0, 50m, 1);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelDurationMustBePositive);
    }

    [Fact]
    public void Create_WithNegativeDurationDays_ShouldThrowBadRequestException()
    {
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => PromotionLevelEntity.Create(Guid.NewGuid(), "Valid Name", -1, 50m, 1);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelDurationMustBePositive);
    }

    [Fact]
    public void Create_WithNegativePriceUsd_ShouldThrowBadRequestException()
    {
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => PromotionLevelEntity.Create(Guid.NewGuid(), "Valid Name", 7, -0.01m, 1);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelPriceMustBeNonNegative);
    }

    [Fact]
    public void Create_WithSpotPriorityZero_ShouldThrowBadRequestException()
    {
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => PromotionLevelEntity.Create(Guid.NewGuid(), "Valid Name", 7, 50m, 0);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelInvalidSpotPriority);
    }

    [Fact]
    public void Create_WithSpotPriorityFour_ShouldThrowBadRequestException()
    {
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => PromotionLevelEntity.Create(Guid.NewGuid(), "Valid Name", 7, 50m, 4);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelInvalidSpotPriority);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithValidArguments_ShouldUpdateFields()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        promoLevel.Update("New Name", 14, 99.99m, 2);

        promoLevel.Name.Should().Be("New Name");
        promoLevel.DurationDays.Should().Be(14);
        promoLevel.PriceUsd.Should().Be(99.99m);
        promoLevel.SpotPriority.Should().Be(2);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowBadRequestException()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => promoLevel.Update("", 7, 50m, 1);

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PromotionLevelNameRequired);
    }

    [Fact]
    public void Update_WithZeroDurationDays_ShouldThrowBadRequestException()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => promoLevel.Update("Name", 0, 50m, 1);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelDurationMustBePositive);
    }

    [Fact]
    public void Update_WithNegativePriceUsd_ShouldThrowBadRequestException()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => promoLevel.Update("Name", 7, -1m, 1);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelPriceMustBeNonNegative);
    }

    [Fact]
    public void Update_WithInvalidSpotPriority_ShouldThrowBadRequestException()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        var errors = TestErrorsFactory.CreatePromotionLevelErrors();

        Action act = () => promoLevel.Update("Name", 7, 50m, 5);

        act.Should()
            .Throw<ContentRuleException>()
            .Which.Code.Should()
            .Be(ContentRuleCodes.PromotionLevelInvalidSpotPriority);
    }

    #endregion

    #region EnsureActive

    [Fact]
    public void EnsureActive_WhenActive_ShouldNotThrow()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();

        Action act = () => promoLevel.EnsureActive();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureActive_WhenInactive_ShouldThrowNotFoundException()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateInactive();

        Action act = () => promoLevel.EnsureActive();

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PromotionLevelNotFound);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrueAndSetActive()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateInactive();

        bool result = promoLevel.Activate();

        result.Should().BeTrue();
        promoLevel.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();

        bool result = promoLevel.Activate();

        result.Should().BeFalse();
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrueAndSetInactive()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();

        bool result = promoLevel.Deactivate();

        result.Should().BeTrue();
        promoLevel.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateInactive();

        bool result = promoLevel.Deactivate();

        result.Should().BeFalse();
    }

    #endregion
}
