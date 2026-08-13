using _116.Content.Application.Shared.Cache;
using _116.Content.Infrastructure.Cache;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Cache;

/// <summary>
/// Unit tests for the concrete <see cref="CacheInvalidator"/> implementations of the Content
/// module. Verifies token lifecycle: the initial token is live, it is stable across reads, it is
/// cancelled by <see cref="ICacheInvalidator.Invalidate"/>, and a fresh live token is issued for
/// subsequent cache fills.
/// </summary>
public class CacheInvalidatorTests
{
    /// <summary>
    /// The number of concrete domain cache invalidators the Content module declares. Update this
    /// deliberately when one is added — the failure is the notification that the new type needs a
    /// dependency-injection registration review.
    /// </summary>
    private const int ConcreteInvalidatorCount = 3;

    /// <summary>
    /// Enumerates the concrete <see cref="ICacheInvalidator"/> implementations from the Content
    /// assembly rather than from a hand-written list, so an invalidator added to the module is
    /// covered without any change to this file. Reflection is used here to walk the type system,
    /// never to reach private state.
    /// </summary>
    /// <returns>The concrete invalidator types, ordered by name for stable test output.</returns>
    private static IReadOnlyList<Type> InvalidatorTypes() =>
        typeof(CacheInvalidator)
            .Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICacheInvalidator).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToList();

    /// <summary>
    /// Supplies one theory row per concrete invalidator type.
    /// </summary>
    /// <returns>The concrete invalidator types as theory rows.</returns>
    public static TheoryData<Type> Invalidators() => new(InvalidatorTypes());

    /// <summary>
    /// Activates the invalidator named by the row. A type that gains a constructor dependency
    /// fails here rather than dropping out of coverage.
    /// </summary>
    /// <param name="invalidatorType">The concrete invalidator type.</param>
    /// <returns>A fresh invalidator instance.</returns>
    private static ICacheInvalidator Create(Type invalidatorType) =>
        (ICacheInvalidator)Activator.CreateInstance(invalidatorType)!;

    #region Discovery

    [Fact]
    public void Invalidators_ShouldDiscoverEveryConcreteInvalidator()
    {
        InvalidatorTypes().Count.Should().Be(ConcreteInvalidatorCount);
    }

    [Theory]
    [MemberData(nameof(Invalidators))]
    public void Invalidator_ShouldDeriveFromTheSharedBaseAndCarryOneDomainMarker(Type invalidatorType)
    {
        Type[] markers = invalidatorType
            .GetInterfaces()
            .Where(i => i != typeof(ICacheInvalidator) && typeof(ICacheInvalidator).IsAssignableFrom(i))
            .ToArray();

        invalidatorType.Should().BeDerivedFrom<CacheInvalidator>();
        markers.Should().ContainSingle($"{invalidatorType.Name} needs one marker so its token is distinct in DI");
    }

    #endregion

    #region GetEvictionToken

    [Theory]
    [MemberData(nameof(Invalidators))]
    public void GetEvictionToken_BeforeInvalidate_ShouldReturnNonCancelledToken(Type invalidatorType)
    {
        ICacheInvalidator invalidator = Create(invalidatorType);

        CancellationToken token = invalidator.GetEvictionToken();

        token.IsCancellationRequested.Should().BeFalse($"{invalidatorType.Name} must accept cache fills when fresh");
    }

    [Theory]
    [MemberData(nameof(Invalidators))]
    public void GetEvictionToken_CalledTwiceBeforeInvalidate_ShouldReturnSameToken(Type invalidatorType)
    {
        ICacheInvalidator invalidator = Create(invalidatorType);

        CancellationToken first = invalidator.GetEvictionToken();
        CancellationToken second = invalidator.GetEvictionToken();

        first.Should().Be(second, $"{invalidatorType.Name} must hand every entry the same eviction token");
    }

    #endregion

    #region Invalidate

    [Theory]
    [MemberData(nameof(Invalidators))]
    public void Invalidate_ShouldCancelThePreviousTokenAndIssueADifferentLiveOne(Type invalidatorType)
    {
        ICacheInvalidator invalidator = Create(invalidatorType);

        CancellationToken before = invalidator.GetEvictionToken();
        invalidator.Invalidate();
        CancellationToken after = invalidator.GetEvictionToken();

        before
            .IsCancellationRequested.Should()
            .BeTrue($"{invalidatorType.Name} must evict its entries on invalidation");
        after.IsCancellationRequested.Should().BeFalse($"{invalidatorType.Name} must accept new cache fills");
        after.Should().NotBe(before, $"{invalidatorType.Name} must issue a fresh token");
    }

    [Theory]
    [MemberData(nameof(Invalidators))]
    public void Invalidate_CalledTwice_ShouldCancelBothPreviousTokensAndIssueNewLiveToken(Type invalidatorType)
    {
        ICacheInvalidator invalidator = Create(invalidatorType);

        CancellationToken first = invalidator.GetEvictionToken();
        invalidator.Invalidate();

        CancellationToken second = invalidator.GetEvictionToken();
        invalidator.Invalidate();

        CancellationToken third = invalidator.GetEvictionToken();

        first.IsCancellationRequested.Should().BeTrue(invalidatorType.Name);
        second.IsCancellationRequested.Should().BeTrue(invalidatorType.Name);
        third.IsCancellationRequested.Should().BeFalse(invalidatorType.Name);
    }

    [Theory]
    [MemberData(nameof(Invalidators))]
    public void Invalidate_WithoutAnyPriorTokenRead_ShouldStillIssueALiveToken(Type invalidatorType)
    {
        ICacheInvalidator invalidator = Create(invalidatorType);

        invalidator.Invalidate();

        invalidator
            .GetEvictionToken()
            .IsCancellationRequested.Should()
            .BeFalse($"{invalidatorType.Name} must recover when invalidated before its first cache fill");
    }

    #endregion
}
