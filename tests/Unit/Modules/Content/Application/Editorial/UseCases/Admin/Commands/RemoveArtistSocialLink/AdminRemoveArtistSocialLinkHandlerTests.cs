using _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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
        ArtistEntity artist = ArtistFactory.Create();
        artist.SetSocialLink(EnumSocialPlatform.TikTok, "https://tiktok.com/@someone");
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        var command = new AdminRemoveArtistSocialLinkCommand(artist.Id, EnumSocialPlatform.TikTok);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        artist.SocialLinks.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoLinkForPlatform_ShouldThrowNotFound()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminRemoveArtistSocialLinkCommand(artist.Id, EnumSocialPlatform.Website);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        artist.SocialLinks.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
