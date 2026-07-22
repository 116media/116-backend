using _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;

/// <summary>
/// Unit tests for <see cref="PublicGetTranslationRevisionsHandler"/>.
/// </summary>
public class PublicGetTranslationRevisionsHandlerTests
{
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly Mock<ITranslationRevisionRepository> _revisionRepositoryMock;
    private readonly PublicGetTranslationRevisionsHandler _handler;

    public PublicGetTranslationRevisionsHandlerTests()
    {
        _translationRepositoryMock = MockTranslationRepository.Create();
        _revisionRepositoryMock = MockTranslationRevisionRepository.Create();
        _handler = new PublicGetTranslationRevisionsHandler(
            _translationRepositoryMock.Object,
            _revisionRepositoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithExistingRevisions_ShouldReturnMappedDtos()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        var proposerId = Guid.NewGuid();
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
            translation.Id,
            proposerId,
            "Proposed replacement text."
        );
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        _revisionRepositoryMock.SetupGetAllByTranslationId(translation.Id, [revision]);
        var query = new PublicGetTranslationRevisionsQuery(translation.Id);

        // Act
        PublicGetTranslationRevisionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Revisions.Should().HaveCount(1);
        result.Revisions[0].Id.Should().Be(revision.Id);
        result.Revisions[0].ProposedText.Should().Be("Proposed replacement text.");
        result.Revisions[0].ProposedByUserId.Should().Be(proposerId);
        result.Revisions[0].Status.Should().Be(EnumRevisionStatus.Pending.ToString());
        result.Revisions[0].DecidedByUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNoRevisions_ShouldReturnEmptyList()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        var query = new PublicGetTranslationRevisionsQuery(translation.Id);

        // Act
        PublicGetTranslationRevisionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Revisions.Should().BeEmpty();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenTranslationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var translationId = Guid.NewGuid();
        _translationRepositoryMock.SetupGetByIdOrThrowNotFound(translationId);
        var query = new PublicGetTranslationRevisionsQuery(translationId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
