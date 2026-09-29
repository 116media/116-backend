namespace _116.Content.Domain.Enums;

/// <summary>
/// Represents the payment method used by the customer to settle a content order.
/// Stored as a <c>string</c> in the database via EF Core's <c>.HasConversion&lt;string&gt;()</c>.
/// </summary>
public enum EnumPaymentMethod
{
    /// <summary>
    /// Payment via bank wire or direct bank transfer.
    /// </summary>
    BankTransfer,

    /// <summary>
    /// Payment via mobile money platform (e.g., M-Pesa, Orange Money, Airtel Money).
    /// </summary>
    MobileMoney,

    /// <summary>
    /// Cash payment delivered in person.
    /// </summary>
    Cash,
}
