using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Validator for the <see cref="PublicGetLyricsByVideoIdQuery" /> ensuring a valid video ID is provided.
/// </summary>
public class PublicGetLyricsByVideoIdValidator : AbstractValidator<PublicGetLyricsByVideoIdQuery>
{
    /// <summary>
    /// Configures validation rules for the lyrics by video ID query.
    /// </summary>
    public PublicGetLyricsByVideoIdValidator()
    {
        RuleFor(x => x.VideoId).IsValidGuid("Video ID");
    }
}
