using _116.Content.Domain.Enums;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.StateMachines;

/// <summary>
/// Unit tests for <see cref="ContentPublicationState" /> — the transition table, the guard, and
/// the editability rule.
/// </summary>
public class ContentPublicationStateTests
{
    [Theory]
    [InlineData(EnumContentStatus.Draft, EnumContentStatus.PendingPayment)]
    [InlineData(EnumContentStatus.Draft, EnumContentStatus.PendingReview)]
    [InlineData(EnumContentStatus.PendingPayment, EnumContentStatus.PendingReview)]
    [InlineData(EnumContentStatus.PendingPayment, EnumContentStatus.Rejected)]
    [InlineData(EnumContentStatus.PendingReview, EnumContentStatus.Approved)]
    [InlineData(EnumContentStatus.PendingReview, EnumContentStatus.Rejected)]
    [InlineData(EnumContentStatus.Approved, EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.Approved, EnumContentStatus.Rejected)]
    [InlineData(EnumContentStatus.Approved, EnumContentStatus.Archived)]
    [InlineData(EnumContentStatus.Published, EnumContentStatus.Archived)]
    [InlineData(EnumContentStatus.Published, EnumContentStatus.Rejected)]
    [InlineData(EnumContentStatus.Rejected, EnumContentStatus.PendingReview)]
    [InlineData(EnumContentStatus.Rejected, EnumContentStatus.Archived)]
    [InlineData(EnumContentStatus.Archived, EnumContentStatus.PendingReview)]
    public void CanMove_ForEveryLegalPair_ShouldReturnTrue(EnumContentStatus from, EnumContentStatus to)
    {
        // Act & Assert
        ContentPublicationState.CanMove(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumContentStatus.Draft, EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.Draft, EnumContentStatus.Approved)]
    [InlineData(EnumContentStatus.Draft, EnumContentStatus.Rejected)]
    [InlineData(EnumContentStatus.Draft, EnumContentStatus.Archived)]
    [InlineData(EnumContentStatus.PendingPayment, EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.PendingPayment, EnumContentStatus.Approved)]
    [InlineData(EnumContentStatus.PendingPayment, EnumContentStatus.Archived)]
    [InlineData(EnumContentStatus.PendingReview, EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.PendingReview, EnumContentStatus.Archived)]
    [InlineData(EnumContentStatus.Published, EnumContentStatus.PendingPayment)]
    [InlineData(EnumContentStatus.Published, EnumContentStatus.PendingReview)]
    [InlineData(EnumContentStatus.Archived, EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.Archived, EnumContentStatus.Approved)]
    [InlineData(EnumContentStatus.Rejected, EnumContentStatus.Published)]
    public void CanMove_ForAnIllegalPair_ShouldReturnFalse(EnumContentStatus from, EnumContentStatus to)
    {
        // Act & Assert
        ContentPublicationState.CanMove(from, to).Should().BeFalse();
    }

    [Theory]
    [InlineData(EnumContentStatus.Draft)]
    [InlineData(EnumContentStatus.PendingPayment)]
    [InlineData(EnumContentStatus.PendingReview)]
    [InlineData(EnumContentStatus.Approved)]
    [InlineData(EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.Rejected)]
    [InlineData(EnumContentStatus.Archived)]
    public void CanMove_ToTheSameState_ShouldReturnFalse(EnumContentStatus state)
    {
        // Act & Assert — idempotency is the entity's early return, never a table row
        ContentPublicationState.CanMove(state, state).Should().BeFalse();
    }

    [Fact]
    public void EnsureCanMove_ForALegalPair_ShouldNotThrow()
    {
        // Act
        var act = () =>
            ContentPublicationState.EnsureCanMove(
                EnumContentStatus.Approved,
                EnumContentStatus.Published,
                EnumCoreContentType.Article
            );

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanMove_ForAnIllegalPair_ShouldThrowWithTheCodeAndContext()
    {
        // Act
        var act = () =>
            ContentPublicationState.EnsureCanMove(
                EnumContentStatus.Draft,
                EnumContentStatus.Published,
                EnumCoreContentType.Lyrics
            );

        // Assert
        DomainRuleException exception = act.Should().Throw<DomainRuleException>().Which;
        exception.Code.Should().Be(ContentRuleCodes.InvalidStatusTransition);
        exception.Args.Should().Equal("Lyrics", "Draft", "Published");
    }

    [Theory]
    [InlineData(EnumContentStatus.Draft)]
    [InlineData(EnumContentStatus.PendingPayment)]
    [InlineData(EnumContentStatus.PendingReview)]
    [InlineData(EnumContentStatus.Rejected)]
    public void EnsureEditable_WhileStillEditable_ShouldNotThrow(EnumContentStatus status)
    {
        // Act
        var act = () => ContentPublicationState.EnsureEditable(status, EnumCoreContentType.Article);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(EnumContentStatus.Approved)]
    [InlineData(EnumContentStatus.Published)]
    [InlineData(EnumContentStatus.Archived)]
    public void EnsureEditable_PastReview_ShouldThrowWithTheCodeAndContext(EnumContentStatus status)
    {
        // Act
        var act = () => ContentPublicationState.EnsureEditable(status, EnumCoreContentType.Video);

        // Assert
        DomainRuleException exception = act.Should().Throw<DomainRuleException>().Which;
        exception.Code.Should().Be(ContentRuleCodes.NotEditable);
        exception.Args.Should().Equal("Video", status.ToString());
    }
}
