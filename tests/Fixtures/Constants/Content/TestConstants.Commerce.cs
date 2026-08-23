namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Commerce entity testing (orders, payments, items, tiers). Amounts and
    /// free-text notes are per-row data with no production counterpart.
    /// </summary>
    public static class Commerce
    {
        /// <summary>
        /// Test-owned fixture tier line amount.
        /// </summary>
        public const decimal ValidTierPriceUsd = 100.00m;

        /// <summary>
        /// Test-owned fixture promotion line amount.
        /// </summary>
        public const decimal ValidPromoPriceUsd = 50.00m;

        /// <summary>
        /// Test-owned fixture order total, the sum of the two line amounts above.
        /// </summary>
        public const decimal ValidTotalAmountUsd = 150.00m;

        /// <summary>
        /// Test-owned fixture payment receipt URL.
        /// </summary>
        public const string ValidReceiptUrl = "https://receipts.example.com/pay-123.pdf";

        /// <summary>
        /// Test-owned fixture payment rejection note.
        /// </summary>
        public const string ValidRejectionNotes = "Payment proof is not legible, please resubmit.";
    }
}
