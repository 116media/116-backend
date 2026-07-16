# Two Patterns That Turn Painful Test Setup Into One Readable Line

## The Problem

Every test starts with a chore nobody actually asked for. Before you can check a single thing, you have to build the objects the test runs on. And on a domain with any real depth, that's where the time goes.

The backend I work on powers a media platform for music promotion: videos, articles, lyrics, short clips, customers, orders, all the usual moving parts. None of these entities are simple bags of setters. They protect themselves with private constructors, static creation methods, and lifecycles you have to walk through in order. Great for production. Rough on tests.

Here's what it takes to build one published video by hand, with data that looks real. It's the wrong way. Notice how much of it there is:

```csharp
/// <summary>
/// Stands up a single published video entirely by hand, with no test helpers.
/// </summary>
[Fact]
public async Task SomeTest()
{
    // Arrange
    var faker = new Faker();

    var services = new ServiceCollection();
    services.AddLogging();
    services.AddLocalization();
    services.AddScoped<VideoErrorMessage>();
    ServiceProvider provider = services.BuildServiceProvider();
    VideoErrors errors = new(provider.GetRequiredService<VideoErrorMessage>());

    VideoEntity video = VideoEntity.CreateFree(
        id: Guid.NewGuid(),
        categoryId: categoryId,
        title: faker.Lorem.Sentence(wordCount: 4),
        slug: faker.Lorem.Slug(),
        authorId: Guid.NewGuid(),
        description: faker.Lorem.Paragraph(),
        errors: errors
    );

    video.ScheduleShoot(faker.Date.FutureOffset());
    video.AttachYoutubeVideoUrl(faker.Internet.Url(), errors);
    video.SetThumbnailFileId(Guid.NewGuid());
    video.MarkPendingReview();
    video.Approve();
    video.Publish(errors);

    // ...and only now can the real test begin
}
```

Count that up. Over twenty lines, and not one of them is what the test is about. Just to get a single argument, you spin up a dependency injection container so it can hand you a localized errors object. Then, because the entity starts life as a draft, getting it to Published means stepping through the lifecycle one legal move at a time, and the YouTube URL has to be on it before you call publish or the whole thing throws. Skip a step, swap two, and you end up with a video in a state no real user could ever create. All of that, for an object you were planning to treat as a given.

Now picture it across thousands of tests, each one wanting a slightly different shape: a paid video, a rejected one, a draft with no URL, ten at once, a request body that actually passes its validator. Copy that setup into every test and three things go wrong:

- They get unreadable. The two lines that matter drown in setup noise.
- They get fragile. Add one required field and you're touching hundreds of files.
- They start lying. Somebody copies a slightly wrong setup and the test passes for the wrong reason.

Building test data is its own problem. It has nothing to do with what any given test is checking, and it deserves its own solution.

## The Idea

Two small patterns fix this, and they're better together than apart.

A builder gives you a fluent way to put one object together. It knows the annoying multi step dance (create the draft, attach the URL, push it through review and approval to published) and tucks it behind a few chained methods. You spell out only the bits your test cares about, and it fills in sane, valid defaults for everything else.

A factory gives you names for the setups you reach for over and over. Instead of chaining the same three builder calls in fifty places, you ask for the thing by name. `CreatePublished()`. `CreateRejected()`. `CreatePaid(customerId, orderItemId)`. The method name is the intent.

Think of the builder as the engine and the factory as the menu bolted on top. This started as one of each and grew into roughly a hundred builders and thirty factories across the content, core, and identity modules. They all share the same shape, so once you've read one you can pretty much read the rest.

## The Entity Builder

Here's the video builder, trimmed to what matters. Watch it eat the entire wall of code from the problem above:

```csharp
/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// For test code, prefer using VideoFactory instead of direct Builder usage.
/// </summary>
internal class VideoBuilder
{
    private readonly Faker _faker = new();

    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;
    private string _title;
    private string _slug;
    private Guid _authorId = Guid.NewGuid();
    private string _description;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private string? _youtubeVideoUrl;
    private string? _rejectionReason;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;

    /// <summary>
    /// Initializes the builder for the given category and seeds Faker generated defaults.
    /// </summary>
    /// <param name="categoryId">The category the video belongs to.</param>
    public VideoBuilder(Guid categoryId)
    {
        _categoryId = categoryId;

        // Bogus for readable content; a guid suffix on the unique columns to guarantee no collision
        _title = $"{_faker.Lorem.Sentence(wordCount: 4)} {Guid.NewGuid():N}";
        _slug = $"{_faker.Lorem.Slug()}-{Guid.NewGuid():N}";
        _description = _faker.Lorem.Paragraph();
    }

    /// <summary>
    /// Makes the video a paid video linked to a customer and an order item.
    /// </summary>
    /// <param name="customerId">The commissioning customer.</param>
    /// <param name="orderItemId">The order item the video fulfils.</param>
    /// <returns>The builder instance for chaining.</returns>
    public VideoBuilder WithCustomer(Guid customerId, Guid orderItemId)
    {
        _customerId = customerId;
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Transitions the built video to Approved status.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public VideoBuilder AsApproved()
    {
        _targetStatus = EnumContentStatus.Approved;
        return this;
    }

    /// <summary>
    /// Transitions the built video to Published status, attaching a valid YouTube URL when none is set.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public VideoBuilder AsPublished()
    {
        _targetStatus = EnumContentStatus.Published;
        _youtubeVideoUrl ??= TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;
        return this;
    }

    /// <summary>
    /// Transitions the built video to Rejected status with a reason.
    /// </summary>
    /// <param name="reason">The rejection reason, or null to use a valid default.</param>
    /// <returns>The builder instance for chaining.</returns>
    public VideoBuilder AsRejected(string? reason = null)
    {
        _targetStatus = EnumContentStatus.Rejected;
        _rejectionReason = reason ?? TestConstants.Content.Editorial.Video.ValidRejectionReason;
        return this;
    }

    /// <summary>
    /// Builds the configured <see cref="VideoEntity"/>, resolving the URL and lifecycle steps.
    /// </summary>
    /// <returns>The constructed video entity.</returns>
    public VideoEntity Build()
    {
        VideoErrors errors = TestErrorsFactory.CreateVideoErrors();

        VideoEntity entity = _customerId.HasValue
            ? VideoEntity.CreatePaid(
                id: _id,
                customerId: _customerId.Value,
                orderItemId: _orderItemId!.Value,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                description: _description,
                errors: errors
            )
            : VideoEntity.CreateFree(
                id: _id,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                description: _description,
                errors: errors
            );

        if (_youtubeVideoUrl is not null)
        {
            entity.AttachYoutubeVideoUrl(_youtubeVideoUrl, errors);
        }

        ApplyStatusTransition(entity, errors);
        return entity;
    }

    /// <summary>
    /// Walks the entity from Draft to the requested target status one legal step at a time.
    /// </summary>
    /// <param name="entity">The entity to transition.</param>
    /// <param name="errors">The domain errors facade.</param>
    private void ApplyStatusTransition(VideoEntity entity, VideoErrors errors)
    {
        switch (_targetStatus)
        {
            case EnumContentStatus.PendingReview:
                entity.MarkPendingReview();
                break;
            case EnumContentStatus.Approved:
                entity.MarkPendingReview();
                entity.Approve();
                break;
            case EnumContentStatus.Published:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish(errors);
                break;
            case EnumContentStatus.Rejected:
                entity.MarkPendingReview();
                entity.Reject(_rejectionReason!);
                break;
        }
    }
}
```

A few things worth pointing out.

Every field starts from a valid default: a fresh id, a Faker title and slug, a random author, a Faker paragraph for the description. Call `new VideoBuilder(categoryId).Build()` and you get a free video in Draft that clears every invariant. You override only what your test cares about, nothing more.

The title and slug are the interesting pair. Both columns are unique in the database, and Faker on its own won't save you there: its word list is finite, so two `Lorem.Slug()` calls can eventually land on the same value and the second insert blows up. So the builder uses both. Faker gives you something readable, and the guid on the end makes it genuinely unique. The description has no such constraint, so it stays a plain Faker paragraph.

`ApplyStatusTransition` is the part I like most. Remember typing `MarkPendingReview`, `Approve`, `Publish` by hand back in the problem? It lives here now, once, in the right order, behind a single `AsPublished()`. All the lifecycle knowledge that used to get copied into every test sits in one switch.

`AsPublished()` also quietly enforces a domain rule: you can't publish a video without a YouTube URL. So if the test didn't set one, the method drops a valid one in. It's doing what a careful person would do by hand, so nobody has to remember to.

## The Entity Factory

The factory is just a flat list of static methods, each naming a scenario and handing off to the builder:

```csharp
/// <summary>
/// Factory for quickly creating <see cref="VideoEntity"/> instances in tests.
/// </summary>
public static class VideoFactory
{
    /// <summary>
    /// Creates a free video in Draft status.
    /// </summary>
    public static VideoEntity Create(Guid categoryId) => new VideoBuilder(categoryId).Build();

    /// <summary>
    /// Creates a published free video (a YouTube URL is attached for you).
    /// </summary>
    public static VideoEntity CreatePublished(Guid categoryId) => new VideoBuilder(categoryId).AsPublished().Build();

    /// <summary>
    /// Creates an approved free video.
    /// </summary>
    public static VideoEntity CreateApproved(Guid categoryId) => new VideoBuilder(categoryId).AsApproved().Build();

    /// <summary>
    /// Creates a rejected free video.
    /// </summary>
    public static VideoEntity CreateRejected(Guid categoryId) => new VideoBuilder(categoryId).AsRejected().Build();

    /// <summary>
    /// Creates a paid video tied to a customer and an order item.
    /// </summary>
    public static VideoEntity CreatePaid(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new VideoBuilder(categoryId).WithCustomer(customerId, orderItemId).Build();

    /// <summary>
    /// Creates a list of published videos.
    /// </summary>
    public static List<VideoEntity> CreateManyPublished(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreatePublished(categoryId)).ToList();
}
```

Every method is a single line, and every one reads like a sentence. `CreatePublished` says "a published free video" in its name and in its body. `CreateManyPublished` hands you a whole page of distinct, valid videos with no loop in the test. And notice the factory never repeats any construction logic. It names a combination and lets the builder do the actual work.

## Why the Builder Is Hidden and the Factory Is Public

Look at the two declarations again. The builder is `internal`. The factory is `public`. That's on purpose, and it holds across the whole suite: all nineteen entity builders are internal, and every factory that wraps them is public.

It comes down to what a test should be allowed to say. An entity in a test should always be a state the real system could actually produce. If any test could grab the builder directly, sooner or later somebody assembles a half valid object to force a stubborn test green, and now that object has drifted away from anything production would ever create. Keeping the builder behind the factory means tests can only ask for named, approved setups. Need one the factory doesn't have yet? Add a method to the factory, out in the open, where the rest of the team can find it and reuse it.

So the rule is easy to remember: in a test you call the factory. The builder is there to keep the factory clean, not to be used on its own.

## The Factories Underneath the Builders

There's one more layer, and it clears up something the very first example left hanging. Go back to the builder and look at this line:

```csharp
VideoErrors errors = TestErrorsFactory.CreateVideoErrors();
```

Remember that dependency injection container you had to stand up by hand just to get an errors object? It didn't disappear. It moved down here, into a factory every builder shares, written once. The builder never constructs that facade itself. It calls another factory:

```csharp
/// <summary>
/// Builds real *Errors instances backed by embedded .resx resources for use in test builders.
/// </summary>
public static class TestErrorsFactory
{
    /// <summary>
    /// Creates a real <see cref="VideoErrors"/> facade for the video entity builder.
    /// </summary>
    /// <returns>A configured video errors instance.</returns>
    public static VideoErrors CreateVideoErrors() =>
        new(LocalizerFactory.CreateMessage<VideoErrorMessage>());

    // one method per domain: CreateShortVideoErrors, CreateTagErrors, and so on
}
```

`TestErrorsFactory` builds dependencies, not entities, and it sits on top of `LocalizerFactory`. That localizer factory is exactly where the `ServiceCollection`, `AddLocalization`, resolve dance from the opening example ended up, written down one time so no test ever types it again. The layers stack: a test calls an entity factory, the factory drives a builder, the builder asks the errors factory for a real errors object, and that asks the localizer factory for real messages. Each layer hides one flavor of ugliness so the one above it stays short.

Two more helpers turn up everywhere. `TestConstants` holds the known good literals, the titles, slugs, and descriptions builders start from. And `Bogus` (that `Faker` field) covers the cases where variety or uniqueness matters more than a fixed value. Constants for what should be predictable, Faker for what should vary, and either way the builder starts from something valid.

## What the Tests Look Like Now

Go back to that twenty line arrange block from the start. With the factory it shrinks to this:

```csharp
VideoEntity published = VideoFactory.CreatePublished(category.Id);
VideoEntity rejected = VideoFactory.CreateRejected(category.Id);
```

The setup finally says what it means. One published, one rejected. No container, no errors object, no slug wrangling, no lifecycle steps.

But two lines out of context don't prove much, so here's a real one, top to bottom. It rates a published video and checks the rating actually landed in the database. The part to watch is the arrange:

```csharp
/// <summary>
/// Integration tests for the PublicRateVideo endpoint. The base class supplies
/// <c>Client</c> (an HttpClient to the test server), SeedAsync, and CreateDbContext.
/// </summary>
public class PublicRateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Seeds a published video with its category and content type, and returns it.
    /// </summary>
    private async Task<VideoEntity> SeedPublishedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = VideoFactory.CreatePublished(category.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);

            return video;
        });
    }

    /// <summary>
    /// A visitor rating a published video stores the rating against that user.
    /// </summary>
    [Fact]
    public async Task RateVideo_AsVisitor_WithValidData_StoresRating()
    {
        // Arrange
        Client.AuthenticateAsVisitor();
        VideoEntity video = await SeedPublishedVideoAsync();
        PublicRateVideoRequest request = new PublicRateVideoRequestBuilder().Build();

        // Act
        var response = await Client.PostAsJsonAsync(Routes.Public.Videos.Ratings(video.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        VideoRatingEntity? rating = await verifyDb.VideoRatings.FirstOrDefaultAsync(r =>
            r.VideoId == video.Id && r.UserId == TestUser.VisitorId
        );
        rating.Should().NotBeNull();
        rating!.Stars.Should().Be(request.Stars);
    }
}
```

Look at what the arrange is doing. A video can't exist without a category, and a category can't exist without a content type, so there's a three step chain in `SeedPublishedVideoAsync`, and it's three factory calls. The request is a single builder call. Everything after `// Act` is the actual test. If the video model grows a new required field tomorrow, not one line of this test moves, because all of that lives inside `VideoFactory` and `VideoBuilder`.

And it's the same story across modules. A file over in core and a user over in identity expose the same builder plus factory shape, so pulling a thumbnail file or an author into a test reads like a story about how things connect instead of a pile of setup.

## Keeping the Defaults Honest

There's one rule this whole thing lives or dies by: the default a builder produces has to be valid.

If `new VideoBuilder(categoryId).Build()` handed back something the domain rejected, every test would have to patch it up first, and you'd be right back where you started. So the defaults are picked to clear every invariant: a real title, a unique slug, a believable description, Draft status as the plainest legal starting point. The builder gives you the most boring legal version of the thing, and each `With` and `As` nudges it toward one specific case.

It's also why the builders go through the real creation methods and lifecycle calls instead of reflection or some bypassed constructor. Forcing fields into place and skipping the invariants would be easy, and it would let tests build objects production never could. Then the tests slowly stop matching reality. By going through `CreateFree`, `CreatePaid`, and the actual `MarkPendingReview`, `Approve`, and `Publish` calls, whatever the builder returns is a state the real system can genuinely reach.

## What I Got Out of It

Once this was the standard across the codebase, a few things got noticeably better.

- **Arrange blocks became about intent.** Setup names the scenario and stops. You read "published plus rejected" or "a draft" instead of a recipe.
- **Domain changes stopped wrecking the suite.** When the entity picked up a new field, the builder was the one place to look. No test mentioned the field, so no test had to change.
- **Uniqueness stopped biting.** Constraint violations don't surprise me anymore, because the builders mint unique titles and slugs on their own.
- **Edge cases got names.** `CreatePublished`, `CreateRejected`, `CreatePaid` turned vague setup into stated intent. A new teammate can guess each one without opening the builder.
- **Validation tests are one liners.** A public request builder and one overridden field is enough to prove an endpoint rejects bad input.
- **Learn one, read them all.** A hundred builders and thirty factories sounds like a lot, but it's the same handful of shapes over and over. Read `VideoFactory` once and you can read every factory in the project on sight.

The price is real but small. Every entity and request gets a little builder, entities get a factory too, and you have to keep the defaults valid as the model changes. But these files are short, they barely move, and they put all the construction knowledge in one place. I'll take a tidy builder and a one line factory over a twenty line arrange block copy pasted across a hundred tests, every time.
