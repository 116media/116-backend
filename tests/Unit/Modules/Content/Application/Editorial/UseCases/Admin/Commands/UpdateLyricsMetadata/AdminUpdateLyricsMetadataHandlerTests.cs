using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsMetadataHandler"/>.
/// </summary>
public class AdminUpdateLyricsMetadataHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateLyricsMetadataHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateLyricsMetadataHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IUserLookupService> userLookupMock = MockUserLookupService.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUpdateLyricsMetadataHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            userLookupMock.Object,
            fileRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsExists_ShouldUpdateMetadataAndReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: lyrics.Id,
            Album: "Testament",
            ReleaseYear: 1995,
            Label: "Sonodisc",
            Songwriter: "Papa Wemba",
            Producer: "Viviane Arnoux"
        );

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminUpdateLyricsMetadataResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        result.Lyrics.Album.Should().Be("Testament");
        result.Lyrics.ReleaseYear.Should().Be(1995);
        result.Lyrics.Label.Should().Be("Sonodisc");
        result.Lyrics.Songwriter.Should().Be("Papa Wemba");
        result.Lyrics.Producer.Should().Be("Viviane Arnoux");
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithAllFieldsNull_ShouldClearMetadata()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", "Producer");
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: lyrics.Id,
            Album: null,
            ReleaseYear: null,
            Label: null,
            Songwriter: null,
            Producer: null
        );

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminUpdateLyricsMetadataResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Lyrics.Album.Should().BeNull();
        result.Lyrics.ReleaseYear.Should().BeNull();
        result.Lyrics.Label.Should().BeNull();
        result.Lyrics.Songwriter.Should().BeNull();
        result.Lyrics.Producer.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: nonExistentId,
            Album: null,
            ReleaseYear: null,
            Label: null,
            Songwriter: null,
            Producer: null
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
