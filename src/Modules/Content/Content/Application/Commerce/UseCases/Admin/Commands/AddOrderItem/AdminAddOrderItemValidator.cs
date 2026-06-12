using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminAddOrderItemValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);
        RuleFor(x => x.CategoryId).IsValidGuid(i18n.Category.Msg.Localizer);
        RuleFor(x => x.ContentKind).ValidOrderItemContentKind(i18n.ContentOrder.Msg);
    }
}
