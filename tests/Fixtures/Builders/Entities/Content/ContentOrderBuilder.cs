using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentOrderEntity"/> instances in tests.
/// For test code, prefer using ContentOrderFactory instead of direct Builder usage.
/// </summary>
internal class ContentOrderBuilder
{
    private Guid _id;
    private Guid _customerId;
    private Guid? _packageId;
    private bool _submitted;
    private bool _paid;
    private bool _cancelled;

    public ContentOrderBuilder()
    {
        _id = Guid.NewGuid();
        _customerId = Guid.NewGuid();
    }

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
        ContentOrderEntity order = ContentOrderEntity.Create(_id, _customerId, _packageId);

        if (_submitted)
        {
            order.Submit();
        }

        if (_paid)
        {
            order.MarkPaid();
        }

        if (_cancelled)
        {
            order.Cancel();
        }

        return order;
    }
}
