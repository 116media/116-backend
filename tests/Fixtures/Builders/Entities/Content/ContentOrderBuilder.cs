using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentOrderEntity"/> instances in tests.
/// For test code, prefer using ContentOrderFactory instead of direct Builder usage.
/// </summary>
internal class ContentOrderBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _customerId = Guid.NewGuid();
    private Guid? _packageId;
    private bool _submitted;
    private bool _paid;
    private bool _cancelled;

    public ContentOrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ContentOrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public ContentOrderBuilder WithPackageId(Guid? packageId)
    {
        _packageId = packageId;
        return this;
    }

    public ContentOrderBuilder AsSubmitted()
    {
        _submitted = true;
        return this;
    }

    public ContentOrderBuilder AsPaid()
    {
        _submitted = true;
        _paid = true;
        return this;
    }

    public ContentOrderBuilder AsCancelled()
    {
        _cancelled = true;
        return this;
    }

    public ContentOrderEntity Build()
    {
        var errors = TestErrorsFactory.CreateContentOrderErrors();
        var order = ContentOrderEntity.Create(_id, _customerId, _packageId);

        if (_submitted)
        {
            order.Submit(errors);
        }

        if (_paid)
        {
            order.MarkPaid(
                paymentId: Guid.NewGuid(),
                verifiedAt: DateTimeOffset.UtcNow,
                promotionDurationsByLevelId: new Dictionary<Guid, int>(),
                errors: errors
            );
        }

        if (_cancelled)
        {
            order.Cancel(errors);
        }

        return order;
    }
}
