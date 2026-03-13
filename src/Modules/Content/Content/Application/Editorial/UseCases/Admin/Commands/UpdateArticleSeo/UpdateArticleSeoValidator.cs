using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Validator for the <see cref="UpdateArticleSeoCommand" /> ensuring a valid article ID is provided.
/// </summary>
public class UpdateArticleSeoValidator : AbstractValidator<UpdateArticleSeoCommand>
{
    /// <summary>
    /// Configures validation rules for article SEO update.
    /// </summary>
    public UpdateArticleSeoValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Article ID");
    }
}
