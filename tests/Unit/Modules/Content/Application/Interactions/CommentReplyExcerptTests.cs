using _116.Content.Application.Interactions.EventHandlers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions;

/// <summary>
/// Unit tests for the comment-reply excerpt truncation embedded in the
/// notification email.
/// </summary>
public class CommentReplyExcerptTests
{
    [Fact]
    public void Excerpt_ShortBody_ShouldPassThroughUnchanged()
    {
        CommentReplyAddedNotificationsHandler.Excerpt("Totally agree!").Should().Be("Totally agree!");
    }

    [Fact]
    public void Excerpt_ExactlyAtTheLimit_ShouldPassThroughUnchanged()
    {
        string body = new('a', 140);

        CommentReplyAddedNotificationsHandler.Excerpt(body).Should().Be(body);
    }

    [Fact]
    public void Excerpt_LongBody_ShouldCutAtAWordBoundaryWithEllipsis()
    {
        string body = string.Join(' ', Enumerable.Repeat("word", 60));

        string excerpt = CommentReplyAddedNotificationsHandler.Excerpt(body);

        excerpt.Length.Should().BeLessThanOrEqualTo(141);
        excerpt.Should().EndWith("…");
        excerpt.TrimEnd('…').Should().NotEndWith(" ").And.NotContain("wor…");
    }

    [Fact]
    public void Excerpt_LongBodyWithoutSpaces_ShouldHardCutAtTheLimitWithEllipsis()
    {
        string body = new('a', 200);

        string excerpt = CommentReplyAddedNotificationsHandler.Excerpt(body);

        excerpt.Should().Be($"{new string('a', 140)}…");
    }
}
