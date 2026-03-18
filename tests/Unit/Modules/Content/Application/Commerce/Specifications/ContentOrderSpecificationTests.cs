using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Specifications;

/// <summary>
/// Unit tests for Commerce order specifications.
/// </summary>
public class ContentOrderSpecificationTests
{
    #region ContentOrderByIdSpecification

    [Fact]
    public void ByIdSpec_WhenIdMatches_ShouldReturnTrue()
    {
        Guid id = Guid.NewGuid();
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(id);
        var spec = new ContentOrderByIdSpecification(id);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void ByIdSpec_WhenIdDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        var spec = new ContentOrderByIdSpecification(Guid.NewGuid());

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderByStatusSpecification

    [Fact]
    public void ByStatusSpec_WhenStatusMatches_ShouldReturnTrue()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        var spec = new ContentOrderByStatusSpecification(EnumOrderStatus.Draft);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void ByStatusSpec_WhenStatusDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        var spec = new ContentOrderByStatusSpecification(EnumOrderStatus.Paid);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderByCustomerIdSpecification

    [Fact]
    public void ByCustomerIdSpec_WhenCustomerIdMatches_ShouldReturnTrue()
    {
        Guid customerId = Guid.NewGuid();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customerId);
        var spec = new ContentOrderByCustomerIdSpecification(customerId);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void ByCustomerIdSpec_WhenCustomerIdDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(Guid.NewGuid());
        var spec = new ContentOrderByCustomerIdSpecification(Guid.NewGuid());

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeFalse();
    }

    #endregion
}
