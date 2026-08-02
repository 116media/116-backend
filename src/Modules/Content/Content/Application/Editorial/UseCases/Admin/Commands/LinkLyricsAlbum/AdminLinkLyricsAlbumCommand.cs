using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum;

/// <summary>
/// Command to link or unlink a lyrics page's real, addressable album. Linking happens when
/// <see cref="AlbumId" /> is present; passing null unlinks the current album, reverting the
/// lyrics page to its plain-text <c>Album</c> field only.
/// </summary>
/// <param name="LyricsId">The lyrics page to link or unlink.</param>
/// <param name="AlbumId">The album to link, or null to unlink.</param>
public record AdminLinkLyricsAlbumCommand(Guid LyricsId, Guid? AlbumId) : ICommand<AdminLinkLyricsAlbumResult>;

/// <summary>
/// Result of the <see cref="AdminLinkLyricsAlbumCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminLinkLyricsAlbumResult(bool IsSuccess);
