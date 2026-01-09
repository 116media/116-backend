using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Validator for <see cref="AdminGetAllSessionsQuery" /> that ensures valid pagination and filter parameters.
/// </summary>
public class AdminGetAllSessionsValidator : AbstractValidator<AdminGetAllSessionsQuery>
{
    /// <summary>
    /// Configure validation rules for the query parameters.
    /// </summary>
    public AdminGetAllSessionsValidator()
    {
        RuleFor(x => x.PaginatedRequest.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be greater than or equal to 0.");

        RuleFor(x => x.PaginatedRequest.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.UserId).IsValidGuid("User ID", false);
    }
}
