using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsSeoHandler"/>.
/// </summary>
public class AdminUpdateLyricsSeoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateLyricsSeoHandler _handler;

    public AdminUpdateLyricsSeoHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IUserLookupService> userLookupMock = MockUserLookupService.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUpdateLyricsSeoHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            userLookupMock.Object,
            fileRepositoryMock.Object
        );
    }

    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public async Task Handle_WhenLyricsExists_ShouldUpdateSeoAndReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var command = new AdminUpdateLyricsSeoCommand(
            Id: lyrics.Id.ToString(),
            MetaTitle: "Updated SEO Title",
            MetaDescription: "Updated SEO Description",
            StructuredData: null
        );

        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        AdminUpdateLyricsSeoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.MetaTitle.Should().Be("Updated SEO Title");
        lyrics.MetaDescription.Should().Be("Updated SEO Description");
        lyrics.StructuredData.Should().BeNull();
        result.Lyrics.MetaTitle.Should().Be("Updated SEO Title");
        result.Lyrics.MetaDescription.Should().Be("Updated SEO Description");
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateLyricsSeoCommand(
            Id: nonExistentId.ToString(),
            MetaTitle: null,
            MetaDescription: null,
            StructuredData: null
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
