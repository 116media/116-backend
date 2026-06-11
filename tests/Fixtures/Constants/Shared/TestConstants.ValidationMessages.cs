namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for validation error messages testing.
    /// These will be replaced by <c>LocalizerFactory</c> once Track B (i18n) is implemented.
    /// </summary>
    public static class ValidationMessages
    {
        public const string RequiredField = "is required";
        public const string InvalidFormat = "is not valid";
        public const string TooShort = "too short";
        public const string TooLong = "too long";
        public const string AlreadyExists = "already exists";
        public const string NotFound = "not found";

        /// <summary>
        /// Role-specific validation error messages.
        /// </summary>
        public static class Role
        {
            public const string NameRequired = "Role name is required.";
            public const string NameTooLong = "Role name cannot exceed 20 characters.";
            public const string DescriptionRequired = "Role description is required.";
            public const string DescriptionTooLong = "Role description cannot exceed 300 characters.";
        }

        /// <summary>
        /// Permission-specific validation error messages.
        /// </summary>
        public static class Permission
        {
            public const string ResourceRequired = "Permission resource is required.";
            public const string ResourceTooLong = "Permission resource cannot exceed 15 characters.";
            public const string ActionRequired = "Permission action is required.";
            public const string ActionTooLong = "Permission action cannot exceed 15 characters.";
            public const string DescriptionRequired = "Permission description is required.";
            public const string DescriptionTooLong = "Permission description cannot exceed 300 characters.";
        }

        /// <summary>
        /// GUID validation error messages.
        /// </summary>
        public static class Guid
        {
            public const string RoleIdRequired = "Role ID is required.";
            public const string RoleIdInvalid = "Role ID is invalid.";
            public const string PermissionIdRequired = "Permission ID is required.";
            public const string PermissionIdInvalid = "Permission ID is invalid.";
            public const string UserIdRequired = "User ID is required.";
            public const string UserIdInvalid = "User ID is invalid.";
        }
    }
}
