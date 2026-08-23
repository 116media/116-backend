using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentPaymentEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; ContentPaymentFactory only names chains three or more tests share.
/// </summary>
public class ContentPaymentBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _orderId = Guid.NewGuid();
    private decimal _amountUsd = TestConstants.Commerce.ValidTotalAmountUsd;
    private Guid? _proofFileId;
    private EnumPaymentMethod? _paymentMethod;
    private bool _verified;
    private Guid _verifiedByAdminId = Guid.NewGuid();
    private string _receiptUrl = TestConstants.Commerce.ValidReceiptUrl;
    private bool _rejected;
    private string? _rejectionNotes;
    private ContentOrderEntity? _order;

    public ContentPaymentBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ContentPaymentBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public ContentPaymentBuilder WithAmountUsd(decimal amountUsd)
    {
        _amountUsd = amountUsd;
        return this;
    }

    public ContentPaymentBuilder WithProofFileId(Guid proofFileId, EnumPaymentMethod paymentMethod)
    {
        _proofFileId = proofFileId;
        _paymentMethod = paymentMethod;
        return this;
    }

    public ContentPaymentBuilder AsVerified(Guid adminUserId, string receiptUrl)
    {
        _verified = true;
        _verifiedByAdminId = adminUserId;
        _receiptUrl = receiptUrl;
        return this;
    }

    public ContentPaymentBuilder AsRejected(string? notes = null)
    {
        _rejected = true;
        _rejectionNotes = notes;
        return this;
    }

    /// <summary>
    /// Attaches the Order navigation EF Core populates through <c>.Include(p =&gt; p.Order)</c>,
    /// and points the foreign key at the same order.
    /// </summary>
    public ContentPaymentBuilder WithOrder(ContentOrderEntity order)
    {
        _order = order;
        _orderId = order.Id;
        return this;
    }

    public ContentPaymentEntity Build()
    {
        var errors = TestErrorsFactory.CreateContentOrderErrors();
        var payment = ContentPaymentEntity.Create(_id, _orderId, _amountUsd);

        if (_proofFileId.HasValue && _paymentMethod.HasValue)
        {
            payment.AttachProof(_proofFileId.Value, _paymentMethod.Value);
        }

        if (_verified)
        {
            payment.Verify(_verifiedByAdminId, _receiptUrl, errors);
        }

        if (_rejected)
        {
            payment.Reject(_rejectionNotes, errors);
        }

        if (_order is not null)
        {
            typeof(ContentPaymentEntity)
                .GetProperty(nameof(ContentPaymentEntity.Order), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(payment, _order);
        }

        return payment;
    }
}
