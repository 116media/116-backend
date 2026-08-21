using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ArtistOwnershipVerifiedNotificationsHandler"/>.
/// </summary>
public class ArtistOwnershipVerifiedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly ArtistOwnershipVerifiedNotificationsHandler _handler;
    private readonly ArtistEntity _artist;

    public ArtistOwnershipVerifiedNotificationsHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _artist = ArtistFactory.Create();
        _artistRepositoryMock
            .Setup(x => x.GetByIdAsync(_artist.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_artist);

        _handler = new ArtistOwnershipVerifiedNotificationsHandler(
            _userLookupServiceMock.Object,
            _artistRepositoryMock.Object,
            _mailerMock.Object,
            _notifierMock.Object,
            NullLogger<ArtistOwnershipVerifiedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_ShouldEnqueueTheArtistVerifiedEmail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        SetupUser(ownerId, "owner@test.com");

        // Act
        await _handler.Handle(new ArtistOwnershipVerifiedEvent(_artist.Id, ownerId), CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.ArtistVerified,
                    It.Is<EmailRecipient>(r => r.Address == "owner@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["userName"] == "Fally" && t["artistName"] == _artist.Name
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldWriteTheArtistVerifiedNotificationLinkedToTheProfile()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        SetupUser(ownerId, "owner@test.com");

        // Act
        await _handler.Handle(new ArtistOwnershipVerifiedEvent(_artist.Id, ownerId), CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    ownerId,
                    EnumNotificationType.ArtistVerified,
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["artistName"] == _artist.Name && t["linkPath"] == $"/artists/{_artist.Slug}"
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenOwnerHasNoEmail_ShouldSkipTheEmailButStillNotify()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        SetupUser(ownerId, email: null);

        // Act
        await _handler.Handle(new ArtistOwnershipVerifiedEvent(_artist.Id, ownerId), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    ownerId,
                    EnumNotificationType.ArtistVerified,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenOwnerNotFound_ShouldSkipBothChannels()
    {
        // Act
        await _handler.Handle(new ArtistOwnershipVerifiedEvent(_artist.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldSkipBothChannels()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        SetupUser(ownerId, "owner@test.com");

        // Act
        await _handler.Handle(new ArtistOwnershipVerifiedEvent(Guid.NewGuid(), ownerId), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", email, null, "Visitor"));
    }
}
