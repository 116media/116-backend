using System.Runtime.CompilerServices;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ITagRepository"/>.
/// </summary>
public static class MockTagRepository
{
    /// <summary>
    /// Tags registered through <see cref="SetupGetTagByName" />, per mock instance, keyed by
    /// lower-cased name so the batch lookup serves the same arrangement as the single one.
    /// </summary>
    private static readonly ConditionalWeakTable<Mock<ITagRepository>, Dictionary<string, TagEntity>> KnownTags = new();

    /// <summary>
    /// Creates a new mock instance of ITagRepository with default setups. Identity lookups are
    /// left unconfigured so that a miss has to be arranged by the test, naming the identifier it
    /// is a miss for, rather than being asserted for every identifier before the test says
    /// anything.
    /// </summary>
    public static Mock<ITagRepository> Create()
    {
        Mock<ITagRepository> mock = new();
        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, TagEntity>());
        Dictionary<string, TagEntity> known = KnownTags.GetOrCreateValue(mock);

        mock.Setup(x => x.GetByNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<string> names, CancellationToken _) =>
                    (IReadOnlyDictionary<string, TagEntity>)
                        names
                            .Select(name => name.ToLower())
                            .Distinct()
                            .Where(known.ContainsKey)
                            .ToDictionary(name => name, name => known[name])
            );
        mock.Setup(x => x.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new List<TagEntity>());
        return mock;
    }

    /// <summary>
    /// Arranges the batch lookup to resolve exactly the supplied rows, keyed by id.
    /// </summary>
    public static Mock<ITagRepository> SetupGetByIds(this Mock<ITagRepository> mock, params TagEntity[] entities)
    {
        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities.ToDictionary(entity => entity.Id));
        return mock;
    }

    public static Mock<ITagRepository> SetupGetTagByIdOrThrow(this Mock<ITagRepository> mock, TagEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ITagRepository> SetupGetTagByIdOrThrowNotFound(this Mock<ITagRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Tag with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ITagRepository> SetupGetTagBySlug(this Mock<ITagRepository> mock, string slug, TagEntity? tag)
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        return mock;
    }

    public static Mock<ITagRepository> SetupGetTagByName(this Mock<ITagRepository> mock, string name, TagEntity? tag)
    {
        mock.Setup(x => x.GetByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        Dictionary<string, TagEntity> known = KnownTags.GetOrCreateValue(mock);
        if (tag is not null)
        {
            known[name.ToLower()] = tag;
        }
        else
        {
            known.Remove(name.ToLower());
        }

        return mock;
    }

    public static Mock<ITagRepository> SetupGetAllTags(this Mock<ITagRepository> mock, IReadOnlyList<TagEntity> list)
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<string?>(),
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(list);
        return mock;
    }

    public static Mock<ITagRepository> SetupGetPopularTags(
        this Mock<ITagRepository> mock,
        IReadOnlyList<TagEntity> list
    )
    {
        mock.Setup(x =>
                x.GetPopularAsync(It.IsAny<int?>(), It.IsAny<EnumCoreContentType?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddTagCalled(this Mock<ITagRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddTagNotCalled(this Mock<ITagRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static void VerifyRemoveTagCalled(this Mock<ITagRepository> mock, TagEntity tag)
    {
        mock.Verify(x => x.Remove(tag), Times.Once);
    }

    public static void VerifyRemoveTagNotCalled(this Mock<ITagRepository> mock)
    {
        mock.Verify(x => x.Remove(It.IsAny<TagEntity>()), Times.Never);
    }
}
