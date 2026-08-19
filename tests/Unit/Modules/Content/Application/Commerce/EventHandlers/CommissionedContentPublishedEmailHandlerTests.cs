using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="CommissionedContentPublishedEmailHandler"/>.
/// </summary>
public class CommissionedContentPublishedEmailHandlerTests
{
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly CommissionedContentPublishedEmailHandler _handler;

    public CommissionedContentPublishedEmailHandlerTests()
    {
        _handler = new CommissionedContentPublishedEmailHandler(
            _notifierMock.Object,
            NullLogger<CommissionedContentPublishedEmailHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumCoreContentType.Article, "/articles/")]
    [InlineData(EnumCoreContentType.Video, "/videos/")]
    [InlineData(EnumCoreContentType.Lyrics, "/lyrics/")]
    public async Task Handle_ShouldBuildThePublicUrlFromTheSlugPerContentType(
        EnumCoreContentType contentType,
        string expectedSegment
    )
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new CommissionedContentPublishedEvent(
            Guid.NewGuid(),
            contentType,
            customerId,
            "Some title",
            "some-slug"
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyContentPublishedAsync(
                    customerId,
                    "Some title",
                    It.Is<string>(url => url.Contains(expectedSegment + "some-slug")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithUnsupportedContentType_ShouldSkipTheEmail()
    {
        // Arrange
        var domainEvent = new CommissionedContentPublishedEvent(
            Guid.NewGuid(),
            EnumCoreContentType.Short,
            Guid.NewGuid(),
            "Some title",
            "some-slug"
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.VerifyNoOtherCalls();
    }
}
