using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Handles the <see cref="CreateTagCommand" /> to create a new content tag.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class CreateTagHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<CreateTagCommand, CreateTagResult>
{
    /// <inheritdoc />
    public async Task<CreateTagResult> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        TagEntity? existing = await lookupRepository.GetTagBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw TagErrors.SlugAlreadyExists(slug: command.Slug);
        }

        var tag = TagEntity.Create(id: Guid.NewGuid(), name: command.Name, slug: command.Slug);

        await lookupRepository.AddTagAsync(tag: tag, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = tag.ToTagDto(mapper);
        return new CreateTagResult(Tag: dto);
    }
}
