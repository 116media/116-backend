using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById;

/// <summary>
/// Validator for <see cref="PublicGetOwnSessionByIdQuery" /> that ensures valid GUIDs.
/// </summary>
public class PublicGetOwnSessionByIdValidator : AbstractValidator<PublicGetOwnSessionByIdQuery>
{
    /// <summary>
    /// Configure validation rules for the query properties.
    /// </summary>
    public PublicGetOwnSessionByIdValidator()
    {
        RuleFor(x => x.SessionId).Cascade(CascadeMode.Stop).IsValidGuid("Session ID");
    }
}
