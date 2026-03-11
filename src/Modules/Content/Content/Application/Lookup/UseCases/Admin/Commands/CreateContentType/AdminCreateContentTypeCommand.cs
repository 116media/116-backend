using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Command for creating a new content type.
/// </summary>
/// <param name="Name">The display name of the content type (e.g., "Article", "Video").</param>
public record AdminCreateContentTypeCommand(string Name) : ICommand<AdminCreateContentTypeResult>;

/// <summary>
/// Result of the <see cref="AdminCreateContentTypeCommand" /> containing the created content type details.
/// </summary>
/// <param name="ContentType">The created content type information.</param>
public record AdminCreateContentTypeResult(ContentTypeDto ContentType);
