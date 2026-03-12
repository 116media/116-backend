using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Command for updating an existing content type.
/// </summary>
/// <param name="Id">The unique identifier of the content type to update.</param>
/// <param name="Name">The new name for the content type.</param>
public record UpdateContentTypeCommand(string Id, string Name) : ICommand<UpdateContentTypeResult>;

/// <summary>
/// Result of the <see cref="UpdateContentTypeCommand" /> containing the updated content type details.
/// </summary>
/// <param name="ContentType">The updated content type information.</param>
public record UpdateContentTypeResult(ContentTypeDto ContentType);
