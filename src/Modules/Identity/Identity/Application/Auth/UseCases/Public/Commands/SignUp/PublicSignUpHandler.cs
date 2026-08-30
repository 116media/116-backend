using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Handles the <see cref="PublicSignUpCommand" /> to register new public users. No session or
/// tokens are issued: the user verifies the emailed code first, then logs in.
/// </summary>
/// <param name="authFactory">Factory for handling user registration logic.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicSignUpHandler(IPublicSignUpAuthFactory authFactory, IMapper mapper)
    : ICommandHandler<PublicSignUpCommand, PublicSignUpResult>
{
    /// <summary>
    /// Handles the public sign-up command by creating a new unverified user account.
    /// </summary>
    /// <param name="command">The public sign-up command containing user registration data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSignUpResult" /> containing the created user.</returns>
    public async Task<PublicSignUpResult> Handle(PublicSignUpCommand command, CancellationToken cancellationToken)
    {
        PublicSignUpAuthData authData = await authFactory.RegisterAsync(
            email: command.Email,
            userName: command.UserName,
            password: command.Password,
            cancellationToken: cancellationToken
        );

        UserResponseDto userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper)
        );

        return new PublicSignUpResult(User: userDto, VerificationRequired: true);
    }
}
