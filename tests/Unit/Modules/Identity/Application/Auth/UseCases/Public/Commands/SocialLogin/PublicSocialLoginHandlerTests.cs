using _116.Core.Application.Shared.Repositories;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginHandler"/>. The handler verifies the provider token,
/// maps verification failures to localized errors, then hands the verified payload to the factory.
/// </summary>
public class PublicSocialLoginHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicSocialLoginAuthFactory> _authFactoryMock = new();
    private readonly Mock<ISessionFactory> _sessionFactoryMock = new();
    private readonly Mock<IFileRepository> _fileRepositoryMock = MockFileRepository.Create();
    private readonly Mock<ISocialTokenVerifierFactory> _verifierFactoryMock = new();
    private readonly Mock<ISocialTokenVerifier> _verifierMock = new();
    private readonly PublicSocialLoginHandler _handler;

    public PublicSocialLoginHandlerTests()
    {
        _verifierFactoryMock.Setup(x => x.For(It.IsAny<EnumAuthProvider>())).Returns(_verifierMock.Object);

        _handler = new PublicSocialLoginHandler(
            _authFactoryMock.Object,
            _sessionFactoryMock.Object,
            _fileRepositoryMock.Object,
            _verifierFactoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n(),
            Mapper
        );
    }

    private static PublicSocialLoginCommand Command() =>
        new(Provider: TestConstants.Auth.ProviderGoogle, IdToken: TestConstants.Auth.SocialLoginIdToken);

    private static SocialTokenPayload VerifiedPayload(bool emailVerified = true) =>
        new(
            ProviderSubjectId: TestConstants.Auth.SocialLoginProviderSubjectId,
            Email: TestConstants.Auth.SocialLoginEmail,
            EmailVerified: emailVerified,
            Name: TestConstants.Auth.SocialLoginUserName,
            PictureUrl: null
        );

    private void ArrangeVerify(SocialTokenPayload payload) =>
        _verifierMock
            .Setup(x => x.VerifyAsync(TestConstants.Auth.SocialLoginIdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

    #region Success Cases

    [Fact]
    public async Task Handle_WithVerifiedToken_ShouldReturnAuthenticationResult()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);

        ArrangeVerify(VerifiedPayload());
        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    It.IsAny<SocialTokenPayload>(),
                    EnumAuthProvider.Google,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, authData.UserPermissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthTestHelpers.CreateDefaultSessionResult());
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicSocialLoginResult result = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        result.AuthenticationResult.AccessToken.Should().Be("access-token");
        result.AuthenticationResult.RefreshToken.Should().Be("refresh-token");
        result.AuthenticationResult.User.Id.Should().Be(user.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenProviderEmailNotVerified_ShouldThrow()
    {
        // Arrange
        ArrangeVerify(VerifiedPayload(emailVerified: false));

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AccountNotVerifiedException>();
    }

    [Fact]
    public async Task Handle_WhenTokenDoesNotVerify_ShouldPropagateException()
    {
        // Arrange — the verifier throws for a token it cannot verify; the pipeline maps it, not the handler
        _verifierMock
            .Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SocialTokenVerificationException());

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SocialTokenVerificationException>();
    }

    [Fact]
    public async Task Handle_WhenProviderUnsupported_ShouldPropagateException()
    {
        // Arrange — no verifier is registered for the provider
        _verifierFactoryMock
            .Setup(x => x.For(It.IsAny<EnumAuthProvider>()))
            .Throws(new UnsupportedProviderException(EnumAuthProvider.Google));

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnsupportedProviderException>();
    }

    #endregion
}
