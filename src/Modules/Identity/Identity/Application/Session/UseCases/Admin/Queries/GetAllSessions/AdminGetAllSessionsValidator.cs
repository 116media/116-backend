using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Validator for <see cref="AdminGetAllSessionsQuery" /> that ensures the optional user ID filter is valid.
/// </summary>
public class AdminGetAllSessionsValidator : AbstractValidator<AdminGetAllSessionsQuery>
{
    /// <summary>
    /// Configure validation rules for the query parameters.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminGetAllSessionsValidator(IdentityI18n i18n)
    {
        RuleFor(x => x.UserId)
            .IsValidGuid(i18n.User.Validation.Localizer, "UserIdRequired", "UserIdInvalid", isRequired: false);
    }
}
