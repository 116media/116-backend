using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Customer entity testing.
    /// </summary>
    public static class Customer
    {
        /// <summary>
        /// The production maximum customer full name length.
        /// </summary>
        public const int FullNameMaxLength = ContentConstants.MaxCustomerFullNameLength;

        /// <summary>
        /// The production maximum customer email length. Commerce customers are not
        /// identity users, so this is a separate limit from the account email one.
        /// </summary>
        public const int EmailMaxLength = ContentConstants.MaxCustomerEmailLength;

        /// <summary>
        /// The production maximum customer phone length.
        /// </summary>
        public const int PhoneMaxLength = ContentConstants.MaxCustomerPhoneLength;

        /// <summary>
        /// The production maximum customer company length.
        /// </summary>
        public const int CompanyMaxLength = ContentConstants.MaxCustomerCompanyLength;

        /// <summary>
        /// The production maximum customer notes length.
        /// </summary>
        public const int NotesMaxLength = ContentConstants.MaxCustomerNotesLength;

        /// <summary>
        /// Test-owned fixture customer name.
        /// </summary>
        public const string ValidFullName = "John Doe";

        /// <summary>
        /// Test-owned fixture customer email.
        /// </summary>
        public const string ValidEmail = "customer@example.com";

        /// <summary>
        /// Test-owned second fixture customer email, for uniqueness assertions.
        /// </summary>
        public const string AnotherValidEmail = "other@example.com";

        /// <summary>
        /// Test-owned fixture customer phone number.
        /// </summary>
        public const string ValidPhone = "+243812345678";

        /// <summary>
        /// Test-owned fixture customer company name.
        /// </summary>
        public const string ValidCompany = "Acme Music Label";

        /// <summary>
        /// Test-owned fixture customer notes.
        /// </summary>
        public const string ValidNotes = "VIP customer with special pricing.";
    }
}
