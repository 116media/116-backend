using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.User.Ports;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Helpers;
using _116.Identity.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginHandler"/>: the authentication call, the session
/// and the response. Token verification is covered by <c>PublicSocialLoginAuthServiceTests</c>.
/// </summary>
public class PublicSocialLoginHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicSocialLoginAuthService> _authServiceMock = new();
    private readonly Mock<ISessionService> _sessionServiceMock = new();
    private readonly Mock<IAvatarService> _avatarServiceMock = MockAvatarService.Create();
    private readonly PublicSocialLoginHandler _handler;

    public PublicSocialLoginHandlerTests()
    {
        _handler = new PublicSocialLoginHandler(
            _authServiceMock.Object,
            _sessionServiceMock.Object,
            _avatarServiceMock.Object,
            Mapper
        );
    }

    private static PublicSocialLoginCommand Command() =>
        new(Provider: TestConstants.Auth.ProviderGoogle, IdToken: TestConstants.Auth.SocialLoginIdToken);

    private UserEntity ArrangeAuthenticatedUser()
    {
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);

        _authServiceMock
            .Setup(x =>
                x.AuthenticateAsync(
                    EnumAuthProvider.Google,
                    TestConstants.Auth.SocialLoginIdToken,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionServiceMock
            .Setup(x => x.CreateSessionAsync(user, authData.UserPermissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthTestHelpers.CreateDefaultSessionResult());
        _avatarServiceMock.SetupGetAvatarReturnsNull(user.AvatarFileId);

        return user;
    }

    [Fact]
    public async Task Handle_WithVerifiedToken_ShouldReturnAuthenticationDto()
    {
        // Arrange
        UserEntity user = ArrangeAuthenticatedUser();

        // Act
        PublicSocialLoginResult result = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        result.Authentication.AccessToken.Should().Be("access-token");
        result.Authentication.RefreshToken.Should().Be("refresh-token");
        result.Authentication.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldOpenTheSessionForTheAuthenticatedUser()
    {
        // Arrange
        UserEntity user = ArrangeAuthenticatedUser();

        // Act
        await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        _sessionServiceMock.Verify(
            x => x.CreateSessionAsync(user, It.IsAny<List<RolePermissionEntity>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldPropagateWithoutOpeningASession()
    {
        // Arrange
        _authServiceMock
            .Setup(x =>
                x.AuthenticateAsync(It.IsAny<EnumAuthProvider>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new SocialTokenVerificationException());

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SocialTokenVerificationException>();
        _sessionServiceMock.Verify(
            x =>
                x.CreateSessionAsync(
                    It.IsAny<UserEntity>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToTheAuthService()
    {
        // Arrange
        ArrangeAuthenticatedUser();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(Command(), cts.Token);

        // Assert
        _authServiceMock.Verify(
            x => x.AuthenticateAsync(EnumAuthProvider.Google, TestConstants.Auth.SocialLoginIdToken, cts.Token),
            Times.Once
        );
    }
}
