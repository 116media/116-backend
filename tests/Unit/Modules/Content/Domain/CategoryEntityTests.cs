using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain;

/// <summary>
/// Unit tests for <see cref="CategoryEntity"/> domain behaviour.
/// </summary>
public class CategoryEntityTests
{
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

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void EnsureCommissionable_WhenFree_ShouldThrowNotFoundException()
    {
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.CreateFree(contentTypeId);

        Action act = () => category.EnsureCommissionable();

        act.Should().Throw<NotFoundException>();
    }

    #endregion
}
