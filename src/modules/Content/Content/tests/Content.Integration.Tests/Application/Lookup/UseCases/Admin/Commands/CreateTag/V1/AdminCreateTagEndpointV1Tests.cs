using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Content.Integration.Tests.Application.Lookup.UseCases.Admin.Commands.CreateTag.V1;

/// <summary>
/// Integration tests for the AdminCreateTag endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateTagEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateTag_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminCreateTagRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTag_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new AdminCreateTagRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTag_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreateTagRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        await response.ShouldBeValidationProblem("Name", Localized<TagErrorMessage>(m => m.NameRequired()));
    }

    [Fact]
    public async Task CreateTag_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreateTagRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreateTagResponse>();
        body.Tag.Id.Should().NotBeEmpty();
        body.Tag.Name.Should().Be(request.Name);
        body.Tag.Slug.Should().Be(request.Slug);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        TagEntity? persisted = await context.Tags.FindAsync(body.Tag.Id);
        persisted.Should().NotBeNull();
        persisted!.Slug.Value.Should().Be(request.Slug);
    }

    [Fact]
    public async Task CreateTag_AsSuperAdmin_DuplicateSlug_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity existingTag = TagFactory.Create("Fally Ipupa", "fally-ipupa");
            ctx.Tags.Add(existingTag);
            return existingTag;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreateTagRequestBuilder().WithSlug("fally-ipupa").Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Tags, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<TagErrorMessage>(m => m.SlugAlreadyExists(request.Slug))
        );
    }
}
