using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoByIdAdmin;

/// <summary>
/// Validator for the <see cref="GetVideoByIdAdminQuery" /> ensuring a valid video ID is provided.
/// </summary>
public class GetVideoByIdAdminValidator : AbstractValidator<GetVideoByIdAdminQuery>
{
    /// <summary>
    /// Configures validation rules for retrieving a video by ID.
    /// </summary>
    public GetVideoByIdAdminValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Video ID");
    }
}
