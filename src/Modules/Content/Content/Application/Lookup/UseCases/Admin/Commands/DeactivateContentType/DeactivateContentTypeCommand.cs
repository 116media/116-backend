using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Command to deactivate a content type, preventing it from being used.
/// </summary>
/// <param name="Id">The unique identifier of the content type to deactivate.</param>
public record DeactivateContentTypeCommand(Guid Id) : ICommand<DeactivateContentTypeResult>;

/// <summary>
/// Result returned after successfully deactivating a content type.
/// </summary>
/// <param name="ContentType">The updated content type information.</param>
public record DeactivateContentTypeResult(ContentTypeDto ContentType);
