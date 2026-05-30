using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Validator for the <see cref="AdminUpdateArticleSeoCommand" /> ensuring a valid article ID is provided
/// and that SEO fields respect their length constraints when supplied.
/// </summary>
public class AdminUpdateArticleSeoValidator : AbstractValidator<AdminUpdateArticleSeoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateArticleSeoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUpdateArticleSeoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer);

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
