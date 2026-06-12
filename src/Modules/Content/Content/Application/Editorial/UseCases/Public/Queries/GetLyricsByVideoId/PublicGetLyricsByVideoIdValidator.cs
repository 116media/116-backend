using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Validator for the <see cref="PublicGetLyricsByVideoIdQuery" /> ensuring a valid video ID is provided.
/// </summary>
public class PublicGetLyricsByVideoIdValidator : AbstractValidator<PublicGetLyricsByVideoIdQuery>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicGetLyricsByVideoIdValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Video validation error messages.</param>
    public PublicGetLyricsByVideoIdValidator(VideoErrorMessage i18n)
    {
        RuleFor(x => x.VideoId).IsValidGuid(i18n.Localizer);
    }
}
