using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist;

/// <summary>
/// Command to link or unlink a video's real, addressable artist profile. Linking happens
/// when <see cref="ArtistId" /> is present; passing null unlinks the current artist profile.
/// </summary>
/// <param name="VideoId">The video to link or unlink.</param>
/// <param name="ArtistId">The artist profile to link, or null to unlink.</param>
public record AdminLinkVideoArtistCommand(Guid VideoId, Guid? ArtistId) : ICommand<AdminLinkVideoArtistResult>;

/// <summary>
/// Result of the <see cref="AdminLinkVideoArtistCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminLinkVideoArtistResult(bool IsSuccess);
