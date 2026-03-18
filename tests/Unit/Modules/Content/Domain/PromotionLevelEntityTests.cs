using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain;

/// <summary>
/// Unit tests for <see cref="PromotionLevelEntity"/> domain behaviour.
/// </summary>
public class PromotionLevelEntityTests
{
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

        act.Should().Throw<NotFoundException>();
    }

    #endregion
}
