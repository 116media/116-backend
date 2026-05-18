using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Handles the <see cref="AdminGetOrderByIdQuery" /> to retrieve a full order detail by identifier.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="fileRepository">Repository for resolving payment proof file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="userLookup">Cross-module service for resolving admin user names.</param>
/// <param name="contentOrderErrors">Content order domain error factory.</param>
public class AdminGetOrderByIdHandler(
    IContentOrderRepository contentOrderRepository,
    IFileRepository fileRepository,
    IMapper mapper,
    IUserLookupService userLookup,
    ContentOrderErrors contentOrderErrors
) : IQueryHandler<AdminGetOrderByIdQuery, AdminGetOrderByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetOrderByIdResult> Handle(AdminGetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: query.Id,
            ct: cancellationToken
        );

        if (order is not null)
        {
            var dto = order.ToContentOrderDetailDto(mapper);

            if (order.Payment?.PaymentProofFileId is not { } proofFileId)
            {
                return new AdminGetOrderByIdResult(Order: dto);
            }

            FileEntity? proofFile = await fileRepository.GetByIdAsync(proofFileId, cancellationToken);
            var proofDto = proofFile.ToFileDto(mapper);
            dto = dto with
            {
                Payment = await order.Payment.ToPaymentDtoAsync(mapper, userLookup, proofDto, cancellationToken),
            };

            return new AdminGetOrderByIdResult(Order: dto);
        }

        throw contentOrderErrors.NotFound(id: query.Id);
    }
}
