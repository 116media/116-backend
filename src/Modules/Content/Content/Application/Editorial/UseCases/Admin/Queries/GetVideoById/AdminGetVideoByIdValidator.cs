using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Validator for the <see cref="AdminGetVideoByIdQuery" /> ensuring a valid video ID is provided.
/// </summary>
public class AdminGetVideoByIdValidator : AbstractValidator<AdminGetVideoByIdQuery>
{
    /// <summary>
    /// Configures validation rules for retrieving a video by ID.
    /// </summary>
    public AdminGetVideoByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
