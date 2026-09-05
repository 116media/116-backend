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
/// Unit tests for <see cref="CategoryEntity"/> domain behaviour.
/// </summary>
public class CategoryEntityTests
{
    #region Create

    [Fact]
    public void Create_WithValidArguments_ShouldReturnEntity()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        category.Should().NotBeNull();
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowBadRequestException()
    {
        Guid id = Guid.NewGuid();
        Guid contentTypeId = Guid.NewGuid();
        var errors = TestErrorsFactory.CreateCategoryErrors();

        Action act = () => CategoryEntity.Create(id, contentTypeId, "   ", "valid-slug", "desc", false);

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.CategoryNameRequired);
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldThrowBadRequestException()
    {
        Guid id = Guid.NewGuid();
        Guid contentTypeId = Guid.NewGuid();
        var errors = TestErrorsFactory.CreateCategoryErrors();

        Action act = () => CategoryEntity.Create(id, contentTypeId, "Valid Name", "", "desc", false);

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.CategorySlugRequired);
    }

    [Fact]
    public void Create_WithIsExclusive_ShouldSetProperty()
    {
        Guid id = Guid.NewGuid();
        Guid contentTypeId = Guid.NewGuid();
        var errors = TestErrorsFactory.CreateCategoryErrors();

        CategoryEntity category = CategoryEntity.Create(
            id,
            contentTypeId,
            "Test",
            "test",
            "desc",
            false,
            isExclusive: true
        );

        category.IsExclusive.Should().BeTrue();
    }

    [Fact]
    public void Create_DefaultIsExclusive_ShouldBeFalse()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        category.IsExclusive.Should().BeFalse();
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithValidArguments_ShouldUpdateFields()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        var errors = TestErrorsFactory.CreateCategoryErrors();

        category.Update("New Name", "new-slug", "New description", false, false, false);

        category.Name.Should().Be("New Name");
        category.Slug.Should().Be("new-slug");
        category.Description.Should().Be("New description");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowBadRequestException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        var errors = TestErrorsFactory.CreateCategoryErrors();

        Action act = () => category.Update("", "valid-slug", "desc", false, false, false);

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.CategoryNameRequired);
    }

    [Fact]
    public void Update_WithEmptySlug_ShouldThrowBadRequestException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        var errors = TestErrorsFactory.CreateCategoryErrors();

        Action act = () => category.Update("Valid Name", "  ", "desc", false, false, false);

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.CategorySlugRequired);
    }

    #endregion

    #region EnsureCommissionable

    [Fact]
    public void EnsureCommissionable_WhenActiveAndPaid_ShouldNotThrow()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreatePaid(contentTypeId);

        Action act = () => category.EnsureCommissionable();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCommissionable_WhenInactive_ShouldThrowNotFoundException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateInactive(contentTypeId);

        Action act = () => category.EnsureCommissionable();

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.CategoryNotFound);
    }

    [Fact]
    public void EnsureCommissionable_WhenFree_ShouldThrowNotFoundException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateFree(contentTypeId);

        Action act = () => category.EnsureCommissionable();

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.CategoryNotFound);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrueAndSetActive()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateInactive(contentTypeId);

        bool result = category.Activate();

        result.Should().BeTrue();
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        bool result = category.Activate();

        result.Should().BeFalse();
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrueAndSetInactive()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        bool result = category.Deactivate();

        result.Should().BeTrue();
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateInactive(contentTypeId);

        bool result = category.Deactivate();

        result.Should().BeFalse();
    }

    #endregion

    #region Update IsExclusive

    [Fact]
    public void Update_SetsIsExclusive()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        var errors = TestErrorsFactory.CreateCategoryErrors();

        category.Update("Name", "slug", "desc", false, true, false);

        category.IsExclusive.Should().BeTrue();
    }

    #endregion

    #region SetExclusive / ClearExclusive

    [Fact]
    public void SetExclusive_ShouldSetToTrue()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        category.SetExclusive();

        category.IsExclusive.Should().BeTrue();
    }

    [Fact]
    public void ClearExclusive_ShouldSetToFalse()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        category.SetExclusive();

        category.ClearExclusive();

        category.IsExclusive.Should().BeFalse();
    }

    #endregion

    #region SetPosterFileId

    [Fact]
    public void SetPosterFileId_ShouldSetValue()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        Guid posterFileId = Guid.NewGuid();

        category.SetPosterFileId(posterFileId);

        category.PosterFileId.Should().Be(posterFileId);
    }

    [Fact]
    public void SetPosterFileId_WithNull_ShouldClearValue()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        category.SetPosterFileId(Guid.NewGuid());

        category.SetPosterFileId(null);

        category.PosterFileId.Should().BeNull();
    }

    #endregion

    #region PinToFeed / UnpinFromFeed

    [Fact]
    public void PinToFeed_WhenNotPinned_ShouldSetTimestampAndFlag()
    {
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());

        category.PinToFeed();

        category.PinnedToFeedAt.Should().NotBeNull();
        category.IsPinnedToFeed.Should().BeTrue();
    }

    [Fact]
    public void PinToFeed_WhenAlreadyPinned_ShouldRefreshTimestampForward()
    {
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        category.PinToFeed();
        DateTimeOffset first = category.PinnedToFeedAt!.Value;

        category.PinToFeed();

        category.PinnedToFeedAt!.Value.Should().BeOnOrAfter(first);
    }

    [Fact]
    public void UnpinFromFeed_WhenPinned_ShouldClearAndReturnTrue()
    {
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        category.PinToFeed();

        bool result = category.UnpinFromFeed();

        result.Should().BeTrue();
        category.PinnedToFeedAt.Should().BeNull();
        category.IsPinnedToFeed.Should().BeFalse();
    }

    [Fact]
    public void UnpinFromFeed_WhenNotPinned_ShouldReturnFalse()
    {
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());

        bool result = category.UnpinFromFeed();

        result.Should().BeFalse();
        category.PinnedToFeedAt.Should().BeNull();
    }

    [Fact]
    public void IsPinnedToFeed_ShouldReflectTimestampPresence()
    {
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());

        category.IsPinnedToFeed.Should().BeFalse();
        category.PinToFeed();
        category.IsPinnedToFeed.Should().BeTrue();
    }

    #endregion

    #region IsDefaultForLyrics

    [Fact]
    public void Create_DefaultIsDefaultForLyrics_ShouldBeFalse()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        category.IsDefaultForLyrics.Should().BeFalse();
    }

    [Fact]
    public void Create_WithIsDefaultForLyrics_ShouldSetProperty()
    {
        Guid id = Guid.NewGuid();
        Guid contentTypeId = Guid.NewGuid();
        var errors = TestErrorsFactory.CreateCategoryErrors();

        CategoryEntity category = CategoryEntity.Create(
            id,
            contentTypeId,
            "Test",
            "test",
            "desc",
            false,
            isDefaultForLyrics: true
        );

        category.IsDefaultForLyrics.Should().BeTrue();
    }

    [Fact]
    public void SetDefaultForLyrics_ShouldSetToTrue()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);

        category.SetDefaultForLyrics();

        category.IsDefaultForLyrics.Should().BeTrue();
    }

    [Fact]
    public void ClearDefaultForLyrics_ShouldSetToFalse()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        category.SetDefaultForLyrics();

        category.ClearDefaultForLyrics();

        category.IsDefaultForLyrics.Should().BeFalse();
    }

    [Fact]
    public void Update_SetsIsDefaultForLyrics()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        var errors = TestErrorsFactory.CreateCategoryErrors();

        category.Update("Name", "slug", "desc", false, false, isDefaultForLyrics: true);

        category.IsDefaultForLyrics.Should().BeTrue();
    }

    #endregion
}
