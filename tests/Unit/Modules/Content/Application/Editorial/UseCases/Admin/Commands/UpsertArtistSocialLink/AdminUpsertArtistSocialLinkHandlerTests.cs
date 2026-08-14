using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;

/// <summary>
/// Unit tests for <see cref="AdminUpsertArtistSocialLinkHandler"/>.
/// </summary>
public class AdminUpsertArtistSocialLinkHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpsertArtistSocialLinkHandler _handler;

    public AdminUpsertArtistSocialLinkHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpsertArtistSocialLinkHandler(_artistRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoExistingLink_ShouldCreateNewLink()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminUpsertArtistSocialLinkCommand(
            artist.Id,
            EnumSocialPlatform.Instagram,
            "https://instagram.com/fallyipupa01"
        );

        // Act
        AdminUpsertArtistSocialLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SocialLinkId.Should().NotBeEmpty();
        _artistRepositoryMock.Verify(
            x => x.AddSocialLinkAsync(It.IsAny<ArtistSocialLinkEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLinkExistsForPlatform_ShouldReplaceUrlInstead()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        ArtistSocialLinkEntity existing = ArtistSocialLinkEntity.Create(
            Guid.NewGuid(),
            artist.Id,
            EnumSocialPlatform.Instagram,
            "https://instagram.com/old"
        );
        _artistRepositoryMock.SetupGetSocialLink(artist.Id, EnumSocialPlatform.Instagram, existing);

        var command = new AdminUpsertArtistSocialLinkCommand(
            artist.Id,
            EnumSocialPlatform.Instagram,
            "https://instagram.com/new"
        );

        // Act
        AdminUpsertArtistSocialLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert — same row, new URL; nothing added.
        result.SocialLinkId.Should().Be(existing.Id);
        existing.Url.Should().Be("https://instagram.com/new");
        _artistRepositoryMock.Verify(
            x => x.AddSocialLinkAsync(It.IsAny<ArtistSocialLinkEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _artistRepositoryMock.Verify(x => x.UpdateSocialLink(existing), Times.Once);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var missingArtistId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(missingArtistId);
        var command = new AdminUpsertArtistSocialLinkCommand(
            missingArtistId,
            EnumSocialPlatform.X,
            "https://x.com/someone"
        );

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
