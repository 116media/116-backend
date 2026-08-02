using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminCreateLyricsHandler"/>.
/// </summary>
public class AdminCreateLyricsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    public AdminCreateLyricsHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IUserLookupService> userLookupMock = MockUserLookupService.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        fileRepositoryMock.SetupGetById(coverFile);
        _handler = new AdminCreateLyricsHandler(
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

    private static AdminCreateLyricsCommand BuildCommand(
        Guid categoryId,
        string slug,
        Guid? videoId = null,
        Guid? customerId = null,
        Guid? orderItemId = null
    ) =>
        new(
            CategoryId: categoryId,
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            Slug: slug,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: AuthorId,
            VideoId: videoId,
            CustomerId: customerId,
            OrderItemId: orderItemId
        );

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidFreeLyrics_ShouldCreateAndReturnLyrics()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        var command = BuildCommand(category.Id, slug);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(slug, null);

        LyricsEntity created = LyricsFactory.Create(category.Id);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();

        _lyricsRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenValidPaidLyrics_ShouldSetCustomerAndOrderItem()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        var command = BuildCommand(category.Id, slug, customerId: customerId, orderItemId: orderItemId);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(slug, null);

        LyricsEntity created = LyricsFactory.CreatePaid(category.Id, customerId, orderItemId);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();

        _lyricsRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsForVideo_ShouldCreateAndMarkVideoHasLyrics()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        Guid videoId = Guid.NewGuid();

        var command = BuildCommand(category.Id, slug, videoId: videoId);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(slug, null);

        VideoEntity video = VideoFactory.Create(category.Id);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        LyricsEntity created = LyricsFactory.CreateForVideo(category.Id, videoId);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        video.HasLyrics.Should().BeTrue();

        _lyricsRepositoryMock.VerifyAddCalled();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldReloadLyricsAfterCreation()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        var command = BuildCommand(category.Id, slug);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(slug, null);

        LyricsEntity reloaded = LyricsFactory.Create(category.Id);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloaded);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = BuildCommand(nonExistentId, TestConstants.Content.Editorial.Lyrics.ValidSlug);

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        var command = BuildCommand(category.Id, slug);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        LyricsEntity existing = LyricsFactory.CreateWithSlug(category.Id, slug);
        _lyricsRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;
        var command = BuildCommand(category.Id, slug);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        LyricsEntity existing = LyricsFactory.CreateWithSlug(category.Id, slug);
        _lyricsRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _lyricsRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
