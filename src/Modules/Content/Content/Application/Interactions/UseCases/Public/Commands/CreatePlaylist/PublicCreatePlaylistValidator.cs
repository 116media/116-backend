using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;

/// <summary>
/// Validator for the <see cref="PublicCreatePlaylistCommand" />.
/// </summary>
public class PublicCreatePlaylistValidator : AbstractValidator<PublicCreatePlaylistCommand>
{
    /// <summary>
    /// Configures validation rules for creating a playlist.
    /// </summary>
    public PublicCreatePlaylistValidator()
    {
        RuleFor(x => x.Name).ValidPlaylistName();
    }
}
