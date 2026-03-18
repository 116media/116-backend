using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Handles the <see cref="PublicRateVideoCommand" /> to create or update a user's video rating.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicRateVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicRateVideoCommand, PublicRateVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicRateVideoResult> Handle(PublicRateVideoCommand command, CancellationToken cancellationToken)
    {
        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: command.VideoId,
            cancellationToken: cancellationToken
        );

        VideoRatingEntity? existingRating = await videoRepository.GetRatingAsync(
            userId: command.UserId,
            videoId: command.VideoId,
            cancellationToken: cancellationToken
        );

        if (existingRating is not null)
        {
            existingRating.UpdateStars(stars: command.Stars);
            videoRepository.UpdateRating(rating: existingRating);
        }
        else
        {
            VideoRatingEntity newRating = VideoRatingEntity.Create(
                id: Guid.NewGuid(),
                userId: command.UserId,
                videoId: command.VideoId,
                stars: command.Stars
            );
            await videoRepository.AddRatingAsync(rating: newRating, cancellationToken: cancellationToken);
        }

        List<VideoRatingEntity> allRatings = await videoRepository.GetAllRatingsForVideoAsync(
            videoId: command.VideoId,
            cancellationToken: cancellationToken
        );

        int count = allRatings.Count;
        decimal average = count > 0 ? (decimal)allRatings.Sum(r => r.Stars) / count : 0m;

        video.UpdateRating(average: Math.Round(average, 2), count: count);
        videoRepository.Update(video: video);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRateVideoResult(IsSuccess: true);
    }
}
