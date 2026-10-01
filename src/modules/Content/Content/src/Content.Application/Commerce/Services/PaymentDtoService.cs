using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Commerce.Services;

/// <summary>
/// Service implementation building payment projections from a pre-resolved verifier map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="userLookup">Identity's lookup contract, resolving verifier names.</param>
/// <param name="fileStorage">Storage contract resolving the proof file.</param>
/// <param name="orderDtoService">Service resolving the ordering customers.</param>
public class PaymentDtoService(
    IMapper mapper,
    IUserLookupService userLookup,
    IContentOrderDtoService orderDtoService,
    IFileStorageService fileStorage
) : IPaymentDtoService
{
    /// <inheritdoc />
    public async Task<PaymentDto> CreateWithProofAsync(ContentPaymentEntity payment, CancellationToken ct = default)
    {
        FileReferenceDto? proofFile = payment.PaymentProofFileId is { } proofFileId
            ? await fileStorage.ResolveAsync(proofFileId, ct)
            : null;

        return await CreateAsync(payment, proofFile.ToFileDto(mapper), ct);
    }

    /// <inheritdoc />
    public async Task<PaymentDto> CreateAsync(
        ContentPaymentEntity payment,
        FileDto? proofFile = null,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, UserProfileDto> verifiers = await ResolveVerifiersAsync([payment], ct);

        return payment.ToPaymentDto(mapper, verifiers, proofFile);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentSummaryDto>> CreateManyAsync(
        IReadOnlyList<ContentOrderEntity> orders,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, UserProfileDto> verifiers = await ResolveVerifiersAsync(
            [.. orders.Select(order => order.Payment!)],
            ct
        );
        IReadOnlyDictionary<Guid, CustomerEntity> customers = await orderDtoService.ResolveCustomersAsync(orders, ct);

        return orders.Select(order => order.ToPaymentSummaryDto(mapper, verifiers, customers)).ToList();
    }

    /// <summary>
    /// Resolves every distinct verifier the supplied payments reference, in one query.
    /// </summary>
    /// <param name="payments">The payments whose verifiers to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The verifiers, keyed by user id.</returns>
    private Task<IReadOnlyDictionary<Guid, UserProfileDto>> ResolveVerifiersAsync(
        IReadOnlyList<ContentPaymentEntity> payments,
        CancellationToken ct
    )
    {
        return userLookup.GetUserProfilesByIdsAsync(
            payments.Where(p => p.VerifiedById.HasValue).Select(p => p.VerifiedById!.Value).Distinct().ToList(),
            ct
        );
    }
}
