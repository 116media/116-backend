using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Validator for the <see cref="AdminAddOrderItemCommand" />.
/// </summary>
public class AdminAddOrderItemValidator : AbstractValidator<AdminAddOrderItemCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminAddOrderItemValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content order validation error messages.</param>
    /// <param name="categoryMsg">Category validation error messages.</param>
    public AdminAddOrderItemValidator(ContentOrderErrorMessage i18n, CategoryErrorMessage categoryMsg)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.CategoryId).IsValidGuid(categoryMsg.Localizer);
        RuleFor(x => x.ContentKind).ValidOrderItemContentKind(i18n);
    }
}
