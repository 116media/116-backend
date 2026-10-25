using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Handles the <see cref="AdminCreateTagCommand" /> to create a new content tag.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreateTagHandler(ITagRepository tagRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<AdminCreateTagCommand, AdminCreateTagResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateTagResult> Handle(AdminCreateTagCommand command, CancellationToken cancellationToken)
    {
        string slug = await SlugHelper.ToAvailableSlugAsync(
            name: command.Name,
            isTakenAsync: async (candidate, ct) =>
                await tagRepository.GetBySlugAsync(slug: candidate, cancellationToken: ct) is not null,
            unique: true,
            cancellationToken: cancellationToken
        );

        var tag = TagEntity.Create(id: Guid.NewGuid(), name: command.Name, slug: slug);

        await tagRepository.AddAsync(tag: tag, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = tag.ToTagDto(mapper);
        return new AdminCreateTagResult(Tag: dto);
    }
}
