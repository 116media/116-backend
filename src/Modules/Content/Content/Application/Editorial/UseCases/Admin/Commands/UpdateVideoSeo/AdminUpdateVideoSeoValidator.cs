using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateVideoSeoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer, "VideoIdRequired", "VideoIdInvalid");

        When(
            x => x.MetaTitle is not null,
            () =>
                RuleFor(x => x.MetaTitle)
                    .ValidMetaTitle(
                        metaTitleTooShort: i18n.Article.Msg.MetaTitleTooShort(ContentConstants.MinMetaTitleLength),
                        metaTitleTooLong: i18n.Article.Msg.MetaTitleTooLong(ContentConstants.MaxMetaTitleLength)
                    )
        );
        When(
            x => x.MetaDescription is not null,
            () =>
                RuleFor(x => x.MetaDescription)
                    .ValidMetaDescription(
                        metaDescriptionTooShort: i18n.Article.Msg.MetaDescriptionTooShort(
                            ContentConstants.MinMetaDescriptionLength
                        ),
                        metaDescriptionTooLong: i18n.Article.Msg.MetaDescriptionTooLong(
                            ContentConstants.MaxMetaDescriptionLength
                        )
                    )
        );
    }
}
