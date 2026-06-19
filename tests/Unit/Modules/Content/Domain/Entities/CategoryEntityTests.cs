using _116.Content.Domain.Entities;
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

        Action act = () => CategoryEntity.Create(id, contentTypeId, "   ", "valid-slug", "desc", false, errors);

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldThrowBadRequestException()
    {
        Guid id = Guid.NewGuid();
        Guid contentTypeId = Guid.NewGuid();
        var errors = TestErrorsFactory.CreateCategoryErrors();

        Action act = () => CategoryEntity.Create(id, contentTypeId, "Valid Name", "", "desc", false, errors);

        act.Should().Throw<BadRequestException>();
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
            errors,
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

        category.Update("New Name", "new-slug", "New description", false, false, errors);

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

        Action act = () => category.Update("", "valid-slug", "desc", false, false, errors);

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Update_WithEmptySlug_ShouldThrowBadRequestException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        var errors = TestErrorsFactory.CreateCategoryErrors();

        Action act = () => category.Update("Valid Name", "  ", "desc", false, false, errors);

        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region EnsureCommissionable

    [Fact]
    public void EnsureCommissionable_WhenActiveAndPaid_ShouldNotThrow()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreatePaid(contentTypeId);

        Action act = () => category.EnsureCommissionable(TestErrorsFactory.CreateCategoryErrors());

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCommissionable_WhenInactive_ShouldThrowNotFoundException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateInactive(contentTypeId);

        Action act = () => category.EnsureCommissionable(TestErrorsFactory.CreateCategoryErrors());

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void EnsureCommissionable_WhenFree_ShouldThrowNotFoundException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateFree(contentTypeId);

        Action act = () => category.EnsureCommissionable(TestErrorsFactory.CreateCategoryErrors());

        act.Should().Throw<NotFoundException>();
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

        category.Update("Name", "slug", "desc", false, true, errors);

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
}
