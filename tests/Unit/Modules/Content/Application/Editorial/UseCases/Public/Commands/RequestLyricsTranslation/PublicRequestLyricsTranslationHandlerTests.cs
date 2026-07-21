using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation;

/// <summary>
/// Unit tests for <see cref="PublicRequestLyricsTranslationHandler"/>.
/// </summary>
public class PublicRequestLyricsTranslationHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly Mock<ITranslationService> _translationServiceMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRequestLyricsTranslationHandler _handler;

    public PublicRequestLyricsTranslationHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _translationRepositoryMock = MockTranslationRepository.Create();
        _translationServiceMock = MockTranslationService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRequestLyricsTranslationHandler(
            _lyricsRepositoryMock.Object,
            _translationRepositoryMock.Object,
            _translationServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenTranslationAlreadyExists_ShouldReturnExistingWithoutCallingTranslationService()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();
        const string language = "es";
        LyricsTranslationEntity existing = LyricsTranslationFactory.CreateWithText(lyricsId, language, "Ya traducido.");
        _translationRepositoryMock.SetupGetByLyricsAndLanguage(lyricsId, language, existing);
        var command = new PublicRequestLyricsTranslationCommand(lyricsId, language);

        // Act
        PublicRequestLyricsTranslationResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Text.Should().Be("Ya traducido.");
        result.Source.Should().Be(EnumTranslationSource.Ai.ToString());
        _translationServiceMock.VerifyTranslateNotCalled();
        _translationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsTranslationEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNoExistingTranslation_ShouldCallTranslationServiceAndCreateAiTranslation()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.Create(categoryId);
        const string language = "es";
        _translationRepositoryMock.SetupGetByLyricsAndLanguage(lyrics.Id, language, null);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _translationServiceMock.SetupTranslate("Texto traducido por IA.");
        var command = new PublicRequestLyricsTranslationCommand(lyrics.Id, language);

        // Act
        PublicRequestLyricsTranslationResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Text.Should().Be("Texto traducido por IA.");
        result.Source.Should().Be(EnumTranslationSource.Ai.ToString());
        _translationServiceMock.VerifyTranslateCalledOnce();
        _translationRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
