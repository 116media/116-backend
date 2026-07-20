using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Command for creating a new, unclaimed artist profile — typically staff-curated from an
/// existing lyrics or video record's <c>ArtistName</c>.
/// </summary>
/// <param name="Name">The artist's display name.</param>
/// <param name="Slug">The URL-safe slug for the artist's public page.</param>
/// <param name="Bio">Optional free-text biography.</param>
public record AdminCreateArtistCommand(string Name, string Slug, string? Bio) : ICommand<AdminCreateArtistResult>;

/// <summary>
/// Result of the <see cref="AdminCreateArtistCommand" /> containing the newly created artist profile.
/// </summary>
/// <param name="Artist">The created artist profile information.</param>
public record AdminCreateArtistResult(ArtistDto Artist);
