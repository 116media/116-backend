using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application.Services;
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
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();
        FileReferenceDto coverFile = FileReferenceDtoFactory.CreateImage();
        fileStorageMock.SetupResolve(coverFile);
        _handler = new AdminUpdateLyricsHandler(
            _categoryRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            userLookupMock.Object,
            fileStorageMock.Object,
            TestErrorsFactory.CreateContentI18n(),
            CreateContentLookupFactory()
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
    public async Task Handle_WhenLyricsExists_ShouldApplyCommandFieldsAndReturnLyrics()
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
        lyrics.CategoryId.Should().Be(category.Id);
        lyrics.SongTitle.Should().Be(TestConstants.Lyrics.ValidSongTitle);
        lyrics.ArtistName.Should().Be(TestConstants.Lyrics.ValidArtistName);
        lyrics.Slug.Value.Should().Be(command.Slug);
        lyrics.LyricsText.Should().Be(TestConstants.Lyrics.ValidLyricsText);
        lyrics.Language.Should().Be(TestConstants.Lyrics.ValidLanguage);
        lyrics.VideoId.Should().BeNull();
        lyrics.CustomerId.Should().BeNull();
        lyrics.OrderItemId.Should().BeNull();
        result.Lyrics.Id.Should().Be(lyrics.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoLinkAdded_ShouldValidateAndLinkTheVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        Guid videoId = Guid.NewGuid();
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id, videoId: videoId);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock
            .Setup(x => x.ExistsOrThrowAsync(videoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.VideoId.Should().Be(videoId);
        _videoRepositoryMock.Verify(x => x.ExistsOrThrowAsync(videoId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoLinkChanged_ShouldRelinkWithoutLoadingEitherVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        Guid oldVideoId = Guid.NewGuid();
        Guid newVideoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, oldVideoId);
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id, videoId: newVideoId);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock
            .Setup(x => x.ExistsOrThrowAsync(newVideoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.VideoId.Should().Be(newVideoId);
        _videoRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
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
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        lyrics.CategoryId.Should().Be(CategoryId);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenSlugConflictsWithAnotherLyrics_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, "original-lyrics-slug");
        AdminUpdateLyricsCommand command = BuildCommand(lyrics, category.Id, slug: TestConstants.Lyrics.ValidSlug);
        LyricsEntity conflicting = LyricsFactory.CreateWithSlug(CategoryId, command.Slug);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(command.Slug, conflicting);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        lyrics.Slug.Value.Should().Be("original-lyrics-slug");
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
