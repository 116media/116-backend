using _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;

/// <summary>
/// Unit tests for <see cref="AdminRemoveArtistSocialLinkHandler"/>.
/// </summary>
public class AdminRemoveArtistSocialLinkHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveArtistSocialLinkHandler _handler;

    public AdminRemoveArtistSocialLinkHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemoveArtistSocialLinkHandler(
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenLinkExists_ShouldRemoveIt()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        ArtistSocialLinkEntity existing = ArtistSocialLinkEntity.Create(
            Guid.NewGuid(),
            artistId,
            EnumSocialPlatform.TikTok,
            "https://tiktok.com/@someone"
        );
        _artistRepositoryMock.SetupGetSocialLink(artistId, EnumSocialPlatform.TikTok, existing);

        var command = new AdminRemoveArtistSocialLinkCommand(artistId, EnumSocialPlatform.TikTok);

        // Act
        AdminRemoveArtistSocialLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _artistRepositoryMock.Verify(x => x.RemoveSocialLink(existing), Times.Once);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoLinkForPlatform_ShouldThrowNotFound()
    {
        // Arrange — the admin asked to delete something specific and must learn it was not there.
        var command = new AdminRemoveArtistSocialLinkCommand(Guid.NewGuid(), EnumSocialPlatform.Website);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _artistRepositoryMock.Verify(x => x.RemoveSocialLink(It.IsAny<ArtistSocialLinkEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
