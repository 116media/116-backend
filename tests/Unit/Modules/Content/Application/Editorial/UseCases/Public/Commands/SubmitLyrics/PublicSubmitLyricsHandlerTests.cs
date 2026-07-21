using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Unit tests for <see cref="PublicSubmitLyricsHandler"/>.
/// </summary>
public class PublicSubmitLyricsHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly PublicSubmitLyricsHandler _handler;

    public PublicSubmitLyricsHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _submissionRepositoryMock = MockLyricsSubmissionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicSubmitLyricsHandler(
            _artistRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _submissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Verified-Artist Fast Path

    [Fact]
    public async Task Handle_WhenSubmitterOwnsClaimedArtist_ShouldSkipQueueAndAttributeToOwnedArtistIdentity()
    {
        // Arrange — the owned artist's real name is deliberately different from the ArtistName
        // the client sends, to prove the fast path is identity-gated, never string-based.
        var userId = Guid.NewGuid();
        ArtistEntity ownedArtist = ArtistFactory.Create("Fally Ipupa", "fally-ipupa");
        _artistRepositoryMock.SetupGetByUserId(userId, ownedArtist);
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);
        _lyricsRepositoryMock.SetupGetBySlug("eloko-oyo", null);

        var command = new PublicSubmitLyricsCommand(
            SongTitle: "Eloko Oyo",
            ArtistName: "Some Impersonator Name",
            LyricsText: "Some submitted lyrics text.",
            Language: "fr",
            Slug: "eloko-oyo",
            UserId: userId
        );

        // Act
        PublicSubmitLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WentToQueue.Should().BeFalse();
        result.SubmissionId.Should().BeNull();
        result.LyricsId.Should().NotBeNull();

        _submissionRepositoryMock.VerifyAddNotCalled();

        _lyricsRepositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<LyricsEntity>(l =>
                        l.ArtistName == ownedArtist.Name
                        && l.ArtistId == ownedArtist.Id
                        && l.ArtistName != "Some Impersonator Name"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSubmitterOwnsClaimedArtistButSlugMissing_ShouldThrowSlugRequired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        ArtistEntity ownedArtist = ArtistFactory.CreateClaimed(userId);
        _artistRepositoryMock.SetupGetByUserId(userId, ownedArtist);

        var command = new PublicSubmitLyricsCommand(
            SongTitle: "Eloko Oyo",
            ArtistName: null,
            LyricsText: "Some submitted lyrics text.",
            Language: "fr",
            Slug: null,
            UserId: userId
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _lyricsRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenSubmitterOwnsClaimedArtistButSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        ArtistEntity ownedArtist = ArtistFactory.CreateClaimed(userId);
        _artistRepositoryMock.SetupGetByUserId(userId, ownedArtist);
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);
        LyricsEntity existing = LyricsFactory.CreateWithSlug(category.Id, "eloko-oyo");
        _lyricsRepositoryMock.SetupGetBySlug("eloko-oyo", existing);

        var command = new PublicSubmitLyricsCommand(
            SongTitle: "Eloko Oyo",
            ArtistName: null,
            LyricsText: "Some submitted lyrics text.",
            Language: "fr",
            Slug: "eloko-oyo",
            UserId: userId
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSubmitterOwnsClaimedArtistButNoDefaultCategoryConfigured_ShouldThrowConfigurationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        ArtistEntity ownedArtist = ArtistFactory.CreateClaimed(userId);
        _artistRepositoryMock.SetupGetByUserId(userId, ownedArtist);
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(null);

        var command = new PublicSubmitLyricsCommand(
            SongTitle: "Eloko Oyo",
            ArtistName: null,
            LyricsText: "Some submitted lyrics text.",
            Language: "fr",
            Slug: "eloko-oyo",
            UserId: userId
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
        _lyricsRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Moderation Queue Path

    [Fact]
    public async Task Handle_WhenSubmitterOwnsNoClaimedArtistAndArtistNameMissing_ShouldThrowArtistNameRequired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByUserId(userId, null);

        var command = new PublicSubmitLyricsCommand(
            SongTitle: "Eloko Oyo",
            ArtistName: null,
            LyricsText: "Some submitted lyrics text.",
            Language: "fr",
            Slug: null,
            UserId: userId
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _submissionRepositoryMock.VerifyAddNotCalled();
    }

    [Fact]
    public async Task Handle_WhenSubmitterOwnsNoClaimedArtistAndArtistNameProvided_ShouldQueueSubmission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByUserId(userId, null);

        var command = new PublicSubmitLyricsCommand(
            SongTitle: "Eloko Oyo",
            ArtistName: "Fally Ipupa",
            LyricsText: "Some submitted lyrics text.",
            Language: "fr",
            Slug: null,
            UserId: userId
        );

        // Act
        PublicSubmitLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WentToQueue.Should().BeTrue();
        result.SubmissionId.Should().NotBeNull();
        result.LyricsId.Should().BeNull();
        _submissionRepositoryMock.VerifyAddCalled();
        _lyricsRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
