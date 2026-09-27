using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Validator for the <see cref="PublicRecordShortVideoViewCommand" />. The device identifier is
/// capped below the dedup-key column, which stores it behind a prefix.
/// </summary>
public class PublicRecordShortVideoViewValidator : AbstractValidator<PublicRecordShortVideoViewCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicRecordShortVideoViewValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicRecordShortVideoViewValidator(ContentI18n i18n)
    {
        RuleFor(x => x.DeviceId).ValidViewDeviceId(i18n.ShortVideoInteraction.Msg);

        RuleFor(x => x.IpAddress).ValidViewIpAddress(i18n.ShortVideoInteraction.Msg);

        RuleFor(x => x.UserAgent).ValidViewUserAgent(i18n.ShortVideoInteraction.Msg);
    }
}
