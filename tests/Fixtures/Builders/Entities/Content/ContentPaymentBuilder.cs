using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentPaymentEntity"/> instances in tests.
/// For test code, prefer using ContentPaymentFactory instead of direct Builder usage.
/// </summary>
internal class ContentPaymentBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _orderId = Guid.NewGuid();
    private decimal _amountUsd = TestConstants.Content.Commerce.ValidTotalAmountUsd;
    private Guid? _proofFileId;
    private EnumPaymentMethod? _paymentMethod;
    private bool _verified;
    private Guid _verifiedByAdminId = Guid.NewGuid();
    private string _receiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl;
    private bool _rejected;
    private string? _rejectionNotes;

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

    public ContentPaymentEntity Build()
    {
        var payment = ContentPaymentEntity.Create(_id, _orderId, _amountUsd);

        if (_proofFileId.HasValue && _paymentMethod.HasValue)
        {
            payment.AttachProof(_proofFileId.Value, _paymentMethod.Value);
        }

        if (_verified)
        {
            payment.Verify(_verifiedByAdminId, _receiptUrl);
        }

        if (_rejected)
        {
            payment.Reject(_rejectionNotes);
        }

        return payment;
    }
}
