using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Command to activate a content type, making it available for use.
/// </summary>
/// <param name="Id">The unique identifier of the content type to activate.</param>
public record AdminActivateContentTypeCommand(string Id) : ICommand<AdminActivateContentTypeResult>;

/// <summary>
/// Result returned after successfully activating a content type.
/// </summary>
/// <param name="ContentType">The updated content type information.</param>
public record AdminActivateContentTypeResult(ContentTypeDto ContentType);
