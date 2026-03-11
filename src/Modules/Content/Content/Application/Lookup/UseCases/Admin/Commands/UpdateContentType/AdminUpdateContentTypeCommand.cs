using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Command for updating an existing content type.
/// </summary>
/// <param name="Id">The unique identifier of the content type to update.</param>
/// <param name="Name">The new name for the content type.</param>
public record AdminUpdateContentTypeCommand(string Id, string Name) : ICommand<AdminUpdateContentTypeResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateContentTypeCommand" /> containing the updated content type details.
/// </summary>
/// <param name="ContentType">The updated content type information.</param>
public record AdminUpdateContentTypeResult(ContentTypeDto ContentType);
