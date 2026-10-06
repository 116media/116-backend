using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Shared.Domain.Constants;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Unit tests for <see cref="AdminGetArticleByIdHandler"/>.
/// </summary>
public class AdminGetArticleByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetArticleByIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetArticleByIdHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetArticleByIdHandler(
            _articleRepositoryMock.Object,
            CreateArticleDtoService(_fileStorageMock.Object, _userLookupMock.Object)
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExists_ShouldReturnArticleDetail()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task Handle_WhenAuthorExists_ShouldResolveAuthorProfile()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "SuperAdmin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Author.Should().NotBeNull();
        result.Article.Author!.UserName.Should().Be(TestConstants.User.ValidUserName);
        result.Article.Author.Email.Should().Be(TestConstants.User.ValidEmail);
        result.Article.Author.Role.Should().Be("SuperAdmin");
        result.Article.Author.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenAuthorHasAvatar_ShouldResolveAvatarUrl()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        Guid avatarFileId = Guid.NewGuid();
        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            avatarFileId,
            "Admin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        FileReferenceDto avatarFile = FileReferenceDtoFactory.CreateWithId(avatarFileId);
        _fileStorageMock.SetupResolve(avatarFile);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Author.Should().NotBeNull();
        result.Article.Author!.AvatarUrl.Should().Be(avatarFile.StorageUrl);
        result.Article.Author.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_WhenAuthorNotFound_ShouldReturnNullAuthor()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Author.Should().BeNull();
        _fileStorageMock.Verify(x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAuthorHasNoAvatar_ShouldNotCallFileRepository()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "Admin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Author.Should().NotBeNull();
        result.Article.Author!.AvatarUrl.Should().BeNull();
        _fileStorageMock.Verify(x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var query = new AdminGetArticleByIdQuery(Id: nonExistentId);
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
