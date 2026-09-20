using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Handles the <see cref="AdminGetArticleByIdQuery" /> to retrieve a single article by its identifier.
/// Enriches the response with the author's profile (user name, email, avatar URL, role) by:
/// 1. Resolving the author's identity info via <see cref="IUserLookupService" />
/// 2. Resolving the avatar file URL via <see cref="IFileRepository" /> if the author has an avatar
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving author profiles.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetArticleByIdHandler(
    IArticleRepository articleRepository,
    IUserLookupService userLookup,
    IFileStorageService fileStorage,
    IMapper mapper,
    IContentLookupFactory contentLookupFactory
) : IQueryHandler<AdminGetArticleByIdQuery, AdminGetArticleByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetArticleByIdResult> Handle(
        AdminGetArticleByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = await article.ToArticleDetailDtoAsync(
            mapper,
            await contentLookupFactory.ResolveForArticlesAsync([article], cancellationToken),
            fileStorage,
            cancellationToken
        );

        AuthorDto? authorInfo = await userLookup.GetAuthorInfoByIdAsync(
            userId: article.AuthorId,
            ct: cancellationToken
        );

        AdminAuthorDto? author = null;
        if (authorInfo is not null)
        {
            string? avatarUrl = null;
            if (authorInfo.AvatarFileId.HasValue)
            {
                FileReferenceDto? avatarFile = await fileStorage.ResolveAsync(
                    authorInfo.AvatarFileId.Value,
                    cancellationToken
                );
                avatarUrl = avatarFile?.StorageUrl;
            }

            author = new AdminAuthorDto(
                UserName: authorInfo.UserName,
                Email: authorInfo.Email,
                AvatarUrl: avatarUrl,
                Role: authorInfo.Role
            );
        }

        return new AdminGetArticleByIdResult(Article: dto with { Author = author });
    }
}
