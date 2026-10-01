using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.Services;

/// <summary>
/// Assembles article comment response DTOs with the author profile resolved.
/// </summary>
public interface IArticleCommentDtoService
{
    /// <summary>
    /// Builds the public DTO of one comment, resolving its author's name and avatar.
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<PublicArticleCommentDto> CreatePublicAsync(ArticleCommentEntity comment, CancellationToken ct = default);
}
