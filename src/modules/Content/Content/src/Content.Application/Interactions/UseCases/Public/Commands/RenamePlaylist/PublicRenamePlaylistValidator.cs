using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Validator for the <see cref="PublicRenamePlaylistCommand" />.
/// </summary>
public class PublicRenamePlaylistValidator : AbstractValidator<PublicRenamePlaylistCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicRenamePlaylistValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicRenamePlaylistValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Name).ValidPlaylistName(i18n.Playlist.Msg);
    }
}
