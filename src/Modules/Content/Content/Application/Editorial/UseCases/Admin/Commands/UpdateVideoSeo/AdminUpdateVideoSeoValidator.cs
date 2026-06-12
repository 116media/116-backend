using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Validator for the <see cref="AdminUpdateVideoSeoCommand" /> ensuring a valid video ID is provided
/// and that SEO fields respect their length constraints when supplied.
/// </summary>
public class AdminUpdateVideoSeoValidator : AbstractValidator<AdminUpdateVideoSeoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateVideoSeoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Article validation error messages.</param>
    public AdminUpdateVideoSeoValidator(ArticleErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer, "VideoIdRequired", "VideoIdInvalid");

        When(
            x => x.MetaTitle is not null,
            () =>
                RuleFor(x => x.MetaTitle)
                    .ValidMetaTitle(
                        metaTitleTooShort: i18n.MetaTitleTooShort(ContentConstants.MinMetaTitleLength),
                        metaTitleTooLong: i18n.MetaTitleTooLong(ContentConstants.MaxMetaTitleLength)
                    )
        );
        When(
            x => x.MetaDescription is not null,
            () =>
                RuleFor(x => x.MetaDescription)
                    .ValidMetaDescription(
                        metaDescriptionTooShort: i18n.MetaDescriptionTooShort(
                            ContentConstants.MinMetaDescriptionLength
                        ),
                        metaDescriptionTooLong: i18n.MetaDescriptionTooLong(ContentConstants.MaxMetaDescriptionLength)
                    )
        );
    }
}
