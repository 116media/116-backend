using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType.V1;

/// <summary>
/// Integration tests for the AdminDeactivateContentType endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateContentTypeEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<bool> IsContentTypeActiveAsync(Guid id)
    {
        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        ContentTypeEntity? contentType = await context.ContentTypes.FindAsync(id);
        return contentType!.IsActive;
    }

    [Fact]
    public async Task DeactivateContentType_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.ContentTypes, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateContentType_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.ContentTypes, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentType"))
        );
    }

    [Fact]
    public async Task DeactivateContentType_AsSuperAdmin_WithValidId_ReturnsOk()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.ContentTypes, contentType.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsContentTypeActiveAsync(contentType.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateContentType_AsSuperAdmin_AlreadyInactive_ReturnsConflict()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.CreateInactive();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.ContentTypes, contentType.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentTypeErrorMessage>(m => m.AlreadyInactive())
        );
        (await IsContentTypeActiveAsync(contentType.Id)).Should().BeFalse();
    }
}
