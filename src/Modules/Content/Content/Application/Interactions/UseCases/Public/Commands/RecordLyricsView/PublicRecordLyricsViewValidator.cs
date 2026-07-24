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
    public PublicRecordLyricsViewValidator()
    {
        RuleFor(x => x.DwellMs).GreaterThanOrEqualTo(0);

        RuleFor(x => x.ScrollDepthRatio).InclusiveBetween(0, 1);
    }
}
