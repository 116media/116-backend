using _116.BuildingBlocks.Constants;
using _116.Content.Application.Commerce.OutboundEmails;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Default <see cref="ICommerceCustomerNotifier" /> implementation over the
/// message dispatcher. Customer emails render in the neutral culture: B2B customer
/// records carry no language preference, and guessing from the admin's request
/// culture would localize by the wrong person.
/// </summary>
/// <param name="messageDispatcher">Dispatcher routing each message to its recipients.</param>
/// <param name="customerRepository">Repository resolving customers by id.</param>
/// <param name="categoryRepository">Repository resolving the item categories named in the invoice.</param>
public class CommerceCustomerNotifier(
    IEmailDispatcher messageDispatcher,
    ICustomerRepository customerRepository,
    ICategoryRepository categoryRepository
) : ICommerceCustomerNotifier
{
    /// <summary>
    /// The locale customer emails render in. B2B customer records carry no language
    /// preference, so they take the platform default rather than the acting admin's culture.
    /// </summary>
    private const string CustomerCulture = UserConstants.DefaultLocale;

    /// <summary>
    /// Resolves a customer row to the recipient the message renders for.
    /// </summary>
    /// <param name="customer">The customer receiving the message.</param>
    /// <returns>The recipient, always in the platform default locale.</returns>
    private static EmailRecipient RecipientFor(CustomerEntity customer)
    {
        return new EmailRecipient(
            UserId: null,
            Address: customer.Email,
            DisplayName: customer.FullName,
            Locale: CustomerCulture
        );
    }

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

        var message = new OrderInvoiceEmail(
            Customer: RecipientFor(customer),
            OrderReference: OrderReference(order.Id),
            AmountUsd: FormatAmount(order.TotalAmountUsd),
            PaymentMethods: PaymentMethods(),
            ItemSummary: ItemSummary(order, categories)
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new PaymentReceiptEmail(
            Customer: RecipientFor(customer),
            OrderReference: OrderReference(order.Id),
            AmountUsd: FormatAmount(payment.AmountUsd),
            ReceiptUrl: payment.ReceiptUrl ?? string.Empty,
            PaidAt: DateTimeOffset.UtcNow
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new PaymentRejectedEmail(
            Customer: RecipientFor(customer),
            OrderReference: OrderReference(order.Id),
            Notes: notes ?? string.Empty
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new OrderCancelledEmail(
            Customer: RecipientFor(customer),
            OrderReference: OrderReference(order.Id)
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new PromotionRemovedEmail(
            Customer: RecipientFor(customer),
            ContentTitle: contentTitle,
            Reason: reason,
            RemovedAt: DateTimeOffset.UtcNow
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new CommissionedContentPublishedEmail(
            Customer: RecipientFor(customer),
            ContentTitle: contentTitle,
            PublicUrl: publicUrl
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new CommissionedContentRejectedEmail(
            Customer: RecipientFor(customer),
            ContentTitle: contentTitle,
            Reason: reason
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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

        var message = new ShootScheduledEmail(
            Customer: RecipientFor(customer),
            ContentTitle: contentTitle,
            ShootDate: shootDate
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
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
