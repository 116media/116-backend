using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicRequestLyricsTranslationRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class PublicRequestLyricsTranslationRequestBuilder
{
    private string _language;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicRequestLyricsTranslationRequestBuilder"/> class with valid default values.
    /// </summary>
    public PublicRequestLyricsTranslationRequestBuilder()
    {
        _language = "es";
    }

    /// <summary>
    /// Sets the language.
    /// </summary>
    /// <param name="language">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicRequestLyricsTranslationRequestBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicRequestLyricsTranslationRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicRequestLyricsTranslationRequest instance.</returns>
    public PublicRequestLyricsTranslationRequest Build()
    {
        return new PublicRequestLyricsTranslationRequest(Language: _language);
    }
}
