using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
        AdminDecideTranslationRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().Be(moderatorId);
        translation.Text.Should().Be("Moderator-approved translation text.");
        translation.Source.Should().Be(EnumTranslationSource.Community);
        _revisionRepositoryMock.VerifyUpdateCalled();
        _translationRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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
        AdminDecideTranslationRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        revision.Status.Should().Be(EnumRevisionStatus.Rejected);
        revision.DecidedByUserId.Should().Be(moderatorId);
        _revisionRepositoryMock.VerifyUpdateCalled();
        _translationRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsTranslationEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
