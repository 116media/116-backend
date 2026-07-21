using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsTranslationsHandler"/>.
/// </summary>
public class PublicGetLyricsTranslationsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly PublicGetLyricsTranslationsHandler _handler;

    public PublicGetLyricsTranslationsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _translationRepositoryMock = MockTranslationRepository.Create();
        _handler = new PublicGetLyricsTranslationsHandler(
            _lyricsRepositoryMock.Object,
            _translationRepositoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithExistingTranslations_ShouldReturnMappedDtos()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        LyricsTranslationEntity translation = LyricsTranslationFactory.CreateWithText(
            lyrics.Id,
            "es",
            "Texto en espanol."
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _translationRepositoryMock.SetupGetAllByLyricsId(lyrics.Id, [translation]);
        var query = new PublicGetLyricsTranslationsQuery(lyrics.Id);

        // Act
        PublicGetLyricsTranslationsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Translations.Should().HaveCount(1);
        result.Translations[0].Id.Should().Be(translation.Id);
        result.Translations[0].Language.Should().Be("es");
        result.Translations[0].Text.Should().Be("Texto en espanol.");
        result.Translations[0].Source.Should().Be(EnumTranslationSource.Ai.ToString());
    }

    [Fact]
    public async Task Handle_WithNoTranslations_ShouldReturnEmptyList()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var query = new PublicGetLyricsTranslationsQuery(lyrics.Id);

        // Act
        PublicGetLyricsTranslationsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Translations.Should().BeEmpty();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(lyricsId);
        var query = new PublicGetLyricsTranslationsQuery(lyricsId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
