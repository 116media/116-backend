using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Validator for the <see cref="AdminUpdateVideoCommand" /> ensuring all editable video fields are valid.
/// </summary>
public class AdminUpdateVideoValidator : AbstractValidator<AdminUpdateVideoCommand>
{
    /// <summary>
    /// Configures validation rules for the full video update.
    /// </summary>
    public AdminUpdateVideoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");

        RuleFor(x => x.CategoryId).ValidArticleCategoryId();

        RuleFor(x => x.Title).ValidVideoTitle();

        RuleFor(x => x.Slug).ValidVideoSlug();

        RuleFor(x => x.Description).ValidVideoDescription();

        When(x => x.CustomerId.HasValue, () => RuleFor(x => x.OrderItemId).ValidOrderItemId());
        When(x => x.OrderItemId.HasValue, () => RuleFor(x => x.CustomerId).ValidCustomerId());

        When(x => x.MetaTitle is not null, () => RuleFor(x => x.MetaTitle).ValidMetaTitle());
        When(x => x.MetaDescription is not null, () => RuleFor(x => x.MetaDescription).ValidMetaDescription());
    }
}
