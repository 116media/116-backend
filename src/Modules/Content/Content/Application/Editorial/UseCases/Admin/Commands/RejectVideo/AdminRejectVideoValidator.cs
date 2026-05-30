using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Validator for the <see cref="AdminRejectVideoCommand" /> ensuring a rejection reason is provided.
/// </summary>
public class AdminRejectVideoValidator : AbstractValidator<AdminRejectVideoCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRejectVideoValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRejectVideoValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Article.Msg.Localizer, "VideoIdRequired", "VideoIdInvalid");

        RuleFor(x => x.Reason).ValidRejectionReason(i18n.Article.Msg);
    }
}
