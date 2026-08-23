using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsHandler"/>.
/// </summary>
public class AdminUpdateLyricsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateLyricsHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IUserLookupService> userLookupMock = MockUserLookupService.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        fileRepositoryMock.SetupGetById(coverFile);
        _handler = new AdminUpdateLyricsHandler(
            _categoryRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            userLookupMock.Object,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminUpdateLyricsCommand BuildCommand(
        LyricsEntity lyrics,
        Guid categoryId,
        string? slug = null,
        Guid? videoId = null
    ) =>
        new(
            Id: lyrics.Id.ToString(),
            CategoryId: categoryId,
            SongTitle: TestConstants.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Lyrics.ValidArtistName,
            Slug: slug ?? lyrics.Slug,
            LyricsText: TestConstants.Lyrics.ValidLyricsText,
            Language: TestConstants.Lyrics.ValidLanguage,
            VideoId: videoId,
            CustomerId: null,
            OrderItemId: null
        );

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsExists_ShouldUpdateAndReturnLyrics()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        AdminUpdateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoLinkAdded_ShouldMarkNewVideoHasLyrics()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        Guid videoId = Guid.NewGuid();
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id, videoId: videoId);

        VideoEntity video = VideoFactory.Create(category.Id);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.HasLyrics.Should().BeTrue();
        _videoRepositoryMock.VerifyUpdateCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoLinkChanged_ShouldUnmarkOldVideoAndMarkNewVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        Guid oldVideoId = Guid.NewGuid();
        Guid newVideoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, oldVideoId);
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id, videoId: newVideoId);

        VideoEntity oldVideo = VideoFactory.Create(category.Id);
        oldVideo.MarkHasLyrics();
        VideoEntity newVideo = VideoFactory.Create(category.Id);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(oldVideoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldVideo);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(newVideoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newVideo);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        oldVideo.HasLyrics.Should().BeFalse();
        newVideo.HasLyrics.Should().BeTrue();
        _videoRepositoryMock.Verify(x => x.Update(oldVideo), Times.Once);
        _videoRepositoryMock.Verify(x => x.Update(newVideo), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        LyricsEntity dummy = LyricsFactory.Create(CategoryId);
        AdminUpdateLyricsCommand command = BuildCommand(dummy, CategoryId) with { Id = nonExistentId.ToString() };
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        Guid nonExistentCategoryId = Guid.NewGuid();
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, nonExistentCategoryId);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentCategoryId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugConflictsWithAnotherLyrics_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        // Lyrics has a different slug so the handler's slug-change check is triggered.
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, "original-lyrics-slug");
        // Command uses ValidSlug — a different slug that already belongs to another lyrics page.
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id, slug: TestConstants.Lyrics.ValidSlug);
        LyricsEntity conflicting = LyricsFactory.CreateWithSlug(CategoryId, command.Slug);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(command.Slug, conflicting);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
