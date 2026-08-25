using System.Reflection;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="UserErrors"/>.
/// </summary>
public class UserErrorsTests
{
    private const string Email = "user@example.com";
    private const string Username = "john_doe";
    private const string PhoneNumber = "+1234567890";
    private const string RoleName = "Admin";
    private const string CoreRoleName = "Visitor";
    private const string MissingRoleName = "SuperAdmin";
    private const string PermissionResource = "users";
    private const string PermissionAction = "read";
    private const string MalformedUsername = "bad username!";
    private const string MalformedEmail = "not-an-email";

    private readonly UserErrors _errors = TestErrorsFactory.CreateUserErrors();
    private readonly ConflictErrorMessage _conflict = LocalizerFactory.CreateMessage<ConflictErrorMessage>();
    private readonly ValidationErrorMessage _validation = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly AuthenticationErrorMessage _authentication =
        LocalizerFactory.CreateMessage<AuthenticationErrorMessage>();
    private readonly AuthorizationErrorMessage _authorization =
        LocalizerFactory.CreateMessage<AuthorizationErrorMessage>();

    /// <summary>
    /// One factory of <see cref="UserErrors"/> under test: the exception type it is required to
    /// return, the call that produces it, and the message it must carry. The expected message is
    /// resolved from the test instance's localizers rather than from a literal, so an emptied
    /// resource entry fails the row instead of silently matching it.
    /// </summary>
    /// <param name="ExpectedException">The concrete exception type the factory must return.</param>
    /// <param name="Invoke">Invokes the factory under test.</param>
    /// <param name="ExpectedMessage">Resolves the localized message the exception must carry.</param>
    private sealed record ErrorCase(
        Type ExpectedException,
        Func<UserErrors, Exception> Invoke,
        Func<UserErrorsTests, string> ExpectedMessage
    );

    /// <summary>
    /// Every exception factory declared by <see cref="UserErrors"/>, keyed by the factory name so
    /// a failing theory row names the factory instead of rendering a delegate's type name.
    /// </summary>
    private static readonly Dictionary<string, ErrorCase> Cases = new()
    {
        [nameof(UserErrors.EmailAlreadyExists)] = new(
            typeof(ConflictException),
            e => e.EmailAlreadyExists(Email),
            t => t._conflict.EmailAlreadyExists(Email)
        ),
        [nameof(UserErrors.UsernameAlreadyExists)] = new(
            typeof(ConflictException),
            e => e.UsernameAlreadyExists(Username),
            t => t._conflict.UsernameAlreadyExists(Username)
        ),
        [nameof(UserErrors.PhoneNumberAlreadyExists)] = new(
            typeof(ConflictException),
            e => e.PhoneNumberAlreadyExists(PhoneNumber),
            t => t._conflict.PhoneNumberAlreadyExists(PhoneNumber)
        ),
        [nameof(UserErrors.RoleAlreadyExists)] = new(
            typeof(ConflictException),
            e => e.RoleAlreadyExists(RoleName),
            t => t._conflict.RoleAlreadyExists(RoleName)
        ),
        [nameof(UserErrors.RoleAlreadyAssignedToUser)] = new(
            typeof(ConflictException),
            e => e.RoleAlreadyAssignedToUser(),
            t => t._conflict.RoleAlreadyAssignedToUser()
        ),
        [nameof(UserErrors.PermissionAlreadyExists)] = new(
            typeof(ConflictException),
            e => e.PermissionAlreadyExists(PermissionResource, PermissionAction),
            t => t._conflict.PermissionAlreadyExists(PermissionResource, PermissionAction)
        ),
        [nameof(UserErrors.PermissionAlreadyAssignedToRole)] = new(
            typeof(ConflictException),
            e => e.PermissionAlreadyAssignedToRole(),
            t => t._conflict.PermissionAlreadyAssignedToRole()
        ),
        [nameof(UserErrors.RoleAlreadyActive)] = new(
            typeof(ConflictException),
            e => e.RoleAlreadyActive(),
            t => t._conflict.RoleAlreadyActive()
        ),
        [nameof(UserErrors.RoleAlreadyInactive)] = new(
            typeof(ConflictException),
            e => e.RoleAlreadyInactive(),
            t => t._conflict.RoleAlreadyInactive()
        ),
        [nameof(UserErrors.RoleAlreadyDeleted)] = new(
            typeof(ConflictException),
            e => e.RoleAlreadyDeleted(),
            t => t._conflict.RoleAlreadyDeleted()
        ),
        [nameof(UserErrors.RoleNotDeleted)] = new(
            typeof(ConflictException),
            e => e.RoleNotDeleted(),
            t => t._conflict.RoleNotDeleted()
        ),
        [nameof(UserErrors.PermissionAlreadyActive)] = new(
            typeof(ConflictException),
            e => e.PermissionAlreadyActive(),
            t => t._conflict.PermissionAlreadyActive()
        ),
        [nameof(UserErrors.PermissionAlreadyInactive)] = new(
            typeof(ConflictException),
            e => e.PermissionAlreadyInactive(),
            t => t._conflict.PermissionAlreadyInactive()
        ),
        [nameof(UserErrors.PermissionAlreadyDeleted)] = new(
            typeof(ConflictException),
            e => e.PermissionAlreadyDeleted(),
            t => t._conflict.PermissionAlreadyDeleted()
        ),
        [nameof(UserErrors.PermissionNotDeleted)] = new(
            typeof(ConflictException),
            e => e.PermissionNotDeleted(),
            t => t._conflict.PermissionNotDeleted()
        ),
        [nameof(UserErrors.AccountAlreadyVerified)] = new(
            typeof(ConflictException),
            e => e.AccountAlreadyVerified(),
            t => t._validation.AccountAlreadyVerified()
        ),
        [nameof(UserErrors.NewPasswordSameAsOld)] = new(
            typeof(ConflictException),
            e => e.NewPasswordSameAsOld(),
            t => t._validation.NewPasswordSameAsOld()
        ),
        [nameof(UserErrors.CoreRoleCannotBeDeleted)] = new(
            typeof(BadRequestException),
            e => e.CoreRoleCannotBeDeleted(CoreRoleName),
            t => t._validation.CoreRoleCannotBeDeleted(CoreRoleName)
        ),
        [nameof(UserErrors.RoleIsInactive)] = new(
            typeof(BadRequestException),
            e => e.RoleIsInactive(),
            t => t._validation.RoleIsInactive()
        ),
        [nameof(UserErrors.RoleIsDeleted)] = new(
            typeof(BadRequestException),
            e => e.RoleIsDeleted(),
            t => t._validation.RoleIsDeleted()
        ),
        [nameof(UserErrors.PermissionIsInactive)] = new(
            typeof(BadRequestException),
            e => e.PermissionIsInactive(),
            t => t._validation.PermissionIsInactive()
        ),
        [nameof(UserErrors.PermissionIsDeleted)] = new(
            typeof(BadRequestException),
            e => e.PermissionIsDeleted(),
            t => t._validation.PermissionIsDeleted()
        ),
        [nameof(UserErrors.PermissionNotAssignedToRole)] = new(
            typeof(BadRequestException),
            e => e.PermissionNotAssignedToRole(),
            t => t._validation.PermissionNotAssignedToRole()
        ),
        [nameof(UserErrors.RoleNotAssignedToUser)] = new(
            typeof(BadRequestException),
            e => e.RoleNotAssignedToUser(),
            t => t._validation.RoleNotAssignedToUser()
        ),
        [nameof(UserErrors.InvalidUsernameFormat)] = new(
            typeof(BadRequestException),
            e => e.InvalidUsernameFormat(MalformedUsername),
            t => t._validation.InvalidUsernameFormat(MalformedUsername)
        ),
        [nameof(UserErrors.PermissionResourceRequired)] = new(
            typeof(BadRequestException),
            e => e.PermissionResourceRequired(),
            t => t._validation.PermissionResourceRequired()
        ),
        [nameof(UserErrors.PermissionActionRequired)] = new(
            typeof(BadRequestException),
            e => e.PermissionActionRequired(),
            t => t._validation.PermissionActionRequired()
        ),
        [nameof(UserErrors.PermissionDescriptionRequired)] = new(
            typeof(BadRequestException),
            e => e.PermissionDescriptionRequired(),
            t => t._validation.PermissionDescriptionRequired()
        ),
        [nameof(UserErrors.RoleNameRequired)] = new(
            typeof(BadRequestException),
            e => e.RoleNameRequired(),
            t => t._validation.RoleNameRequired()
        ),
        [nameof(UserErrors.RoleDescriptionRequired)] = new(
            typeof(BadRequestException),
            e => e.RoleDescriptionRequired(),
            t => t._validation.RoleDescriptionRequired()
        ),
        [nameof(UserErrors.InvalidOtpCode)] = new(
            typeof(BadRequestException),
            e => e.InvalidOtpCode(),
            t => t._validation.InvalidOtpCode()
        ),
        [nameof(UserErrors.OtpNotYetVerified)] = new(
            typeof(BadRequestException),
            e => e.OtpNotYetVerified(),
            t => t._validation.OtpNotYetVerified()
        ),
        [nameof(UserErrors.PasswordNotConfigured)] = new(
            typeof(BadRequestException),
            e => e.PasswordNotConfigured(EnumAuthProvider.Google),
            t => t._validation.PasswordNotConfigured(EnumAuthProvider.Google)
        ),
        [nameof(UserErrors.IncorrectCurrentPassword)] = new(
            typeof(BadRequestException),
            e => e.IncorrectCurrentPassword(),
            t => t._validation.IncorrectCurrentPassword()
        ),
        [nameof(UserErrors.EmailRequiredToSetPassword)] = new(
            typeof(BadRequestException),
            e => e.EmailRequiredToSetPassword(),
            t => t._validation.EmailRequiredToSetPassword()
        ),
        [nameof(UserErrors.PasswordOnlyForExternalAuth)] = new(
            typeof(BadRequestException),
            e => e.PasswordOnlyForExternalAuth(),
            t => t._validation.PasswordOnlyForExternalAuth()
        ),
        [nameof(UserErrors.AccountInactive)] = new(
            typeof(AccountInactiveException),
            e => e.AccountInactive(Email),
            t => t._authorization.AccountInactive(Email)
        ),
        [nameof(UserErrors.AccountNotVerified)] = new(
            typeof(AccountNotVerifiedException),
            e => e.AccountNotVerified(Email),
            t => t._authorization.AccountNotVerified(Email)
        ),
        [nameof(UserErrors.InvalidCredentials)] = new(
            typeof(AuthenticationException),
            e => e.InvalidCredentials(),
            t => t._authentication.InvalidCredentials()
        ),
        [nameof(UserErrors.InvalidEmailFormat)] = new(
            typeof(AuthenticationException),
            e => e.InvalidEmailFormat(MalformedEmail),
            t => t._validation.InvalidEmailFormat(MalformedEmail)
        ),
        [nameof(UserErrors.InvalidPasswordFormat)] = new(
            typeof(AuthenticationException),
            e => e.InvalidPasswordFormat(),
            t => t._validation.InvalidPasswordFormat()
        ),
        [nameof(UserErrors.InvalidUserAuthentication)] = new(
            typeof(AuthenticationException),
            e => e.InvalidUserAuthentication(),
            t => t._authentication.InvalidUserAuthentication()
        ),
        [nameof(UserErrors.InsufficientPermissions)] = new(
            typeof(AccessDeniedException),
            e => e.InsufficientPermissions(),
            t => t._authentication.InsufficientPermissions()
        ),
        [nameof(UserErrors.NoValidOtpFound)] = new(
            typeof(NotFoundException),
            e => e.NoValidOtpFound(),
            t => t._validation.NoValidOtpFound()
        ),
        [nameof(UserErrors.RoleNotFoundByName)] = new(
            typeof(NotFoundException),
            e => e.RoleNotFoundByName(MissingRoleName),
            _ => $"Could not find Role with name: {MissingRoleName}"
        ),
        [nameof(UserErrors.OtpExpired)] = new(
            typeof(OtpExpirationException),
            e => e.OtpExpired(),
            t => t._validation.OtpExpired()
        ),
        [nameof(UserErrors.MaxOtpAttemptsReached)] = new(
            typeof(OtpAttemptsLimitException),
            e => e.MaxOtpAttemptsReached(),
            t => t._validation.MaxOtpAttemptsReached()
        ),
    };

    /// <summary>
    /// Supplies one theory row per exception factory, identified by the factory name so the
    /// runner reports a readable case identifier.
    /// </summary>
    /// <returns>The factory names as theory rows.</returns>
    public static TheoryData<string> FactoryCases() => new(Cases.Keys.OrderBy(name => name, StringComparer.Ordinal));

    #region Exception Factories

    [Fact]
    public void FactoryCases_ShouldCoverEveryExceptionFactoryDeclaredByUserErrors()
    {
        // Arrange
        IEnumerable<string> declared = typeof(UserErrors)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && typeof(Exception).IsAssignableFrom(m.ReturnType))
            .Select(m => m.Name);

        // Assert
        declared.Should().BeEquivalentTo(Cases.Keys);
    }

    [Theory]
    [MemberData(nameof(FactoryCases))]
    public void Factory_ShouldReturnTheDeclaredExceptionTypeWithTheLocalizedMessage(string caseName)
    {
        // Arrange
        ErrorCase errorCase = Cases[caseName];

        // Act
        Exception exception = errorCase.Invoke(_errors);

        // Assert
        exception.Should().BeOfType(errorCase.ExpectedException, caseName);
        exception.Message.Should().Be(errorCase.ExpectedMessage(this), caseName);
    }

    #endregion

    #region Message Providers

    [Fact]
    public void Validation_ShouldReturnValidationErrorMessage()
    {
        ValidationErrorMessage result = _errors.Validation;

        result.Should().NotBeNull();
        result.RoleIsInactive().Should().Be(_validation.RoleIsInactive());
    }

    [Fact]
    public void ConflictErrorMessage_Localizer_EmailAlreadyExists_ShouldReturnLocalizedString()
    {
        _conflict.Localizer["EmailAlreadyExists"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuthenticationErrorMessage_Localizer_InvalidCredentials_ShouldReturnLocalizedString()
    {
        _authentication.Localizer["InvalidCredentials"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuthorizationErrorMessage_Localizer_AccountInactive_ShouldReturnLocalizedString()
    {
        _authorization.Localizer["AccountInactive"].Value.Should().NotBeNullOrEmpty();
    }

    #endregion
}
