using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the user aggregate.
/// </summary>
public sealed class UserRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [IdentityRuleCodes.InvalidEmailFormat] = new(
                StatusCodes.Status401Unauthorized,
                nameof(AuthenticationException),
                (ctx, args) => ctx.Resolve<ValidationErrorMessage>().InvalidEmailFormat(email: args[0])
            ),
            [IdentityRuleCodes.InvalidUsernameFormat] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, args) => ctx.Resolve<ValidationErrorMessage>().InvalidUsernameFormat(userName: args[0])
            ),
            [IdentityRuleCodes.InvalidPasswordFormat] = new(
                StatusCodes.Status401Unauthorized,
                nameof(AuthenticationException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().InvalidPasswordFormat()
            ),
            [IdentityRuleCodes.ProviderMismatch] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ConflictErrorMessage>().ProviderMismatch()
            ),
            [IdentityRuleCodes.EmailRequiredToSetPassword] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().EmailRequiredToSetPassword()
            ),
            [IdentityRuleCodes.AccountInactive] = new(
                StatusCodes.Status423Locked,
                nameof(AccountInactiveException),
                (ctx, args) => ctx.Resolve<AuthorizationErrorMessage>().AccountInactive(email: args[0])
            ),
            [IdentityRuleCodes.AccountNotVerified] = new(
                StatusCodes.Status403Forbidden,
                nameof(AccountNotVerifiedException),
                (ctx, args) => ctx.Resolve<AuthorizationErrorMessage>().AccountNotVerified(email: args[0])
            ),
            [IdentityRuleCodes.RoleAlreadyAssignedToUser] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ConflictErrorMessage>().RoleAlreadyAssignedToUser()
            ),
        };
}
