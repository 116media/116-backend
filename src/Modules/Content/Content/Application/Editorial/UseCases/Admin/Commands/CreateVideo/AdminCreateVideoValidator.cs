using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Validator for the <see cref="AdminCreateVideoCommand" /> ensuring proper video draft creation data.
/// </summary>
public class AdminCreateVideoValidator : AbstractValidator<AdminCreateVideoCommand>
{
    /// <summary>
    /// Configures validation rules for video draft creation.
    /// </summary>
    public AdminCreateVideoValidator()
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId();

        RuleFor(x => x.Title).ValidVideoTitle();
        RuleFor(x => x.Slug).ValidVideoSlug();

        RuleFor(x => x.Description).ValidVideoDescription();

        RuleFor(x => x.OrderItemId).ValidOrderItemIdConditional(x => x.CustomerId.HasValue);

        RuleFor(x => x.CustomerId).ValidCustomerIdConditional(x => x.OrderItemId.HasValue);
    }
}
