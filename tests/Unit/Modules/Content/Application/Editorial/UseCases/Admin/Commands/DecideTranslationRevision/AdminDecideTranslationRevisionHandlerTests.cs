using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision;

/// <summary>
/// Unit tests for <see cref="AdminDecideTranslationRevisionHandler"/>.
/// </summary>
public class AdminDecideTranslationRevisionHandlerTests
{
    private readonly Mock<ITranslationRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDecideTranslationRevisionHandler _handler;

    public AdminDecideTranslationRevisionHandlerTests()
    {
        _revisionRepositoryMock = MockTranslationRevisionRepository.Create();
        _translationRepositoryMock = MockTranslationRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDecideTranslationRevisionHandler(
            _revisionRepositoryMock.Object,
            _translationRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Accept Cases

    [Fact]
    public async Task Handle_WhenAcceptTrue_ShouldBypassTallyAndApplyTextWithRealModerator()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
            translation.Id,
            Guid.NewGuid(),
            "Moderator-approved translation text."
        );
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideTranslationRevisionCommand(revision.Id, Accept: true, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().Be(moderatorId);
        translation.Text.Should().Be("Moderator-approved translation text.");
        translation.Source.Should().Be(EnumTranslationSource.Community);
        _revisionRepositoryMock.VerifyUpdateCalled(revision);
        _translationRepositoryMock.VerifyUpdateCalled(translation);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAcceptTrue_ShouldRaiseTranslationRevisionDecidedEvent()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
            translation.Id,
            Guid.NewGuid(),
            "Moderator-approved translation text."
        );
        revision.ClearDomainEvents();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideTranslationRevisionCommand(revision.Id, Accept: true, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision
            .DomainEvents.OfType<TranslationRevisionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new TranslationRevisionDecidedEvent(
                    RevisionId: revision.Id,
                    TranslationId: translation.Id,
                    ProposedByUserId: revision.ProposedByUserId,
                    Accepted: true,
                    ByModerator: true
                )
            );
    }

    #endregion

    #region Reject Cases

    [Fact]
    public async Task Handle_WhenAcceptFalse_ShouldBypassTallyAndRejectWithoutTouchingTranslation()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideTranslationRevisionCommand(revision.Id, Accept: false, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Rejected);
        revision.DecidedByUserId.Should().Be(moderatorId);
        _revisionRepositoryMock.VerifyUpdateCalled(revision);
        _translationRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsTranslationEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAcceptFalse_ShouldRaiseTranslationRevisionDecidedEvent()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        revision.ClearDomainEvents();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideTranslationRevisionCommand(revision.Id, Accept: false, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision
            .DomainEvents.OfType<TranslationRevisionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new TranslationRevisionDecidedEvent(
                    RevisionId: revision.Id,
                    TranslationId: revision.TranslationId,
                    ProposedByUserId: revision.ProposedByUserId,
                    Accepted: false,
                    ByModerator: true
                )
            );
    }

    #endregion
}
