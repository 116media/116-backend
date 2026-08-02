using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;

/// <summary>
/// Unit tests for <see cref="PublicProposeTranslationRevisionHandler"/>.
/// </summary>
public class PublicProposeTranslationRevisionHandlerTests
{
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly Mock<ITranslationRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicProposeTranslationRevisionHandler _handler;

    public PublicProposeTranslationRevisionHandlerTests()
    {
        _translationRepositoryMock = MockTranslationRepository.Create();
        _revisionRepositoryMock = MockTranslationRevisionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicProposeTranslationRevisionHandler(
            _translationRepositoryMock.Object,
            _revisionRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePendingRevision()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        var command = new PublicProposeTranslationRevisionCommand(
            translation.Id,
            "Proposed replacement text.",
            "Fixed a typo.",
            Guid.NewGuid()
        );

        // Act
        PublicProposeTranslationRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RevisionId.Should().NotBeEmpty();
        _revisionRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenTranslationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var translationId = Guid.NewGuid();
        _translationRepositoryMock.SetupGetByIdOrThrowNotFound(translationId);
        var command = new PublicProposeTranslationRevisionCommand(
            translationId,
            "Proposed replacement text.",
            null,
            Guid.NewGuid()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
