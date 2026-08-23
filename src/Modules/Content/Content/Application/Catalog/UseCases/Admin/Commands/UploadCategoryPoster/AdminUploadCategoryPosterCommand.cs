using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Command for uploading or replacing a category's poster image.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
/// <param name="File">The poster image file to upload. Null when the file part is missing.</param>
public record AdminUploadCategoryPosterCommand(string Id, IFormFile? File) : ICommand<AdminUploadCategoryPosterResult>;

/// <summary>
/// Result of the <see cref="AdminUploadCategoryPosterCommand" /> containing the updated category.
/// </summary>
/// <param name="Category">The updated category information with resolved poster URL.</param>
public record AdminUploadCategoryPosterResult(CategoryDto Category);
