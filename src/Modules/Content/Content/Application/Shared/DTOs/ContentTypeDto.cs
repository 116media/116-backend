namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a content type.
/// </summary>
/// <param name="Id">The unique identifier of the content type.</param>
/// <param name="Name">The display name of the content type.</param>
/// <param name="IsActive">Whether the content type is currently active.</param>
public record ContentTypeDto(Guid Id, string Name, bool IsActive);
