using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Default <see cref="ICommerceCustomerNotifier" /> implementation over the
/// outbox mailer. Customer emails render in the neutral culture: B2B customer
/// records carry no language preference, and guessing from the admin's request
/// culture would localize by the wrong person.
/// </summary>
/// <param name="emailService">The outbox mailer.</param>
/// <param name="customerRepository">Repository resolving customers by id.</param>
/// <param name="categoryRepository">Repository resolving the item categories named in the invoice.</param>
public class CommerceCustomerNotifier(
    IEmailService emailService,
    ICustomerRepository customerRepository,
    ICategoryRepository categoryRepository
) : ICommerceCustomerNotifier
{
    /// <summary>
    /// The culture customer emails render in; neutral resources are the
    /// English source of truth.
    /// </summary>
    private const string CustomerCulture = "en";

    /// <inheritdoc />
    public async Task NotifyOrderInvoiceAsync(ContentOrderEntity order, CancellationToken cancellationToken)
    {
        CustomerEntity? customer = await ResolveAsync(order.CustomerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        IReadOnlyDictionary<Guid, CategoryEntity> categories = await categoryRepository.GetByIdsAsync(
            ids: order.Items.Select(item => item.CategoryId).Distinct().ToList(),
            cancellationToken: cancellationToken
        );

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.OrderInvoice,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["orderReference"] = OrderReference(order.Id),
                ["amountUsd"] = FormatAmount(order.TotalAmountUsd),
                ["paymentMethods"] = PaymentMethods(),
                ["itemSummary"] = ItemSummary(order, categories),
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
        CustomerEntity? customer = await ResolveAsync(order.CustomerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.PaymentReceipt,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
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
        string? notes,
        CancellationToken cancellationToken
    )
    {
        CustomerEntity? customer = await ResolveAsync(order.CustomerId, cancellationToken);

        if (customer is null)
        {
            return;
        }

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.PaymentRejected,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
            tokens: new Dictionary<string, string>
            {
                ["customerName"] = customer.FullName,
                ["orderReference"] = OrderReference(order.Id),
                ["notes"] = notes ?? string.Empty,
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

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.OrderCancelled,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
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

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.PromotionForceRemoved,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
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

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.CommissionedContentPublished,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
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

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.CommissionedContentRejected,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
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

        await emailService.EnqueueAsync(
            template: EnumEmailTemplate.ShootScheduled,
            to: new EmailRecipientDto(Address: customer.Email, DisplayName: customer.FullName),
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
    /// Lists the item categories on the order, or the item count when a category row
    /// could not be resolved.
    /// </summary>
    internal static string ItemSummary(ContentOrderEntity order, IReadOnlyDictionary<Guid, CategoryEntity> categories)
    {
        List<string> names =
        [
            .. order
                .Items.Select(item => categories.GetValueOrDefault(item.CategoryId))
                .OfType<CategoryEntity>()
                .Select(category => category.Name),
        ];

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
