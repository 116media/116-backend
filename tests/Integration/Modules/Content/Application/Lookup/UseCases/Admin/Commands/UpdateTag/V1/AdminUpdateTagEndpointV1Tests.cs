using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;

/// <summary>
/// Integration tests for the AdminUpdateTag endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateTagEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task UpdateTag_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminUpdateTagRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateTagRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Tag"))
        );
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create();
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateTagRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{tag.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateTagResponse>();
        body.Tag.Id.Should().Be(tag.Id);
        body.Tag.Name.Should().Be(request.Name);
        body.Tag.Slug.Should().Be(request.Slug);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        TagEntity? persisted = await context.Tags.FindAsync(tag.Id);
        persisted!.Name.Should().Be(request.Name);
        persisted.Slug.Should().Be(request.Slug);
    }

    [Fact]
    public async Task UpdateTag_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateTagRequestBuilder()
            .WithSlug(new string('a', TestConstants.Tag.SlugMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<TagErrorMessage>(m => m.SlugTooLong(ContentConstants.MaxTagSlugLength)))
        );
    }

    [Fact]
    public async Task UpdateTag_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateTagRequestBuilder()
            .WithName(new string('T', TestConstants.Tag.NameMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<TagErrorMessage>(m => m.NameTooLong(ContentConstants.MaxTagNameLength)))
        );
    }

    [Fact]
    public async Task UpdateTag_WithInvalidSlugFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateTagRequestBuilder().WithSlug("INVALID SLUG!!!").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<TagErrorMessage>(m => m.SlugInvalidFormat()))
        );
    }
}
