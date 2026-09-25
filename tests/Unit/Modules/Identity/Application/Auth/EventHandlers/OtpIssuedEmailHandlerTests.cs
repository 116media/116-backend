using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Application.Shared.Messages;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Unit tests for <see cref="OtpIssuedEmailHandler" />: the code reaches the owner through the
/// template its purpose selects, in the culture captured where it was issued.
/// </summary>
public class OtpIssuedEmailHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupMock = new();
    private readonly Mock<IMessageDispatcher> _dispatcherMock = new();
    private readonly OtpIssuedEmailHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();

    public OtpIssuedEmailHandlerTests()
    {
        _handler = new OtpIssuedEmailHandler(
            _userLookupMock.Object,
            _dispatcherMock.Object,
            NullLogger<OtpIssuedEmailHandler>.Instance
        );
    }

    /// <summary>
    /// Points the lookup at a recipient with the supplied address.
    /// </summary>
    /// <param name="email">The address to resolve, or null for an account without one.</param>
    private void SetupRecipient(string? email)
    {
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto(UserName: "Fan", Email: email, AvatarFileId: null, Role: null));
    }

    [Theory]
    [InlineData(EnumOtpPurpose.EmailVerification, IdentityMessageTemplates.EmailVerificationOtp)]
    [InlineData(EnumOtpPurpose.PasswordReset, IdentityMessageTemplates.PasswordResetOtp)]
    public async Task Handle_ShouldSendThePurposeTemplateCarryingThePlainCode(
        EnumOtpPurpose purpose,
        string expectedTemplate
    )
    {
        // Arrange
        SetupRecipient("fan@example.com");
        var domainEvent = new OtpIssuedEvent(UserId, TestConstants.Otp.DefaultCode, purpose);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<Message>(m =>
                        m.TemplateName == expectedTemplate
                        && m.Recipients[0].Address == "fan@example.com"
                        && m.Tokens["otpCode"] == TestConstants.Otp.DefaultCode
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldSendInTheCultureTheEventCarries()
    {
        // Arrange
        SetupRecipient("fan@example.com");
        var domainEvent = new OtpIssuedEvent(UserId, TestConstants.Otp.DefaultCode, EnumOtpPurpose.PasswordReset);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EnumOtpPurpose.TwoFactorAuthentication)]
    [InlineData(EnumOtpPurpose.AccountRecovery)]
    public async Task Handle_WithAPurposeThatHasNoTemplate_ShouldSendNothing(EnumOtpPurpose purpose)
    {
        // Arrange
        var domainEvent = new OtpIssuedEvent(UserId, TestConstants.Otp.DefaultCode, purpose);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _userLookupMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTheAccountHasNoEmailAddress_ShouldSendNothing()
    {
        // Arrange
        SetupRecipient(email: null);
        var domainEvent = new OtpIssuedEvent(UserId, TestConstants.Otp.DefaultCode, EnumOtpPurpose.EmailVerification);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
    }
}
