using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Unit tests for <see cref="AdminCreateArtistService"/>: the slug gate and the staged artist.
/// </summary>
public class AdminCreateArtistServiceTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock = MockArtistRepository.Create();
    private readonly AdminCreateArtistService _service;

    public AdminCreateArtistServiceTests()
    {
        _service = new AdminCreateArtistService(
            _artistRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n(),
            TimeProvider.System
        );
    }

    private static AdminCreateArtistCommand Command() => new("Name", "artist-slug", null, null, null, null, null);

    [Fact]
    public async Task CreateAsync_WithAFreeSlug_ShouldStageTheArtist()
    {
        // Arrange
        _artistRepositoryMock.SetupGetBySlug("artist-slug", null);

        // Act
        ArtistEntity artist = await _service.CreateAsync(Command(), CancellationToken.None);

        // Assert
        artist.Slug.Value.Should().Be("artist-slug");
        _artistRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CreateAsync_WhenTheSlugIsTaken_ShouldThrowConflictException()
    {
        // Arrange
        _artistRepositoryMock.SetupGetBySlug("artist-slug", ArtistFactory.Create());

        // Act
        Func<Task> act = async () => await _service.CreateAsync(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _artistRepositoryMock.VerifyAddNotCalled();
    }
}
