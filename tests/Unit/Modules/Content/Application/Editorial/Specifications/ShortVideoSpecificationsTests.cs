using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for short video specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class ShortVideoSpecificationsTests
{
    [Fact]
    public void ShortVideoByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ShortVideoByIdSpecification(shortVideo.Id);

        // Act & Assert
        spec.IsSatisfiedBy(shortVideo).Should().BeTrue();
    }

    [Fact]
    public void ShortVideoByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ShortVideoByIdSpecification(Guid.NewGuid());

        // Act & Assert
        spec.IsSatisfiedBy(shortVideo).Should().BeFalse();
    }

    #region ShortVideoByIdSpecification

    #endregion

    #region ShortVideoSearchSpecification

    [Theory]
    [InlineData("fally", true)]
    [InlineData("FALLY IPUPA", true)]
    [InlineData("teaser", true)]
    [InlineData("koffi", false)]
    public void ShortVideoSearchSpecification_ShouldMatchTitleSubstringCaseInsensitively(string search, bool expected)
    {
        // Arrange
        ShortVideoEntity shortVideo = new ShortVideoBuilder().WithTitle("Teaser Fally Ipupa Focus").Build();
        var spec = new ShortVideoSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(shortVideo);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ShortVideoBySlugSpecification

    [Fact]
    public void ShortVideoBySlugSpecification_WithMatchingSlug_ShouldReturnTrue()
    {
        // Arrange
        const string slug = "teaser-fally-focus";
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithSlug(slug);
        var spec = new ShortVideoBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoBySlugSpecification_WithDifferentSlug_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ShortVideoBySlugSpecification("different-slug");

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActiveShortVideoSpecification

    [Fact]
    public void ActiveShortVideoSpecification_WithActiveShortVideo_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ActiveShortVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveShortVideoSpecification_WithInactiveShortVideo_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateInactive();
        var spec = new ActiveShortVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ShortVideoCountedViewSinceSpecification

    [Fact]
    public void ShortVideoCountedViewSinceSpecification_WithCountedViewInWindow_ShouldReturnTrue()
    {
        // Arrange
        Guid shortVideoId = Guid.NewGuid();
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(shortVideoId, "dedup-key", isCounted: true);
        var spec = new ShortVideoCountedViewSinceSpecification(
            shortVideoId: shortVideoId,
            dedupKey: "dedup-key",
            since: DateTime.UtcNow.AddMinutes(-5)
        );

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoCountedViewSinceSpecification_WithUncountedView_ShouldReturnFalse()
    {
        // Arrange
        Guid shortVideoId = Guid.NewGuid();
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(shortVideoId, "dedup-key", isCounted: false);
        var spec = new ShortVideoCountedViewSinceSpecification(
            shortVideoId: shortVideoId,
            dedupKey: "dedup-key",
            since: DateTime.UtcNow.AddMinutes(-5)
        );

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShortVideoCountedViewSinceSpecification_WithViewOlderThanWindow_ShouldReturnFalse()
    {
        // Arrange
        Guid shortVideoId = Guid.NewGuid();
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(shortVideoId, "dedup-key", isCounted: true);
        var spec = new ShortVideoCountedViewSinceSpecification(
            shortVideoId: shortVideoId,
            dedupKey: "dedup-key",
            since: DateTime.UtcNow.AddMinutes(5)
        );

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShortVideoCountedViewSinceSpecification_WithDifferentDedupKey_ShouldReturnFalse()
    {
        // Arrange
        Guid shortVideoId = Guid.NewGuid();
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(shortVideoId, "dedup-key", isCounted: true);
        var spec = new ShortVideoCountedViewSinceSpecification(
            shortVideoId: shortVideoId,
            dedupKey: "other-key",
            since: DateTime.UtcNow.AddMinutes(-5)
        );

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UncountedShortVideoViewBeforeSpecification

    [Fact]
    public void UncountedShortVideoViewBeforeSpecification_WithUncountedViewBeforeCutoff_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(Guid.NewGuid(), "dedup-key", isCounted: false);
        var spec = new UncountedShortVideoViewBeforeSpecification(cutoff: DateTime.UtcNow.AddMinutes(5));

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UncountedShortVideoViewBeforeSpecification_WithCountedView_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(Guid.NewGuid(), "dedup-key", isCounted: true);
        var spec = new UncountedShortVideoViewBeforeSpecification(cutoff: DateTime.UtcNow.AddMinutes(5));

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UncountedShortVideoViewBeforeSpecification_WithViewAfterCutoff_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoViewEventEntity viewEvent = CreateViewEvent(Guid.NewGuid(), "dedup-key", isCounted: false);
        var spec = new UncountedShortVideoViewBeforeSpecification(cutoff: DateTime.UtcNow.AddMinutes(-5));

        // Act
        bool result = spec.IsSatisfiedBy(viewEvent);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    private static ShortVideoViewEventEntity CreateViewEvent(Guid shortVideoId, string dedupKey, bool isCounted) =>
        ShortVideoViewEventEntity.Create(
            id: Guid.NewGuid(),
            shortVideoId: shortVideoId,
            userId: null,
            dedupKey: dedupKey,
            ipAddress: null,
            userAgent: null,
            isCounted: isCounted
        );
}
