using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Handles the <see cref="PublicSocialLoginCommand" /> for social authentication. Verifies the
/// provider token, maps verification failures to localized errors, and hands the verified identity to
/// the auth factory.
/// </summary>
/// <param name="authFactory">Factory for handling social authentication logic.</param>
/// <param name="sessionFactory">Factory for creating authentication sessions.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="verifierFactory">Resolves the provider token verifier.</param>
/// <param name="i18n">Identity module i18n facade for localized errors.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicSocialLoginHandler(
    IPublicSocialLoginAuthFactory authFactory,
    ISessionFactory sessionFactory,
    IFileRepository fileRepository,
    ISocialTokenVerifierFactory verifierFactory,
    IdentityI18n i18n,
    IMapper mapper
) : ICommandHandler<PublicSocialLoginCommand, PublicSocialLoginResult>
{
    /// <summary>
    /// Handles the social login command by verifying the provider token and finding or creating a
    /// user account.
    /// </summary>
    /// <param name="command">The social login command containing the provider and its token.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSocialLoginResult" /> containing authentication information.</returns>
    public async Task<PublicSocialLoginResult> Handle(
        PublicSocialLoginCommand command,
        CancellationToken cancellationToken
    )
    {
        EnumAuthProvider provider = new AuthProvider(value: command.Provider).Value;

        // An unsupported provider or an unverifiable token surfaces as an exception mapped by the
        // global pipeline; the adapters stay i18n-free.
        ISocialTokenVerifier verifier = verifierFactory.For(provider: provider);
        SocialTokenPayload payload = await verifier.VerifyAsync(
            idToken: command.IdToken,
            cancellationToken: cancellationToken
        );

        if (!payload.EmailVerified || string.IsNullOrWhiteSpace(value: payload.Email))
        {
            throw i18n.User.ProviderEmailNotVerified();
        }

        // Authenticate or create user from the verified payload and get associated data
        PublicSocialLoginAuthData authData = await authFactory.AuthenticateOrCreateAsync(
            payload: payload,
            provider: provider,
            cancellationToken: cancellationToken
        );

        // Create authentication session with tokens
        SessionResult sessionData = await sessionFactory.CreateSessionAsync(
            user: authData.User,
            userPermissions: authData.UserPermissions,
            cancellationToken: cancellationToken
        );

        // Fetch user avatar
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );

        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: sessionData.AccessToken,
            AccessTokenExpiresAt: sessionData.AccessTokenExpiresAt,
            RefreshToken: sessionData.RefreshToken,
            RefreshTokenExpiresAt: sessionData.RefreshTokenExpiresAt
        );

        return new PublicSocialLoginResult(AuthenticationResult: authResult);
    }
}
