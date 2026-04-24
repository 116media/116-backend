using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.DTOs;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
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
            .Map(dest => dest.TierName, src => src.PricingTier.Name)
            .Map(dest => dest.PriceSnapshotUsd, src => src.PriceSnapshotUsd);

        config
            .NewConfig<ContentOrderItemEntity, OrderItemDto>()
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.PromotionLevelId, src => src.PromotionLevelId)
            .Map(dest => dest.PromotionLevelName, src => src.PromotionLevel != null ? src.PromotionLevel.Name : null)
            .Map(dest => dest.PromoPriceUsd, src => src.PromoPriceSnapshotUsd)
            .Map(dest => dest.Tiers, src => src.Tiers);

        config
            .NewConfig<ContentPaymentEntity, PaymentDto>()
            .Map(dest => dest.PaymentProof, _ => (FileDto?)null)
            .Map(dest => dest.VerifiedBy, src => src.VerifiedById)
            .Map(dest => dest.VerifiedByUserName, _ => (string?)null);

        config
            .NewConfig<ContentPaymentEntity, PaymentSummaryDto>()
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.CustomerName, src => src.Order.Customer.FullName)
            .Map(dest => dest.OrderStatus, src => src.Order.Status)
            .Map(dest => dest.VerifiedBy, src => src.VerifiedById)
            .Map(dest => dest.VerifiedByUserName, _ => (string?)null);

        config
            .NewConfig<ContentOrderEntity, ContentOrderSummaryDto>()
            .Map(dest => dest.CustomerName, src => src.Customer.FullName)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config
            .NewConfig<ContentOrderEntity, ContentOrderDetailDto>()
            .Map(dest => dest.CustomerId, src => src.CustomerId)
            .Map(dest => dest.CustomerName, src => src.Customer.FullName)
            .Map(dest => dest.PackageId, src => src.PackageId)
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.Payment, src => src.Payment);
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderEntity" /> to a <see cref="ContentOrderSummaryDto" />.
    /// </summary>
    public static ContentOrderSummaryDto ToContentOrderSummaryDto(this ContentOrderEntity entity, IMapper mapper)
    {
        var dto = mapper.Map<ContentOrderSummaryDto>(entity);
        return dto with { CustomerName = entity.Customer.FullName, ItemCount = entity.Items.Count };
    }

    /// <summary>
    /// Maps a collection of <see cref="ContentOrderEntity" /> to a list of <see cref="ContentOrderSummaryDto" />.
    /// </summary>
    public static IReadOnlyList<ContentOrderSummaryDto> ToContentOrderSummaryDtos(
        this IReadOnlyList<ContentOrderEntity> entities,
        IMapper mapper
    )
    {
        return entities.Select(e => e.ToContentOrderSummaryDto(mapper)).ToList();
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderEntity" /> to a <see cref="ContentOrderDetailDto" />.
    /// </summary>
    public static ContentOrderDetailDto ToContentOrderDetailDto(this ContentOrderEntity entity, IMapper mapper)
    {
        var dto = mapper.Map<ContentOrderDetailDto>(entity);
        return dto with
        {
            Items = entity.Items.Select(i => i.ToOrderItemDto(mapper)).ToList(),
            Payment = entity.Payment != null ? mapper.Map<PaymentDto>(entity.Payment) : null,
        };
    }

    /// <summary>
    /// Maps a <see cref="ContentPaymentEntity" /> to a <see cref="PaymentSummaryDto" />
    /// including customer name and order status from the linked order.
    /// </summary>
    public static async Task<PaymentSummaryDto> ToPaymentSummaryDtoAsync(
        this ContentPaymentEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<PaymentSummaryDto>(entity);
        string? verifiedByUserName = entity.VerifiedById.HasValue
            ? await userLookup.GetUserNameByIdAsync(entity.VerifiedById.Value, ct)
            : null;

        return dto with
        {
            OrderId = entity.OrderId,
            CustomerName = entity.Order.Customer.FullName,
            OrderStatus = entity.Order.Status,
            VerifiedBy = entity.VerifiedById,
            VerifiedByUserName = verifiedByUserName,
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="ContentPaymentEntity" /> to a list of <see cref="PaymentSummaryDto" />.
    /// </summary>
    public static async Task<IReadOnlyList<PaymentSummaryDto>> ToPaymentSummaryDtosAsync(
        this IReadOnlyList<ContentPaymentEntity> entities,
        IMapper mapper,
        IUserLookupService userLookup,
        CancellationToken ct = default
    )
    {
        var tasks = entities.Select(e => e.ToPaymentSummaryDtoAsync(mapper, userLookup, ct));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    /// <summary>
    /// Maps a <see cref="ContentPaymentEntity" /> to a <see cref="PaymentDto" />,
    /// injecting the resolved proof file so the frontend can render image or PDF accordingly.
    /// </summary>
    public static async Task<PaymentDto> ToPaymentDtoAsync(
        this ContentPaymentEntity entity,
        IMapper mapper,
        IUserLookupService userLookup,
        FileDto? proofFile = null,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<PaymentDto>(entity);
        string? verifiedByUserName = entity.VerifiedById.HasValue
            ? await userLookup.GetUserNameByIdAsync(entity.VerifiedById.Value, ct)
            : null;

        return dto with
        {
            PaymentProof = proofFile,
            VerifiedByUserName = verifiedByUserName,
        };
    }

    /// <summary>
    /// Maps a <see cref="FileEntity" /> to a <see cref="FileDto" />, or returns null if the entity is null.
    /// </summary>
    public static FileDto? ToFileDto(this FileEntity? fileEntity, IMapper mapper)
    {
        return fileEntity == null ? null : mapper.Map<FileDto>(fileEntity);
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderItemEntity" /> to an <see cref="OrderItemDto" />.
    /// </summary>
    public static OrderItemDto ToOrderItemDto(this ContentOrderItemEntity entity, IMapper mapper)
    {
        var dto = mapper.Map<OrderItemDto>(entity);
        return dto with
        {
            Tiers = mapper.Map<IReadOnlyList<ItemTierDto>>(entity.Tiers),
            PromoPriceUsd = entity.PromoPriceSnapshotUsd,
        };
    }
}
