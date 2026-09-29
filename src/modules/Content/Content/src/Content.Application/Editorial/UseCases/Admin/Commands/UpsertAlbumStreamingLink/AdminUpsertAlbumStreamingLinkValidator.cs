using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink;

/// <summary>
/// Validator for the <see cref="AdminUpsertAlbumStreamingLinkCommand" />.
/// </summary>
public class AdminUpsertAlbumStreamingLinkValidator : AbstractValidator<AdminUpsertAlbumStreamingLinkCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpsertAlbumStreamingLinkValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpsertAlbumStreamingLinkValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Url).ValidStreamingLinkUrl(i18n.StreamingLink.Msg);
    }
}
