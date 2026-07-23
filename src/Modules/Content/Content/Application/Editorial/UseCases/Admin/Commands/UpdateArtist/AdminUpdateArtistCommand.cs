using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;

/// <summary>
/// Command for updating an artist profile's editable fields. Slug is immutable after
/// creation and cannot be changed via this command.
/// </summary>
/// <param name="Id">The unique identifier of the artist profile to update.</param>
/// <param name="Name">The artist's display name.</param>
/// <param name="Bio">Optional free-text biography, or null to clear it.</param>
public record AdminUpdateArtistCommand(Guid Id, string Name, string? Bio) : ICommand<AdminUpdateArtistResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateArtistCommand" /> containing the updated artist profile.
/// </summary>
/// <param name="Artist">The updated artist profile information.</param>
public record AdminUpdateArtistResult(ArtistDto Artist);
