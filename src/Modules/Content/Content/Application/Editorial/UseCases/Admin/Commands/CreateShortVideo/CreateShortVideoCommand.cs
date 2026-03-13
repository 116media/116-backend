using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Command for uploading and creating a new short video clip.
/// The video file is uploaded directly to Cloudinary (not YouTube).
/// Optionally links the short video as a teaser for an existing full video.
/// </summary>
/// <param name="Title">The display title of the short video.</param>
/// <param name="VideoFile">The video file to upload to cloud storage.</param>
/// <param name="VideoId">Optional parent full video identifier. When provided, creates a teaser.</param>
public record CreateShortVideoCommand(string Title, IFormFile VideoFile, Guid? VideoId)
    : ICommand<CreateShortVideoResult>;

/// <summary>
/// Result of the <see cref="CreateShortVideoCommand" /> containing the newly created short video details.
/// </summary>
/// <param name="ShortVideo">The created short video information.</param>
public record CreateShortVideoResult(ShortVideoDto ShortVideo);
