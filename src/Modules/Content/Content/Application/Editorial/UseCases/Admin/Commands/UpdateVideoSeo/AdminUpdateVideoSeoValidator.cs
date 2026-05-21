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
    /// <param name="msg">Article validation error messages.</param>
    public AdminUpdateVideoSeoValidator(ArticleErrorMessage msg)
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");

        When(
            x => x.MetaTitle is not null,
            () =>
                RuleFor(x => x.MetaTitle)
                    .ValidMetaTitle(
                        metaTitleTooShort: msg.MetaTitleTooShort(ContentConstants.MinMetaTitleLength),
                        metaTitleTooLong: msg.MetaTitleTooLong(ContentConstants.MaxMetaTitleLength)
                    )
        );
        When(
            x => x.MetaDescription is not null,
            () =>
                RuleFor(x => x.MetaDescription)
                    .ValidMetaDescription(
                        metaDescriptionTooShort: msg.MetaDescriptionTooShort(ContentConstants.MinMetaDescriptionLength),
                        metaDescriptionTooLong: msg.MetaDescriptionTooLong(ContentConstants.MaxMetaDescriptionLength)
                    )
        );
    }
}
