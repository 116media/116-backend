using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;
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

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Unit tests for <see cref="AdminAlbumLinkResolutionService"/>: the target gate, the provider call
/// and the empty-answer refusal.
/// </summary>
public class AdminAlbumLinkResolutionServiceTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock = MockAlbumRepository.Create();
    private readonly Mock<IStreamingLinkResolutionService> _resolutionServiceMock = new();
    private readonly AdminAlbumLinkResolutionService _service;

    public AdminAlbumLinkResolutionServiceTests()
    {
        _service = new AdminAlbumLinkResolutionService(
            _albumRepositoryMock.Object,
            _resolutionServiceMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task ResolveAsync_ShouldReturnWhatTheProviderResolved()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrow(AlbumFactory.Create());
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
            albumId,
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
        Guid albumId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrow(AlbumFactory.Create());
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumStreamingPlatform, string>());

        // Act
        Func<Task> act = async () => await _service.ResolveAsync(albumId, "https://source", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ResolveAsync_WhenTheAlbumDoesNotExist_ShouldThrowNotFoundExceptionWithoutCallingTheProvider()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrowNotFound(albumId);

        // Act
        Func<Task> act = async () => await _service.ResolveAsync(albumId, "https://source", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _resolutionServiceMock.Verify(
            x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
