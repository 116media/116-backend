using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsEntity"/> instances in tests.
/// For test code, prefer using LyricsFactory instead of direct Builder usage.
/// </summary>
internal class LyricsBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _authorId = Guid.NewGuid();
    private string _songTitle = TestConstants.Content.Editorial.Lyrics.ValidSongTitle;
    private string _artistName = TestConstants.Content.Editorial.Lyrics.ValidArtistName;
    private string _lyricsText = TestConstants.Content.Editorial.Lyrics.ValidLyricsText;
    private string _language = TestConstants.Content.Editorial.Lyrics.ValidLanguage;
    private Guid? _videoId;
    private Guid? _articleId;

    /// <summary>Sets the lyrics ID.</summary>
    public LyricsBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the author ID.</summary>
    public LyricsBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    /// <summary>Sets the song title.</summary>
    public LyricsBuilder WithSongTitle(string songTitle)
    {
        _songTitle = songTitle;
        return this;
    }

    /// <summary>Sets the artist name.</summary>
    public LyricsBuilder WithArtistName(string artistName)
    {
        _artistName = artistName;
        return this;
    }

    /// <summary>Sets the lyrics text.</summary>
    public LyricsBuilder WithLyricsText(string lyricsText)
    {
        _lyricsText = lyricsText;
        return this;
    }

    /// <summary>Sets the language code.</summary>
    public LyricsBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>Links this lyrics page to a video.</summary>
    public LyricsBuilder ForVideo(Guid videoId)
    {
        _videoId = videoId;
        _articleId = null;
        return this;
    }

    /// <summary>Links this lyrics page to an article.</summary>
    public LyricsBuilder ForArticle(Guid articleId)
    {
        _articleId = articleId;
        _videoId = null;
        return this;
    }

    /// <summary>Builds the <see cref="LyricsEntity"/> instance.</summary>
    public LyricsEntity Build()
    {
        if (_videoId.HasValue)
        {
            return LyricsEntity.CreateForVideo(
                id: _id,
                videoId: _videoId.Value,
                songTitle: _songTitle,
                artistName: _artistName,
                lyricsText: _lyricsText,
                language: _language,
                authorId: _authorId
            );
        }

        if (_articleId.HasValue)
        {
            return LyricsEntity.CreateForArticle(
                id: _id,
                articleId: _articleId.Value,
                songTitle: _songTitle,
                artistName: _artistName,
                lyricsText: _lyricsText,
                language: _language,
                authorId: _authorId
            );
        }

        return LyricsEntity.CreateStandalone(
            id: _id,
            songTitle: _songTitle,
            artistName: _artistName,
            lyricsText: _lyricsText,
            language: _language,
            authorId: _authorId
        );
    }
}
