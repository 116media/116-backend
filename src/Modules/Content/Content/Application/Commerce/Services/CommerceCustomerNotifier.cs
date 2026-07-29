using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Mailer.Contracts.Application;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Default <see cref="ICommerceCustomerNotifier" /> implementation over the
/// outbox mailer. Customer emails render in the neutral culture: B2B customer
/// records carry no language preference, and guessing from the admin's request
/// culture would localize by the wrong person.
/// </summary>
/// <param name="mailer">The outbox mailer.</param>
/// <param name="customerRepository">Repository resolving customers by id.</param>
public class CommerceCustomerNotifier(IMailer mailer, ICustomerRepository customerRepository)
    : ICommerceCustomerNotifier
{
    /// <summary>
    /// The culture customer emails render in; neutral resources are the
    /// English source of truth.
    /// </summary>
    private const string CustomerCulture = "en";

    /// <inheritdoc />
    public async Task NotifyOrderInvoiceAsync(ContentOrderEntity order, CancellationToken cancellationToken)
    {
        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.OrderInvoice,
            to: new EmailRecipient(Address: order.Customer.Email, DisplayName: order.Customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = order.Customer.FullName,
                ["orderReference"] = OrderReference(order.Id),
                ["amountUsd"] = FormatAmount(order.TotalAmountUsd),
                ["paymentMethods"] = PaymentMethods(),
                ["itemSummary"] = ItemSummary(order),
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyPaymentReceiptAsync(
        ContentOrderEntity order,
        ContentPaymentEntity payment,
        CancellationToken cancellationToken
    )
    {
        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.PaymentReceipt,
            to: new EmailRecipient(Address: order.Customer.Email, DisplayName: order.Customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = order.Customer.FullName,
                ["orderReference"] = OrderReference(order.Id),
                ["amountUsd"] = FormatAmount(payment.AmountUsd),
                ["receiptUrl"] = payment.ReceiptUrl ?? string.Empty,
                ["paidAt"] = DateTime.UtcNow.ToString("u"),
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyPaymentRejectedAsync(
        ContentOrderEntity order,
        string notes,
        CancellationToken cancellationToken
    )
    {
        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.PaymentRejected,
            to: new EmailRecipient(Address: order.Customer.Email, DisplayName: order.Customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = order.Customer.FullName,
                ["orderReference"] = OrderReference(order.Id),
                ["notes"] = notes,
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyOrderCancelledAsync(ContentOrderEntity order, CancellationToken cancellationToken)
    {
        CustomerEntity? customer = await customerRepository.GetByIdAsync(
            id: order.CustomerId,
            cancellationToken: cancellationToken
        );

        if (customer is null)
        {
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.OrderCancelled,
            to: new EmailRecipient(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["orderReference"] = OrderReference(order.Id),
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyPromotionRemovedAsync(
        Guid? customerId,
        string contentTitle,
        string reason,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity? customer = await ResolveAsync(customerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.PromotionForceRemoved,
            to: new EmailRecipient(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["contentTitle"] = contentTitle,
                ["reason"] = reason,
                ["removedAt"] = DateTime.UtcNow.ToString("u"),
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyContentPublishedAsync(
        Guid? customerId,
        string contentTitle,
        string publicUrl,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity? customer = await ResolveAsync(customerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.CommissionedContentPublished,
            to: new EmailRecipient(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["contentTitle"] = contentTitle,
                ["publicUrl"] = publicUrl,
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyContentRejectedAsync(
        Guid? customerId,
        string contentTitle,
        string reason,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity? customer = await ResolveAsync(customerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.CommissionedContentRejected,
            to: new EmailRecipient(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["contentTitle"] = contentTitle,
                ["reason"] = reason,
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task NotifyShootScheduledAsync(
        Guid? customerId,
        string contentTitle,
        DateTime shootDate,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity? customer = await ResolveAsync(customerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.ShootScheduled,
            to: new EmailRecipient(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["contentTitle"] = contentTitle,
                ["shootDate"] = shootDate.ToString("u"),
            },
            culture: CustomerCulture,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Resolves an optional customer id to its entity; null in, null out.
    /// </summary>
    private async Task<CustomerEntity?> ResolveAsync(Guid? customerId, CancellationToken cancellationToken)
    {
        if (customerId is null)
        {
            return null;
        }

        return await customerRepository.GetByIdAsync(id: customerId.Value, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Short human-readable order reference: the first eight hex characters of
    /// the order id, uppercased.
    /// </summary>
    internal static string OrderReference(Guid orderId)
    {
        return orderId.ToString("N")[..8].ToUpperInvariant();
    }

    /// <summary>
    /// Formats a USD amount with two decimals, invariant.
    /// </summary>
    internal static string FormatAmount(decimal amountUsd)
    {
        return amountUsd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Lists the item categories on the order, or the item count when a
    /// category navigation is not loaded.
    /// </summary>
    internal static string ItemSummary(ContentOrderEntity order)
    {
        List<string> names = [.. order.Items.Where(i => i.Category is not null).Select(i => i.Category.Name)];

        return names.Count == order.Items.Count && names.Count > 0
            ? string.Join(", ", names)
            : $"{order.Items.Count} item(s)";
    }

    /// <summary>
    /// The accepted offline payment methods, from the enum so the list can
    /// never drift from the model.
    /// </summary>
    internal static string PaymentMethods()
    {
        return string.Join(", ", Enum.GetNames<EnumPaymentMethod>());
    }
}
