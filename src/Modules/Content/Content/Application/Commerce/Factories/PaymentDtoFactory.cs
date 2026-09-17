using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Commerce.Factories;

/// <summary>
/// Factory implementation building payment projections from a pre-resolved verifier map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="userLookup">Identity's lookup contract, resolving verifier names.</param>
public class PaymentDtoFactory(IMapper mapper, IUserLookupService userLookup) : IPaymentDtoFactory
{
    /// <inheritdoc />
    public async Task<PaymentDto> CreateAsync(
        ContentPaymentEntity payment,
        FileDto? proofFile = null,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, AuthorDto> verifiers = await ResolveVerifiersAsync([payment], ct);

        return payment.ToPaymentDto(mapper, verifiers, proofFile);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentSummaryDto>> CreateManyAsync(
        IReadOnlyList<ContentPaymentEntity> payments,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, AuthorDto> verifiers = await ResolveVerifiersAsync(payments, ct);

        return payments.Select(payment => payment.ToPaymentSummaryDto(mapper, verifiers)).ToList();
    }

    /// <summary>
    /// Resolves every distinct verifier the supplied payments reference, in one query.
    /// </summary>
    /// <param name="payments">The payments whose verifiers to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The verifiers, keyed by user id.</returns>
    private Task<IReadOnlyDictionary<Guid, AuthorDto>> ResolveVerifiersAsync(
        IReadOnlyList<ContentPaymentEntity> payments,
        CancellationToken ct
    )
    {
        return userLookup.GetAuthorInfosByIdsAsync(
            payments.Where(p => p.VerifiedById.HasValue).Select(p => p.VerifiedById!.Value).Distinct().ToList(),
            ct
        );
    }
}
