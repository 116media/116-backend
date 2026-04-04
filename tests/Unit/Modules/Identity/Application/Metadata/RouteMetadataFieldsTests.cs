using System.Reflection;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;
using _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions;
using _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;
using _116.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken;
using _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;
using _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;
using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById;
using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;
using PublicGetOwnProfileNS = _116.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile;
using PublicUpdateAvatarNS = _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using PublicUpdateOwnProfileNS = _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

namespace _116.Unit.Tests.Modules.Identity.Application.Metadata;

/// <summary>
/// Unit tests for all RouteMetadata fields across the application.
/// </summary>
public class RouteMetadataFieldsTests
{
    [Theory]
    [InlineData(typeof(AdminLoginMetaField), "AdminLogin", "AdminLogin")]
    [InlineData(typeof(AdminChangePasswordMetaField), "ChangePassword", "AdminChangePassword")]
    [InlineData(typeof(AdminForgotPasswordMetaField), "ForgotPassword", "AdminForgotPassword")]
    [InlineData(typeof(AdminResendOtpMetaField), "ResendOtp", "AdminResendOtp")]
    [InlineData(typeof(AdminResetPasswordMetaField), "ResetPassword", "AdminResetPassword")]
    [InlineData(typeof(AdminSignOutMetaField), "AdminSignOut", "AdminSignOut")]
    [InlineData(
        typeof(AdminSignOutFromAllDevicesMetaField),
        "AdminSignOutFromAllDevices",
        "AdminSignOutFromAllDevices"
    )]
    [InlineData(typeof(AdminVerifyOtpMetaField), "VerifyOtp", "AdminVerifyOtp")]
    [InlineData(typeof(PublicLoginMetaField), "PublicLogin", "PublicLogin")]
    [InlineData(typeof(PublicChangePasswordMetaField), "ChangePassword", "PublicChangePassword")]
    [InlineData(typeof(PublicForgotPasswordMetaField), "ForgotPassword", "PublicForgotPassword")]
    [InlineData(typeof(PublicResendOtpMetaField), "ResendOtp", "PublicResendOtp")]
    [InlineData(typeof(PublicResetPasswordMetaField), "ResetPassword", "PublicResetPassword")]
    [InlineData(typeof(PublicSetPasswordMetaField), "SetPassword", "PublicSetPassword")]
    [InlineData(typeof(PublicSignOutMetaField), "SignOut", "PublicSignOut")]
    [InlineData(typeof(PublicSignOutFromAllDevicesMetaField), "SignOutFromAllDevices", "PublicSignOutFromAllDevices")]
    [InlineData(typeof(PublicSignUpMetaField), "PublicSignUp", "PublicSignUp")]
    [InlineData(typeof(PublicSocialLoginMetaField), "SocialLogin", "PublicSocialLogin")]
    [InlineData(typeof(PublicVerifyOtpMetaField), "VerifyOtp", "PublicVerifyOtp")]
    [InlineData(typeof(AdminCreateRoleMetaField), "AdminCreateRole", "AdminCreateRole")]
    [InlineData(typeof(AdminUpdateRoleMetaField), "AdminUpdateRole", "AdminUpdateRole")]
    [InlineData(typeof(AdminActivateRoleMetaField), "AdminActivateRole", "AdminActivateRole")]
    [InlineData(typeof(AdminDeactivateRoleMetaField), "AdminDeactivateRole", "AdminDeactivateRole")]
    [InlineData(typeof(AdminSoftDeleteRoleMetaField), "AdminSoftDeleteRole", "AdminSoftDeleteRole")]
    [InlineData(typeof(AdminHardDeleteRoleMetaField), "AdminHardDeleteRole", "AdminHardDeleteRole")]
    [InlineData(typeof(AdminRestoreRoleMetaField), "AdminRestoreRole", "AdminRestoreRole")]
    [InlineData(typeof(AdminGetAllRolesMetaField), "AdminGetAllRoles", "AdminGetAllRoles")]
    [InlineData(typeof(AdminGetRoleByIdMetaField), "AdminGetRoleById", "AdminGetRoleById")]
    [InlineData(typeof(AdminCreatePermissionMetaField), "AdminCreatePermission", "AdminCreatePermission")]
    [InlineData(typeof(AdminUpdatePermissionMetaField), "AdminUpdatePermission", "AdminUpdatePermission")]
    [InlineData(typeof(AdminActivatePermissionMetaField), "AdminActivatePermission", "AdminActivatePermission")]
    [InlineData(typeof(AdminDeactivatePermissionMetaField), "AdminDeactivatePermission", "AdminDeactivatePermission")]
    [InlineData(typeof(AdminSoftDeletePermissionMetaField), "AdminSoftDeletePermission", "AdminSoftDeletePermission")]
    [InlineData(typeof(AdminHardDeletePermissionMetaField), "AdminHardDeletePermission", "AdminHardDeletePermission")]
    [InlineData(typeof(AdminRestorePermissionMetaField), "AdminRestorePermission", "AdminRestorePermission")]
    [InlineData(typeof(AdminGetAllPermissionsMetaField), "AdminGetAllPermissions", "AdminGetAllPermissions")]
    [InlineData(typeof(AdminGetPermissionByIdMetaField), "AdminGetPermissionById", "AdminGetPermissionById")]
    [InlineData(
        typeof(AdminAssignPermissionToRoleMetaField),
        "AdminAssignPermissionToRole",
        "AdminAssignPermissionToRole"
    )]
    [InlineData(
        typeof(AdminRemovePermissionFromRoleMetaField),
        "AdminRemovePermissionFromRole",
        "AdminRemovePermissionFromRole"
    )]
    [InlineData(
        typeof(AdminBulkUpdateRolePermissionsMetaField),
        "AdminBulkUpdateRolePermissions",
        "AdminBulkUpdateRolePermissions"
    )]
    [InlineData(typeof(AdminGetAllSessionsMetaField), "AdminGetAllSessions", "AdminGetAllSessions")]
    [InlineData(typeof(AdminGetSessionMetricsMetaField), "AdminGetSessionMetrics", "AdminGetSessionMetrics")]
    [InlineData(typeof(AdminExportSessionDataMetaField), "AdminExportSessionData", "AdminExportSessionData")]
    [InlineData(
        typeof(AdminCleanupExpiredSessionsMetaField),
        "AdminCleanupExpiredSessions",
        "AdminCleanupExpiredSessions"
    )]
    [InlineData(typeof(AdminForceLogoutUserMetaField), "AdminForceLogoutUser", "AdminForceLogoutUser")]
    [InlineData(typeof(AdminRevokeSessionMetaField), "AdminRevokeSession", "AdminRevokeSession")]
    [InlineData(typeof(AdminGetOwnSessionByIdMetaField), "AdminGetOwnSessionById", "AdminGetOwnSessionById")]
    [InlineData(typeof(AdminGetOwnSessionsMetaField), "AdminGetOwnSessions", "AdminGetOwnSessions")]
    [InlineData(typeof(AdminRefreshTokenMetaField), "AdminRefreshToken", "AdminRefreshToken")]
    [InlineData(typeof(PublicRefreshTokenMetaField), "PublicRefreshToken", "PublicRefreshToken")]
    [InlineData(typeof(PublicRevokeSessionMetaField), "PublicRevokeSession", "PublicRevokeSession")]
    [InlineData(typeof(PublicGetOwnSessionsMetaField), "PublicGetOwnSessions", "PublicGetOwnSessions")]
    [InlineData(typeof(PublicGetOwnSessionByIdMetaField), "PublicGetOwnSessionById", "PublicGetOwnSessionById")]
    [InlineData(typeof(AdminGetOwnProfileMetaField), "GetOwnProfile", "AdminGetOwnProfile")]
    [InlineData(typeof(AdminUpdateOwnProfileMetaField), "UpdateOwnProfile", "AdminUpdateOwnProfile")]
    [InlineData(typeof(AdminUpdateAvatarMetaField), "UpdateAvatar", "AdminUpdateAvatar")]
    [InlineData(typeof(PublicGetOwnProfileNS.PublicGetOwnProfileMetaField), "GetOwnProfile", "PublicGetOwnProfile")]
    [InlineData(
        typeof(PublicUpdateOwnProfileNS.PublicUpdateOwnProfileMetaField),
        "UpdateOwnProfile",
        "PublicUpdateOwnProfile"
    )]
    [InlineData(typeof(PublicUpdateAvatarNS.PublicUpdateAvatarMetaField), "UpdateAvatar", "PublicUpdateAvatar")]
    [InlineData(typeof(AdminAssignRoleToUserMetaField), "AdminAssignRoleToUser", "AdminAssignRoleToUser")]
    [InlineData(typeof(AdminRemoveRoleFromUserMetaField), "AdminRemoveRoleFromUser", "AdminRemoveRoleFromUser")]
    [InlineData(typeof(AdminGetUserRolesMetaField), "AdminGetUserRoles", "AdminGetUserRoles")]
    public void MetaField_ShouldHaveValidRouteMetadata(Type metaFieldType, string fieldName, string expectedName)
    {
        // Arrange & Act
        FieldInfo? field = metaFieldType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        // Assert
        field.Should().NotBeNull();

        var metadata = field.GetValue(null) as RouteMetadata?;
        metadata.Should().NotBeNull();

        metadata.Value.Name.Should().Be(expectedName);
        metadata.Value.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Value.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(typeof(AdminLoginMetaField), "AdminLogin")]
    [InlineData(typeof(PublicLoginMetaField), "PublicLogin")]
    [InlineData(typeof(AdminCreateRoleMetaField), "AdminCreateRole")]
    [InlineData(typeof(AdminUpdateRoleMetaField), "AdminUpdateRole")]
    [InlineData(typeof(PublicSignUpMetaField), "PublicSignUp")]
    [InlineData(typeof(PublicSocialLoginMetaField), "SocialLogin")]
    [InlineData(typeof(AdminGetAllSessionsMetaField), "AdminGetAllSessions")]
    [InlineData(typeof(PublicGetOwnSessionsMetaField), "PublicGetOwnSessions")]
    public void MetaField_Summary_ShouldNotBeEmpty(Type metaFieldType, string fieldName)
    {
        // Arrange & Act
        FieldInfo? field = metaFieldType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        var metadata = field!.GetValue(null) as RouteMetadata?;

        // Assert
        metadata.Should().NotBeNull();
        metadata.Value.Summary.Should().NotBeEmpty();
        (metadata.Value.Summary.Length > 10).Should().BeTrue("Summary should be descriptive");
    }

    [Theory]
    [InlineData(typeof(AdminLoginMetaField), "AdminLogin")]
    [InlineData(typeof(PublicLoginMetaField), "PublicLogin")]
    [InlineData(typeof(AdminCreateRoleMetaField), "AdminCreateRole")]
    [InlineData(typeof(PublicSignUpMetaField), "PublicSignUp")]
    public void MetaField_Description_ShouldContainUsefulInformation(Type metaFieldType, string fieldName)
    {
        // Arrange & Act
        FieldInfo? field = metaFieldType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        var metadata = field!.GetValue(null) as RouteMetadata?;

        // Assert
        metadata.Should().NotBeNull();
        metadata.Value.Description.Should().NotBeEmpty();
        (metadata.Value.Description.Length > 50).Should().BeTrue("Description should be detailed");
    }

    [Fact]
    public void AllMetaFieldClasses_ShouldBeStatic()
    {
        // Arrange
        Type[] metaFieldTypes =
        [
            typeof(AdminLoginMetaField),
            typeof(PublicLoginMetaField),
            typeof(AdminCreateRoleMetaField),
            typeof(AdminUpdateRoleMetaField),
            typeof(PublicSignUpMetaField),
            typeof(PublicSocialLoginMetaField),
            typeof(AdminGetAllSessionsMetaField),
            typeof(PublicGetOwnSessionsMetaField),
        ];

        // Act & Assert
        foreach (Type type in metaFieldTypes)
        {
            (type.IsAbstract && type.IsSealed).Should().BeTrue($"{type.Name} should be a static class");
        }
    }

    [Fact]
    public void AllMetaFieldFields_ShouldBePublicStaticReadonly()
    {
        // Arrange
        (Type, string)[] testCases =
        [
            (typeof(AdminLoginMetaField), "AdminLogin"),
            (typeof(PublicLoginMetaField), "PublicLogin"),
            (typeof(AdminCreateRoleMetaField), "AdminCreateRole"),
            (typeof(PublicSignUpMetaField), "PublicSignUp"),
        ];

        // Act & Assert
        foreach (var (type, fieldName) in testCases)
        {
            FieldInfo? field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

            field.Should().NotBeNull();
            field.IsPublic.Should().BeTrue($"{type.Name}.{fieldName} should be public");
            field.IsStatic.Should().BeTrue($"{type.Name}.{fieldName} should be static");
            field.IsInitOnly.Should().BeTrue($"{type.Name}.{fieldName} should be readonly");
        }
    }
}
