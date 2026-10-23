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

    private static AdminCreateArtistCommand Command() => new("Name", null, null, null, null, null);
}
