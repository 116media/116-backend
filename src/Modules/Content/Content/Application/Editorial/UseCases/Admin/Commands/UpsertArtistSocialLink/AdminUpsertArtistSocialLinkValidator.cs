using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;

/// <summary>
/// Validator for the <see cref="AdminUpsertArtistSocialLinkCommand" />. The URL becomes an
/// href on the public page, so the scheme is locked to https on write: a javascript: value
/// is a stored XSS vector, and rejecting rather than coercing http gives the admin a real
/// error instead of a link that may 404.
/// </summary>
public class AdminUpsertArtistSocialLinkValidator : AbstractValidator<AdminUpsertArtistSocialLinkCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpsertArtistSocialLinkValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpsertArtistSocialLinkValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Platform).IsInEnum();

        RuleFor(x => x.Url)
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(maximumLength: ContentConstants.MaxStreamingLinkUrlLength)
            .Must(url =>
                Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, out Uri? parsed)
                && parsed.Scheme == Uri.UriSchemeHttps
            );
    }
}
