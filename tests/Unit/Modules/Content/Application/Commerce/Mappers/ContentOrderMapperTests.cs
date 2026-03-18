using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Mappers;

/// <summary>
/// Unit tests for <see cref="ContentOrderMapper"/> extension methods.
/// </summary>
public class ContentOrderMapperTests : BaseContentHandlerTest, IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ContentOrderRepository _repository;

    public ContentOrderMapperTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ContentDbContext(options);
        _repository = new ContentOrderRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region ToFileDto

    [Fact]
    public void ToFileDto_WhenEntityIsNull_ShouldReturnNull()
    {
        FileEntity? fileEntity = null;

        FileDto? result = fileEntity.ToFileDto(Mapper);

        result.Should().BeNull();
    }

    [Fact]
    public void ToFileDto_WhenEntityIsNotNull_ShouldMapToFileDto()
    {
        FileEntity fileEntity = FileFactory.CreateWithTestValues();

        FileDto? result = fileEntity.ToFileDto(Mapper);

        result.Should().NotBeNull();
        result!.Id.Should().Be(fileEntity.Id);
        result.StorageUrl.Should().Be(fileEntity.StorageUrl);
        result.MimeType.Should().Be(fileEntity.MimeType);
        result.OriginalFileName.Should().Be(fileEntity.OriginalFileName);
        result.SizeInBytes.Should().Be(fileEntity.SizeInBytes);
    }

    #endregion

    #region ToPaymentDto

    [Fact]
    public void ToPaymentDto_WithNullProofFile_ShouldReturnDtoWithNullProof()
    {
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);

        PaymentDto result = payment.ToPaymentDto(Mapper, proofFile: null);

        result.Should().NotBeNull();
        result.PaymentProof.Should().BeNull();
        result.Id.Should().Be(payment.Id);
    }

    [Fact]
    public void ToPaymentDto_WithProofFile_ShouldInjectProofFile()
    {
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        var proofFile = new FileDto(
            Guid.NewGuid(),
            "https://example.com/proof.pdf",
            "application/pdf",
            "proof.pdf",
            10240
        );

        PaymentDto result = payment.ToPaymentDto(Mapper, proofFile: proofFile);

        result.Should().NotBeNull();
        result.PaymentProof.Should().NotBeNull();
        result.PaymentProof!.Id.Should().Be(proofFile.Id);
    }

    #endregion

    #region ToContentOrderSummaryDto

    [Fact]
    public async Task ToContentOrderSummaryDto_ShouldMapCustomerNameAndStatus()
    {
        // Arrange — seed customer + order in InMemory DB to get navigation properties
        CustomerEntity customer = CustomerFactory.CreateDefault();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        ContentOrderEntity? loaded = await _context
            .ContentOrders.Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        // Act
        ContentOrderSummaryDto dto = loaded!.ToContentOrderSummaryDto(Mapper);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(order.Id);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.Status.Should().Be(order.Status.ToString());
        dto.ItemCount.Should().Be(0);
    }

    #endregion

    #region ToOrderItemDto

    [Fact]
    public async Task ToOrderItemDto_ShouldMapCategoryName()
    {
        // Arrange — seed content type, category, order, item
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
        _context.ContentTypes.Add(contentType);
        await _context.SaveChangesAsync();

        CategoryEntity category = CategoryFactory.CreatePaid(contentType.Id);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        CustomerEntity customer = CustomerFactory.Create();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, category.Id);
        await _repository.AddItemAsync(item);
        await _context.SaveChangesAsync();

        ContentOrderItemEntity? loaded = await _context
            .ContentOrderItems.Include(i => i.Category)
            .Include(i => i.PromotionLevel)
            .Include(i => i.Tiers)
            .FirstOrDefaultAsync(i => i.Id == item.Id);

        // Act
        OrderItemDto dto = loaded!.ToOrderItemDto(Mapper);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(item.Id);
        dto.CategoryName.Should().Be(category.Name);
        dto.PromotionLevelName.Should().BeNull();
        dto.Tiers.Should().BeEmpty();
    }

    #endregion
}
