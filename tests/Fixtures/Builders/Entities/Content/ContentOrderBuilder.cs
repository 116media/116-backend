using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentOrderEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; ContentOrderFactory only names chains three or more tests share.
/// </summary>
public class ContentOrderBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _customerId = Guid.NewGuid();
    private Guid? _packageId;
    private bool _submitted;
    private bool _paid;
    private bool _cancelled;
    private CustomerEntity? _customer;
    private ContentPaymentEntity? _payment;

    public ContentOrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the package the order was created from.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ContentOrderBuilder WithPackageId(Guid packageId)
    {
        _packageId = packageId;
        return this;
    }

    public ContentOrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
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

    /// <summary>
    /// Attaches the Customer navigation EF Core populates through <c>.Include(o =&gt; o.Customer)</c>,
    /// and points the foreign key at the same customer.
    /// </summary>
    public ContentOrderBuilder WithCustomer(CustomerEntity customer)
    {
        _customer = customer;
        _customerId = customer.Id;
        return this;
    }

    /// <summary>
    /// Attaches the Payment navigation EF Core populates through <c>.Include(o =&gt; o.Payment)</c>.
    /// </summary>
    public ContentOrderBuilder WithPayment(ContentPaymentEntity payment)
    {
        _payment = payment;
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

        if (_customer is not null)
        {
            typeof(ContentOrderEntity)
                .GetProperty(nameof(ContentOrderEntity.Customer), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(order, _customer);
        }

        if (_payment is not null)
        {
            typeof(ContentOrderEntity)
                .GetProperty(nameof(ContentOrderEntity.Payment), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(order, _payment);
        }

        return order;
    }
}
