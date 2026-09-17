using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Handles the <see cref="AdminGetOrderPaymentQuery" /> to retrieve the payment record for an order.
/// </summary>
/// <param name="orderPaymentFactory">Shared factory for fetching and validating payment records.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="userLookup">Cross-module service for resolving admin user names.</param>
public class AdminGetOrderPaymentHandler(
    IOrderPaymentFactory orderPaymentFactory,
    IContentOrderRepository contentOrderRepository,
    IFileStorageService fileStorage,
    IMapper mapper,
    IUserLookupService userLookup
) : IQueryHandler<AdminGetOrderPaymentQuery, AdminGetOrderPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOrderPaymentResult> Handle(
        AdminGetOrderPaymentQuery query,
        CancellationToken cancellationToken
    )
    {
        await contentOrderRepository.GetByIdOrThrowAsync(id: query.OrderId, ct: cancellationToken);

        ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
            orderId: query.OrderId,
            ct: cancellationToken
        );

        FileReferenceDto? proofFile = payment.PaymentProofFileId.HasValue
            ? await fileStorage.ResolveAsync(payment.PaymentProofFileId.Value, cancellationToken)
            : null;

        var proofDto = proofFile.ToFileDto(mapper);
        var dto = await payment.ToPaymentDtoAsync(mapper, userLookup, proofDto, cancellationToken);

        return new AdminGetOrderPaymentResult(Payment: dto);
    }
}
