using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.DTOs;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for ContentOrder, ContentOrderItem, ContentItemTier, and ContentPayment entity mappings.
/// </summary>
public static class ContentOrderMapper
{
    /// <summary>
    /// Registers ContentOrder entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<ContentItemTierEntity, ItemTierDto>()
            .Map(dest => dest.TierName, _ => string.Empty)
            .Map(dest => dest.PriceSnapshotUsd, src => src.PriceSnapshotUsd);

        config
            .NewConfig<ContentOrderItemEntity, OrderItemDto>()
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryName, _ => string.Empty)
            .Map(dest => dest.PromotionLevelId, src => src.PromotionLevelId)
            .Map(dest => dest.PromotionLevelName, _ => (string?)null)
            .Map(
                dest => dest.PromoPriceUsd,
                src => src.PromoPriceSnapshotUsd == null ? null : (decimal?)src.PromoPriceSnapshotUsd.Amount
            )
            .Map(dest => dest.Tiers, src => src.Tiers);

        config
            .NewConfig<ContentPaymentEntity, PaymentDto>()
            .Map(dest => dest.PaymentProof, _ => (FileDto?)null)
            .Map(dest => dest.VerifiedBy, src => src.VerifiedById)
            .Map(dest => dest.VerifiedByUserName, _ => (string?)null);

        config
            .NewConfig<ContentPaymentEntity, PaymentSummaryDto>()
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.CustomerName, _ => string.Empty)
            .Map(dest => dest.OrderStatus, _ => EnumOrderStatus.Draft)
            .Map(dest => dest.VerifiedBy, src => src.VerifiedById)
            .Map(dest => dest.VerifiedByUserName, _ => (string?)null);

        config
            .NewConfig<ContentOrderEntity, ContentOrderSummaryDto>()
            .Map(dest => dest.CustomerName, _ => string.Empty)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config
            .NewConfig<ContentOrderEntity, ContentOrderDetailDto>()
            .Map(dest => dest.CustomerId, src => src.CustomerId)
            .Map(dest => dest.CustomerName, _ => string.Empty)
            .Map(dest => dest.PackageId, src => src.PackageId)
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.Payment, src => src.Payment);
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderEntity" /> to a <see cref="ContentOrderSummaryDto" />, reading
    /// the customer name from a pre-fetched map. Performs no IO.
    /// </summary>
    public static ContentOrderSummaryDto ToContentOrderSummaryDto(
        this ContentOrderEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, CustomerEntity> customers
    )
    {
        var dto = mapper.Map<ContentOrderSummaryDto>(entity);
        return dto with { CustomerName = CustomerName(entity.CustomerId, customers), ItemCount = entity.Items.Count };
    }

    /// <summary>
    /// Maps a collection of <see cref="ContentOrderEntity" /> to a list of <see cref="ContentOrderSummaryDto" />.
    /// </summary>
    public static IReadOnlyList<ContentOrderSummaryDto> ToContentOrderSummaryDtos(
        this IReadOnlyList<ContentOrderEntity> entities,
        IMapper mapper,
        IReadOnlyDictionary<Guid, CustomerEntity> customers
    )
    {
        return entities.Select(e => e.ToContentOrderSummaryDto(mapper, customers)).ToList();
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderEntity" /> to a <see cref="ContentOrderDetailDto" />, reading
    /// the customer, category, promotion level and tier names from pre-fetched maps. Performs no IO.
    /// </summary>
    public static ContentOrderDetailDto ToContentOrderDetailDto(
        this ContentOrderEntity entity,
        IMapper mapper,
        OrderLookups lookups
    )
    {
        var dto = mapper.Map<ContentOrderDetailDto>(entity);
        return dto with
        {
            CustomerName = CustomerName(entity.CustomerId, lookups.Customers),
            Items = entity.Items.Select(i => i.ToOrderItemDto(mapper, lookups)).ToList(),
            Payment = entity.Payment != null ? mapper.Map<PaymentDto>(entity.Payment) : null,
        };
    }

    /// <summary>
    /// Reads a customer's display name out of a resolved map, falling back to an empty name when
    /// the customer row is gone.
    /// </summary>
    private static string CustomerName(Guid customerId, IReadOnlyDictionary<Guid, CustomerEntity> customers)
    {
        return customers.TryGetValue(customerId, out CustomerEntity? customer) ? customer.FullName : string.Empty;
    }

    /// <summary>
    /// Maps the payment carried by an order to a <see cref="PaymentSummaryDto" />, taking the
    /// customer name and order status from the order itself and the verifier's name from a
    /// pre-fetched map. Performs no IO — batch mappings resolve users up front.
    /// </summary>
    public static PaymentSummaryDto ToPaymentSummaryDto(
        this ContentOrderEntity order,
        IMapper mapper,
        IReadOnlyDictionary<Guid, AuthorDto> verifiers,
        IReadOnlyDictionary<Guid, CustomerEntity> customers
    )
    {
        ContentPaymentEntity payment = order.Payment!;
        var dto = mapper.Map<PaymentSummaryDto>(payment);
        string? verifiedByUserName =
            payment.VerifiedById is { } verifierId && verifiers.TryGetValue(verifierId, out AuthorDto? verifier)
                ? verifier.UserName
                : null;

        return dto with
        {
            OrderId = order.Id,
            CustomerName = CustomerName(order.CustomerId, customers),
            OrderStatus = order.Status,
            VerifiedBy = payment.VerifiedById,
            VerifiedByUserName = verifiedByUserName,
        };
    }

    /// <summary>
    /// Maps a <see cref="ContentPaymentEntity" /> to a <see cref="PaymentDto" />, injecting the
    /// resolved proof file and reading the verifier's name from a pre-fetched map. Performs no IO.
    /// </summary>
    public static PaymentDto ToPaymentDto(
        this ContentPaymentEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, AuthorDto> verifiers,
        FileDto? proofFile = null
    )
    {
        var dto = mapper.Map<PaymentDto>(entity);
        string? verifiedByUserName =
            entity.VerifiedById is { } verifierId && verifiers.TryGetValue(verifierId, out AuthorDto? verifier)
                ? verifier.UserName
                : null;

        return dto with
        {
            PaymentProof = proofFile,
            VerifiedByUserName = verifiedByUserName,
        };
    }

    /// <summary>
    /// Maps a <see cref="FileReferenceDto" /> to a <see cref="FileDto" />, or returns null if the entity is null.
    /// </summary>
    public static FileDto? ToFileDto(this FileReferenceDto? fileEntity, IMapper mapper)
    {
        return fileEntity == null ? null : mapper.Map<FileDto>(fileEntity);
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderItemEntity" /> to an <see cref="OrderItemDto" />, reading the
    /// category, promotion level and tier names from pre-fetched maps. Performs no IO.
    /// </summary>
    public static OrderItemDto ToOrderItemDto(this ContentOrderItemEntity entity, IMapper mapper, OrderLookups lookups)
    {
        var dto = mapper.Map<OrderItemDto>(entity);
        string? promotionLevelName =
            entity.PromotionLevelId is { } promotionLevelId
            && lookups.PromotionLevels.TryGetValue(promotionLevelId, out PromotionLevelEntity? promotionLevel)
                ? promotionLevel.Name
                : null;

        return dto with
        {
            CategoryName = lookups.Categories.TryGetValue(entity.CategoryId, out CategoryEntity? category)
                ? category.Name
                : string.Empty,
            PromotionLevelName = promotionLevelName,
            Tiers = entity.Tiers.Select(tier => tier.ToItemTierDto(mapper, lookups.PricingTiers)).ToList(),
            PromoPriceUsd = entity.PromoPriceSnapshotUsd?.Amount,
        };
    }

    /// <summary>
    /// Maps a <see cref="ContentItemTierEntity" /> to an <see cref="ItemTierDto" />, reading the
    /// tier name from a pre-fetched map. Performs no IO.
    /// </summary>
    public static ItemTierDto ToItemTierDto(
        this ContentItemTierEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, PricingTierEntity> pricingTiers
    )
    {
        var dto = mapper.Map<ItemTierDto>(entity);
        return dto with
        {
            TierName = pricingTiers.TryGetValue(entity.PricingTierId, out PricingTierEntity? pricingTier)
                ? pricingTier.Name
                : string.Empty,
        };
    }
}
