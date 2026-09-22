using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;

/// <summary>
/// Validator for the <see cref="PublicRecordLyricsViewCommand" /> ensuring the reported
/// read-time signals are within their valid ranges before the handler recomputes the
/// expected reading time server-side.
/// </summary>
public class PublicRecordLyricsViewValidator : AbstractValidator<PublicRecordLyricsViewCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicRecordLyricsViewValidator" />.
    /// </summary>
    public PublicRecordLyricsViewValidator(ContentI18n i18n)
    {
        RuleFor(x => x.DwellMs).ValidViewDwellMs(i18n.LyricsInteraction.Msg);

        RuleFor(x => x.ScrollDepthRatio).ValidViewScrollDepthRatio(i18n.LyricsInteraction.Msg);
    }
}
