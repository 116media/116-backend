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

        RuleFor(x => x.Description).NotEmpty().WithMessage("Video description is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty()
            .When(x => x.CustomerId.HasValue)
            .WithMessage("Order item ID is required when customer ID is provided.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .When(x => x.OrderItemId.HasValue)
            .WithMessage("Customer ID is required when order item ID is provided.");
    }
}
