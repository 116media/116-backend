using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag.V1;

/// <summary>
/// Integration tests for the AdminDeleteTag endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteTagEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteTag_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTag_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Tag"))
        );
    }

    [Fact]
    public async Task DeleteTag_AsSuperAdmin_WithValidId_RemovesTag()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create();
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Tags}/{tag.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminDeleteTagResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        (await context.Tags.AnyAsync(t => t.Id == tag.Id)).Should().BeFalse();
    }
}
