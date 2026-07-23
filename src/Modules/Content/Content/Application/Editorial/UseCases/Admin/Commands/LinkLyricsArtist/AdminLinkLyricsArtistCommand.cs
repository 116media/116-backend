using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist;

/// <summary>
/// Command to link or unlink a lyrics page's real, addressable artist profile. Linking
/// happens when <see cref="ArtistId" /> is present; passing null unlinks the current
/// artist profile, reverting the lyrics page to its plain-text <c>ArtistName</c> only.
/// </summary>
/// <param name="LyricsId">The lyrics page to link or unlink.</param>
/// <param name="ArtistId">The artist profile to link, or null to unlink.</param>
public record AdminLinkLyricsArtistCommand(Guid LyricsId, Guid? ArtistId) : ICommand<AdminLinkLyricsArtistResult>;

/// <summary>
/// Result of the <see cref="AdminLinkLyricsArtistCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminLinkLyricsArtistResult(bool IsSuccess);
