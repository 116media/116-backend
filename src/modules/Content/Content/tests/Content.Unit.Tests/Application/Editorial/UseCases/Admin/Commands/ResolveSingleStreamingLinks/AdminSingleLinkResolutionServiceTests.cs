using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;
using _116.Content.Application.Shared.Ports;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;

/// <summary>
/// Unit tests for <see cref="AdminSingleLinkResolutionService"/>: the target gate, the provider call
/// and the empty-answer refusal.
/// </summary>
public class AdminSingleLinkResolutionServiceTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock = MockLyricsRepository.Create();
    private readonly Mock<IStreamingLinkResolutionService> _resolutionServiceMock = new();
    private readonly AdminSingleLinkResolutionService _service;

    public AdminSingleLinkResolutionServiceTests()
    {
        _service = new AdminSingleLinkResolutionService(
            _lyricsRepositoryMock.Object,
            _resolutionServiceMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task ResolveAsync_ShouldReturnWhatTheProviderResolved()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        Guid lyricsId = lyrics.Id;
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        EnumStreamingPlatform platform = Enum.GetValues<EnumStreamingPlatform>()[0];
        IReadOnlyDictionary<EnumStreamingPlatform, string> resolved = new Dictionary<EnumStreamingPlatform, string>
        {
            [platform] = "https://platform/track",
        };
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync("https://source", It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolved);

        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await _service.ResolveAsync(
            lyricsId,
            "https://source",
            CancellationToken.None
        );

        // Assert
        result.Should().BeSameAs(resolved);
    }

    [Fact]
    public async Task ResolveAsync_WhenNothingResolved_ShouldThrowNotFoundException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        Guid lyricsId = lyrics.Id;
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumStreamingPlatform, string>());

        // Act
        Func<Task> act = async () => await _service.ResolveAsync(lyricsId, "https://source", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ResolveAsync_WhenTheLyricsBelongToAnAlbum_ShouldThrowConflictExceptionWithoutCallingTheProvider()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForAlbum(Guid.NewGuid(), Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _service.ResolveAsync(lyrics.Id, "https://source", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _resolutionServiceMock.Verify(
            x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
