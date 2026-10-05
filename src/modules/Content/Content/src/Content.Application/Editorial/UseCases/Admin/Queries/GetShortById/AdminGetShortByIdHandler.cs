using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Handles the <see cref="AdminGetShortByIdQuery" /> to serve one short with its author.
/// </summary>
/// <param name="shortVideoRepository">Repository loading the short.</param>
/// <param name="shortVideoDtoService">Service assembling the short DTO with its author.</param>
public class AdminGetShortByIdHandler(
    IShortVideoRepository shortVideoRepository,
    IShortVideoDtoService shortVideoDtoService
) : IQueryHandler<AdminGetShortByIdQuery, AdminGetShortByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetShortByIdResult> Handle(AdminGetShortByIdQuery query, CancellationToken cancellationToken)
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        ShortVideoDto dto = await shortVideoDtoService.CreateWithAuthorAsync(shortVideo, cancellationToken);
        return new AdminGetShortByIdResult(ShortVideo: dto);
    }
}
