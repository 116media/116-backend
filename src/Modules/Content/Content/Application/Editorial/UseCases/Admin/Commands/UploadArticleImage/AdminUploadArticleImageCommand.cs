using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Command for uploading an image associated with an article (cover or body).
/// </summary>
/// <param name="ArticleId">The unique identifier of the article to attach the image to.</param>
/// <param name="File">The image file to upload.</param>
/// <param name="ImageType">Whether this is a cover image or a body image.</param>
public record AdminUploadArticleImageCommand(string ArticleId, IFormFile File, EnumArticleImageType ImageType)
    : ICommand<AdminUploadArticleImageResult>;

/// <summary>
/// Result of the <see cref="AdminUploadArticleImageCommand" /> containing the uploaded image details.
/// </summary>
/// <param name="Image">The uploaded article image information.</param>
public record AdminUploadArticleImageResult(ArticleImageDto Image);
