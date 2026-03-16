using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
        _handler = new AdminUpdateLyricsSeoHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenLyricsExists_ShouldUpdateSeoAndReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        var command = new AdminUpdateLyricsSeoCommand(
            Id: lyrics.Id.ToString(),
            MetaTitle: "Updated SEO Title",
            MetaDescription: "Updated SEO Description",
            MetaKeywords: null,
            StructuredData: null
        );

        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        AdminUpdateLyricsSeoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyUpdateCalled();
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
            MetaKeywords: null,
            StructuredData: null
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
