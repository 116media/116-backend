using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Handles the <see cref="AdminUpdateTagCommand" /> to update an existing content tag.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateTagHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateTagCommand, AdminUpdateTagResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateTagResult> Handle(AdminUpdateTagCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        TagEntity tag = await lookupRepository.GetTagByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        TagEntity? existing = await lookupRepository.GetTagBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null && !string.Equals(tag.Slug, command.Slug, StringComparison.OrdinalIgnoreCase))
        {
            throw i18n.Tag.SlugAlreadyExists(slug: command.Slug);
        }

        tag.Update(name: command.Name, slug: command.Slug, errors: i18n.Tag);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = tag.ToTagDto(mapper);
        return new AdminUpdateTagResult(Tag: dto);
    }
}
