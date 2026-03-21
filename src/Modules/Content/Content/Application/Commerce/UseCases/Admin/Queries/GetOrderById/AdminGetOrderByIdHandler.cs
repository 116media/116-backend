using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Handles the <see cref="AdminGetOrderByIdQuery" /> to retrieve a full order detail by identifier.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="fileRepository">Repository for resolving payment proof file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetOrderByIdHandler(
    IContentOrderRepository contentOrderRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<AdminGetOrderByIdQuery, AdminGetOrderByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOrderByIdResult> Handle(AdminGetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: query.Id,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw ContentOrderErrors.NotFound(id: query.Id);
        }

        var dto = order.ToContentOrderDetailDto(mapper);

        if (order.Payment?.PaymentProofFileId is not { } proofFileId)
        {
            return new AdminGetOrderByIdResult(Order: dto);
        }

        FileEntity? proofFile = await fileRepository.GetByIdAsync(proofFileId, cancellationToken);
        var proofDto = proofFile.ToFileDto(mapper);
        dto = dto with { Payment = order.Payment.ToPaymentDto(mapper, proofDto) };

        return new AdminGetOrderByIdResult(Order: dto);
    }
}
