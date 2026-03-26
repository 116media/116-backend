using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Handles the <see cref="PublicSignUpCommand" /> to register new public users.
/// </summary>
/// <param name="authFactory">Factory for handling user registration logic.</param>
/// <param name="sessionFactory">Factory for creating authentication sessions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicSignUpHandler(IPublicSignUpAuthFactory authFactory, ISessionFactory sessionFactory, IMapper mapper)
    : ICommandHandler<PublicSignUpCommand, PublicSignUpResult>
{
    /// <summary>
    /// Handles the public sign-up command by creating a new user account.
    /// </summary>
    /// <param name="command">The public sign-up command containing user registration data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSignUpResult" /> containing user authentication information.</returns>
    public async Task<PublicSignUpResult> Handle(PublicSignUpCommand command, CancellationToken cancellationToken)
    {
        // Register user and get associated data
        PublicSignUpAuthData authData = await authFactory.RegisterAsync(
            email: command.Email,
            userName: command.UserName,
            password: command.Password,
            cancellationToken: cancellationToken
        );

        // Create authentication session with tokens
        SessionResult sessionData = await sessionFactory.CreateSessionAsync(
            user: authData.User,
            userPermissions: authData.UserPermissions,
            cancellationToken: cancellationToken
        );

        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper)
        );

        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: sessionData.AccessToken,
            RefreshToken: sessionData.RefreshToken,
            AccessTokenExpiresAt: sessionData.AccessTokenExpiresAt,
            RefreshTokenExpiresAt: sessionData.RefreshTokenExpiresAt
        );

        return new PublicSignUpResult(AuthenticationResult: authResult, true);
    }
}
