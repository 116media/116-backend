using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Domain.Constants;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Validator for the <see cref="AdminResolveAlbumStreamingLinksCommand" />. The source URL
/// must be an absolute https URL — the same trust posture as every stored outbound link.
/// </summary>
public class AdminResolveAlbumStreamingLinksValidator : AbstractValidator<AdminResolveAlbumStreamingLinksCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminResolveAlbumStreamingLinksValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminResolveAlbumStreamingLinksValidator(ContentI18n i18n)
    {
        RuleFor(x => x.SourceUrl)
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(maximumLength: ContentConstants.MaxStreamingLinkUrlLength)
            .Must(url =>
                Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, out Uri? parsed)
                && parsed.Scheme == Uri.UriSchemeHttps
            )
            .WithMessage(i18n.StreamingLink.Msg.UnresolvableSourceUrl());
    }
}
