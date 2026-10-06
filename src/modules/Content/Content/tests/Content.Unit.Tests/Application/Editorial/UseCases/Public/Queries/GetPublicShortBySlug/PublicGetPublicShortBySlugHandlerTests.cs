using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetPublicShortBySlugHandler"/>.
/// </summary>
public class PublicGetPublicShortBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetPublicShortBySlugHandler _handler;

    public PublicGetPublicShortBySlugHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileStorageMock = MockFileStorageService.Create();

        FileReferenceDto videoFile = FileReferenceDtoFactory.CreateVideo();
        _fileStorageMock.SetupResolve(videoFile);

        _handler = new PublicGetPublicShortBySlugHandler(
            _shortVideoRepositoryMock.Object,
            CreateShortVideoDtoService(_fileStorageMock.Object, _userLookupMock.Object),
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenActiveShortVideoExists_ShouldReturnShortVideo()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        string slug = shortVideo.Slug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, shortVideo);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.Id.Should().Be(shortVideo.Id);
        result.ShortVideo.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.ShortVideo.ValidSlug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenShortVideoExistsButInactive_ShouldThrowNotFoundException()
    {
        // Arrange
        ShortVideoEntity inactiveShortVideo = ShortVideoFactory.CreateInactive();
        string slug = inactiveShortVideo.Slug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, inactiveShortVideo);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserLikedAndBookmarked_ShouldStampFlags()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var userId = Guid.NewGuid();
        var query = new PublicGetPublicShortBySlugQuery(Slug: shortVideo.Slug, CurrentUserId: userId);

        _shortVideoRepositoryMock.SetupGetBySlug(shortVideo.Slug, shortVideo);
        _shortVideoRepositoryMock.SetupHasLikedAsync(userId, shortVideo.Id, result: true);
        _shortVideoRepositoryMock.SetupHasBookmarkedAsync(userId, shortVideo.Id, result: true);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.IsLiked.Should().BeTrue();
        result.ShortVideo.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldNotResolveFlagsAndReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new PublicGetPublicShortBySlugQuery(Slug: shortVideo.Slug);

        _shortVideoRepositoryMock.SetupGetBySlug(shortVideo.Slug, shortVideo);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.IsLiked.Should().BeFalse();
        result.ShortVideo.IsBookmarked.Should().BeFalse();
        _shortVideoRepositoryMock.Verify(
            x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _shortVideoRepositoryMock.Verify(
            x => x.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
