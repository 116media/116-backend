using _116.Content.Application.Interactions.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Interactions.Services;

/// <summary>
/// Unit tests for <see cref="ArticleCommentDtoService"/>: the author profile and avatar resolution.
/// </summary>
public class ArticleCommentDtoServiceTests
{
    private readonly Mock<IUserLookupService> _userLookupMock = MockUserLookupService.Create();
    private readonly Mock<IFileStorageService> _fileStorageMock = MockFileStorageService.Create();
    private readonly ArticleCommentDtoService _service;

    public ArticleCommentDtoServiceTests()
    {
        _service = new ArticleCommentDtoService(_userLookupMock.Object, _fileStorageMock.Object);
    }

    [Fact]
    public async Task CreatePublicAsync_WithAKnownAuthor_ShouldCarryTheNameAndAvatarUrl()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        FileReferenceDto avatar = FileReferenceDtoFactory.CreateJpeg();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), userId);
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto("author", null, avatar.Id, null, "en"));
        _fileStorageMock.SetupResolve(avatar);

        // Act
        PublicArticleCommentDto dto = await _service.CreatePublicAsync(comment, CancellationToken.None);

        // Assert
        dto.Author.Should().NotBeNull();
        dto.Author!.UserName.Should().Be("author");
        dto.Author.AvatarUrl.Should().Be(avatar.StorageUrl);
    }

    [Fact]
    public async Task CreatePublicAsync_WithAnUnknownAuthor_ShouldLeaveTheAuthorEmpty()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), userId);
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        PublicArticleCommentDto dto = await _service.CreatePublicAsync(comment, CancellationToken.None);

        // Assert
        dto.Author.Should().BeNull();
        _fileStorageMock.Verify(x => x.ResolveAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
