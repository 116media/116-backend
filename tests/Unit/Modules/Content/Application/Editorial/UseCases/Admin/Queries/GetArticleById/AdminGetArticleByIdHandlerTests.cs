using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Unit tests for <see cref="AdminGetArticleByIdHandler"/>.
/// </summary>
public class AdminGetArticleByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetArticleByIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetArticleByIdHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetArticleByIdHandler(
            _articleRepositoryMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper
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

        var authorInfo = new AuthorInfo(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "SuperAdmin"
        );
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
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
        var authorInfo = new AuthorInfo(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            avatarFileId,
            "Admin"
        );
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        FileEntity avatarFile = FileFactory.CreateWithId(avatarFileId);
        _fileRepositoryMock.SetupGetById(avatarFile);

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
            .Setup(x => x.GetAuthorInfoByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorInfo?)null);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Author.Should().BeNull();
        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAuthorHasNoAvatar_ShouldNotCallFileRepository()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        var authorInfo = new AuthorInfo(TestConstants.User.ValidUserName, TestConstants.User.ValidEmail, null, "Admin");
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(article.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Author.Should().NotBeNull();
        result.Article.Author!.AvatarUrl.Should().BeNull();
        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
