using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Playlist validation error messages.</param>
    public PublicRenamePlaylistValidator(PlaylistErrorMessage msg)
    {
        RuleFor(x => x.Name)
            .ValidPlaylistName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxPlaylistNameLength)
            );
    }
}
