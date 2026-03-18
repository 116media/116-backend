using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.DTOs;
using _116.Core.Domain.Entities;
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
            .Map(dest => dest.ContentKind, src => src.ContentKind.ToString())
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.PromotionLevelName, src => src.PromotionLevel != null ? src.PromotionLevel.Name : null)
            .Map(dest => dest.PromoPriceUsd, src => src.PromoPriceSnapshotUsd)
            .Map(dest => dest.Tiers, src => src.Tiers);

        config
            .NewConfig<ContentPaymentEntity, PaymentDto>()
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod != null ? src.PaymentMethod.ToString() : null)
            .Map(dest => dest.PaymentProof, _ => (FileDto?)null)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.VerifiedBy, src => src.VerifiedById);

        config
            .NewConfig<ContentOrderEntity, ContentOrderSummaryDto>()
            .Map(dest => dest.CustomerName, src => src.Customer.FullName)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config
            .NewConfig<ContentOrderEntity, ContentOrderDetailDto>()
            .Map(dest => dest.CustomerName, src => src.Customer.FullName)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.Payment, src => src.Payment);
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderEntity" /> to a <see cref="ContentOrderSummaryDto" />.
    /// </summary>
    public static ContentOrderSummaryDto ToContentOrderSummaryDto(this ContentOrderEntity entity, IMapper mapper)
    {
        return mapper.Map<ContentOrderSummaryDto>(entity);
    }

    /// <summary>
    /// Maps a collection of <see cref="ContentOrderEntity" /> to a list of <see cref="ContentOrderSummaryDto" />.
    /// </summary>
    public static IReadOnlyList<ContentOrderSummaryDto> ToContentOrderSummaryDtos(
        this IReadOnlyList<ContentOrderEntity> entities,
        IMapper mapper
    )
    {
        return mapper.Map<IReadOnlyList<ContentOrderSummaryDto>>(entities);
    }

    /// <summary>
    /// Maps a <see cref="ContentOrderEntity" /> to a <see cref="ContentOrderDetailDto" />.
    /// </summary>
    public static ContentOrderDetailDto ToContentOrderDetailDto(this ContentOrderEntity entity, IMapper mapper)
    {
        return mapper.Map<ContentOrderDetailDto>(entity);
    }

    /// <summary>
    /// Maps a <see cref="ContentPaymentEntity" /> to a <see cref="PaymentDto" />,
    /// injecting the resolved proof file so the frontend can render image or PDF accordingly.
    /// </summary>
    public static PaymentDto ToPaymentDto(this ContentPaymentEntity entity, IMapper mapper, FileDto? proofFile = null)
    {
        var dto = mapper.Map<PaymentDto>(entity);
        return dto with { PaymentProof = proofFile };
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
        return mapper.Map<OrderItemDto>(entity);
    }
}
