using System.Linq.Expressions;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Specifications;

/// <summary>
/// Unit tests for <see cref="AndSpecification{T}"/>.
/// </summary>
public class AndSpecificationTests
{
    #region Test Models and Specifications

    private class TestEntity
    {
        public int Value { get; set; }
        public bool IsActive { get; set; }
    }

    private class GreaterThanSpecification(int threshold) : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Value > threshold;
        }
    }

    private class LessThanSpecification(int threshold) : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Value < threshold;
        }
    }

    private class IsActiveSpecification : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.IsActive;
        }
    }

    #endregion

    #region Constructor and ToExpression Tests

    [Fact]
    public void Constructor_WithTwoSpecifications_ShouldCreateAndSpecification()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);

        // Act
        AndSpecification<TestEntity> andSpec = new(left, right);

        // Assert
        andSpec.Should().NotBeNull();
        andSpec.ToExpression().Should().NotBeNull();
    }

    [Fact]
    public void ToExpression_ShouldReturnValidExpression()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);

        // Act
        Expression<Func<TestEntity, bool>> expression = andSpec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region Logical AND Behavior Tests

    [Fact]
    public void IsSatisfiedBy_WithBothSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);
        TestEntity entity = new() { Value = 10 };

        // Act
        bool result = andSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("value 10 is both > 5 and < 15");
    }

    [Fact]
    public void IsSatisfiedBy_WithOnlyLeftSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);
        TestEntity entity = new() { Value = 20 };

        // Act
        bool result = andSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("value 20 is > 5 but not < 15");
    }

    [Fact]
    public void IsSatisfiedBy_WithOnlyRightSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);
        TestEntity entity = new() { Value = 3 };

        // Act
        bool result = andSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("value 3 is < 15 but not > 5");
    }

    [Fact]
    public void IsSatisfiedBy_WithNeitherSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification left = new(10);
        LessThanSpecification right = new(5);
        AndSpecification<TestEntity> andSpec = new(left, right);
        TestEntity entity = new() { Value = 7 };

        // Act
        bool result = andSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("value 7 is neither > 10 nor < 5");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void IsSatisfiedBy_WithBoundaryValueOnLeft_ShouldBehaveLikeGreaterThan()
    {
        // Arrange - Testing exact boundary (value = 5, threshold = 5)
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);
        TestEntity entity = new() { Value = 5 };

        // Act
        bool result = andSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("value 5 is not > 5 (boundary case)");
    }

    [Fact]
    public void IsSatisfiedBy_WithBoundaryValueOnRight_ShouldBehaveLikeLessThan()
    {
        // Arrange - Testing exact boundary (value = 15, threshold = 15)
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);
        TestEntity entity = new() { Value = 15 };

        // Act
        bool result = andSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("value 15 is not < 15 (boundary case)");
    }

    #endregion

    #region Different Property Tests

    [Fact]
    public void IsSatisfiedBy_WithDifferentProperties_ShouldWorkCorrectly()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        IsActiveSpecification isActive = new();
        AndSpecification<TestEntity> andSpec = new(greaterThan5, isActive);

        TestEntity activeEntity = new() { Value = 10, IsActive = true };
        TestEntity inactiveEntity = new() { Value = 10, IsActive = false };

        // Act & Assert
        andSpec.IsSatisfiedBy(activeEntity).Should().BeTrue("value > 5 AND isActive");
        andSpec.IsSatisfiedBy(inactiveEntity).Should().BeFalse("value > 5 BUT not active");
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void ToExpression_CompiledWithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);

        List<TestEntity> entities =
        [
            new() { Value = 3 }, // Excluded: not > 5
            new() { Value = 7 }, // Included: between 5 and 15
            new() { Value = 10 }, // Included: between 5 and 15
            new() { Value = 20 }, // Excluded: not < 15
        ];

        // Act
        List<TestEntity> filtered = entities.Where(andSpec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(e => e.Value > 5 && e.Value < 15);
        filtered.Should().Contain(e => e.Value == 7);
        filtered.Should().Contain(e => e.Value == 10);
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void Chaining_MultipleAndSpecifications_ShouldWorkCorrectly()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        IsActiveSpecification isActive = new();

        // Create nested AND: (greaterThan5 AND lessThan15) AND isActive
        AndSpecification<TestEntity> firstAnd = new(greaterThan5, lessThan15);
        AndSpecification<TestEntity> secondAnd = new(firstAnd, isActive);

        TestEntity matchingEntity = new() { Value = 10, IsActive = true };
        TestEntity nonMatchingEntity = new() { Value = 10, IsActive = false };

        // Act & Assert
        secondAnd.IsSatisfiedBy(matchingEntity).Should().BeTrue("satisfies all three conditions");
        secondAnd.IsSatisfiedBy(nonMatchingEntity).Should().BeFalse("fails isActive condition");
    }

    #endregion

    #region Expression Tree Tests

    [Fact]
    public void ToExpression_ShouldUseAndAlsoOperator()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        LessThanSpecification right = new(15);
        AndSpecification<TestEntity> andSpec = new(left, right);

        // Act
        Expression<Func<TestEntity, bool>> expression = andSpec.ToExpression();

        // Assert
        expression.Body.Should().NotBeNull();
        expression.Body.NodeType.Should().Be(ExpressionType.AndAlso, "should use AndAlso operator");
    }

    #endregion
}
