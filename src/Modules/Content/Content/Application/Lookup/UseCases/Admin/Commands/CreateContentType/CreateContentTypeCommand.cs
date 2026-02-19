using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Command for creating a new content type.
/// </summary>
/// <param name="Name">The display name of the content type (e.g., "Article", "Video").</param>
public record CreateContentTypeCommand(string Name) : ICommand<CreateContentTypeResult>;

/// <summary>
/// Result of the <see cref="CreateContentTypeCommand" /> containing the created content type details.
/// </summary>
/// <param name="ContentType">The created content type information.</param>
public record CreateContentTypeResult(ContentTypeDto ContentType);
