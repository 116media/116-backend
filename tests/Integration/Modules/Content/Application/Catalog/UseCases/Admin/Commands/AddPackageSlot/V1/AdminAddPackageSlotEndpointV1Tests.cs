using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;

/// <summary>
/// Integration tests for the AdminAddPackageSlot endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddPackageSlotEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private async Task<PackageEntity> SeedPackageAsync()
    {
        return await SeedAsync<ContentDbContext, PackageEntity>(ctx =>
        {
            PackageEntity package = PackageFactory.Create();
            ctx.Packages.Add(package);
            return package;
        });
    }

    [Fact]
    public async Task AddPackageSlot_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = true,
            Quantity = 1,
        };

        var response = await Client.PostAsJsonAsync(Routes.Admin.Packages.Slots(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddPackageSlot_AsAdmin_ReturnsForbidden()
    {
        PackageEntity package = await SeedPackageAsync();

        Client.AuthenticateAsAdmin();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = true,
            Quantity = 1,
        };

        var response = await Client.PostAsJsonAsync(Routes.Admin.Packages.Slots(package.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_WithInvalidQuantity_ReturnsValidationError()
    {
        PackageEntity package = await SeedPackageAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminAddPackageSlotRequest request = new AdminAddPackageSlotRequestBuilder().WithQuantity(0).Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Packages.Slots(package.Id), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Quantity", Localized<PackageErrorMessage>(m => m.SlotQuantityMustBePositive()))
        );
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_NonExistentPackage_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminAddPackageSlotRequest request = new AdminAddPackageSlotRequestBuilder().WithQuantity(1).Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Packages.Slots(Guid.NewGuid()), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Package"))
        );
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CategoryEntity category = null!;
        PackageEntity package = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            package = PackageFactory.Create();
            ctx.Packages.Add(package);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddPackageSlotRequest request = new AdminAddPackageSlotRequestBuilder()
            .WithCategoryId(category.Id)
            .WithIsRequired(true)
            .WithQuantity(2)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Packages.Slots(package.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminAddPackageSlotResponse>();
        body.Package.Id.Should().Be(package.Id);
        body.Package.Slots.Should()
            .Contain(s =>
                s.CategoryId == request.CategoryId
                && s.Quantity == request.Quantity
                && s.IsRequired == request.IsRequired
            );

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        PackageSlotEntity? slot = await verifyContext.PackageSlots.FirstOrDefaultAsync(s =>
            s.PackageId == package.Id && s.CategoryId == category.Id
        );
        slot.Should().NotBeNull();
        slot!.Quantity.Should().Be(2);
        slot.IsRequired.Should().BeTrue();
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_WithNullCategory_ReturnsCreated()
    {
        PackageEntity package = await SeedPackageAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminAddPackageSlotRequest request = new AdminAddPackageSlotRequestBuilder()
            .WithCategoryId(null)
            .WithIsRequired(false)
            .WithQuantity(1)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Packages.Slots(package.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminAddPackageSlotResponse>();
        body.Package.Id.Should().Be(package.Id);
        body.Package.Slots.Should().Contain(s => s.CategoryId == null && s.Quantity == request.Quantity);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        PackageSlotEntity? slot = await verifyContext.PackageSlots.FirstOrDefaultAsync(s =>
            s.PackageId == package.Id && s.CategoryId == null
        );
        slot.Should().NotBeNull();
    }
}
