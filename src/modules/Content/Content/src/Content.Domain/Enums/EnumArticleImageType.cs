namespace _116.Content.Domain.Enums;

/// <summary>
/// Identifies whether an image associated with an article is the cover image
/// or an inline body image embedded in the rich-text editor.
/// Stored as a <c>string</c> in the database via EF Core's <c>.HasConversion&lt;string&gt;()</c>.
/// </summary>
public enum EnumArticleImageType
{
    /// <summary>
    /// The article's primary cover image, displayed at the top of the article
    /// and in all article listing cards and feeds.
    /// </summary>
    Cover,

    /// <summary>
    /// An image embedded inline inside the article's rich-text body HTML.
    /// Multiple body images per article are supported.
    /// </summary>
    Body,
}
