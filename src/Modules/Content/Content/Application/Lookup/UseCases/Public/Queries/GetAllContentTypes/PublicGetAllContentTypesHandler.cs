using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;

/// <summary>
/// Handles the <see cref="PublicGetAllContentTypesQuery" /> to retrieve all content types.
/// </summary>
/// <param name="contentTypeRepository">Repository for content type data access operations.</param>
public class PublicGetAllContentTypesHandler(IContentTypeRepository contentTypeRepository)
    : IQueryHandler<PublicGetAllContentTypesQuery, PublicGetAllContentTypesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetAllContentTypesResult> Handle(
        PublicGetAllContentTypesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ContentTypeEntity> contentTypes = await contentTypeRepository.GetActiveAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicContentTypeDto> dtoList = contentTypes.ToPublicContentTypeDtos();
        return new PublicGetAllContentTypesResult(ContentTypes: dtoList);
    }
}
