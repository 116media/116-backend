using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Validator for the <see cref="PublicRenamePlaylistCommand" />.
/// </summary>
public class PublicRenamePlaylistValidator : AbstractValidator<PublicRenamePlaylistCommand>
{
    /// <summary>
    /// Configures validation rules for renaming a playlist.
    /// </summary>
    public PublicRenamePlaylistValidator()
    {
        RuleFor(x => x.Name).ValidPlaylistName();
    }
}
