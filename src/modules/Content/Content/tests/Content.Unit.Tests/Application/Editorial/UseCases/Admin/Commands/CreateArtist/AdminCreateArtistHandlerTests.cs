using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Unit tests for <see cref="AdminCreateArtistHandler"/>.
/// </summary>
public class AdminCreateArtistHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateArtistHandler _handler;

    public AdminCreateArtistHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminCreateArtistHandler(
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new ArtistDtoFactory(fileStorageMock.Object),
            TestErrorsFactory.CreateContentI18n(),
            TimeProvider.System
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreateAndReturnArtist()
    {
        // Arrange
        var command = new AdminCreateArtistCommand(
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            TestConstants.Artist.ValidBio,
            null,
            null,
            null,
            null
        );
        _artistRepositoryMock.SetupGetBySlug(command.Slug, null);

        // Act
        AdminCreateArtistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Artist.Name.Should().Be(command.Name);
        result.Artist.Slug.Should().Be(command.Slug);
        result.Artist.Bio.Should().Be(command.Bio);

        _artistRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new AdminCreateArtistCommand(
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            null,
            null,
            null,
            null
        );
        ArtistEntity existing = ArtistFactory.CreateWithSlug(command.Slug);
        _artistRepositoryMock.SetupGetBySlug(command.Slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        var command = new AdminCreateArtistCommand(
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            null,
            null,
            null,
            null
        );
        ArtistEntity existing = ArtistFactory.CreateWithSlug(command.Slug);
        _artistRepositoryMock.SetupGetBySlug(command.Slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _artistRepositoryMock.VerifyAddNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
