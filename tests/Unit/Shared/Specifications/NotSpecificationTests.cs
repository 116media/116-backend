using System.Linq.Expressions;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Specifications;

/// <summary>
/// Unit tests for <see cref="NotSpecification{T}"/>.
/// </summary>
public class NotSpecificationTests
{
    #region Test Models and Specifications

    private class TestEntity
    {
        public int Value { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class GreaterThanSpecification(int threshold) : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Value > threshold;
        }
    }

    private class IsActiveSpecification : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.IsActive;
        }
    }

    private class NameContainsSpecification(string substring) : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Name.Contains(substring);
        }
    }

    private class LessThanSpecification(int threshold) : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Value < threshold;
        }
    }

    #endregion

    #region Constructor and ToExpression Tests

    [Fact]
    public void Constructor_WithSpecification_ShouldCreateNotSpecification()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);

        // Act
        NotSpecification<TestEntity> notSpec = new(inner);

        // Assert
        notSpec.Should().NotBeNull();
        notSpec.ToExpression().Should().NotBeNull();
    }

    [Fact]
    public void ToExpression_ShouldReturnValidExpression()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);

        // Act
        Expression<Func<TestEntity, bool>> expression = notSpec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region Logical NOT Behavior Tests

    [Fact]
    public void IsSatisfiedBy_WithSatisfiedInnerSpecification_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);
        TestEntity entity = new() { Value = 10 };

        // Act
        bool result = notSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("NOT(value > 5) should be false when value is 10");
    }

    [Fact]
    public void IsSatisfiedBy_WithUnsatisfiedInnerSpecification_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);
        TestEntity entity = new() { Value = 3 };

        // Act
        bool result = notSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("NOT(value > 5) should be true when value is 3");
    }

    [Fact]
    public void IsSatisfiedBy_WithBoundaryValue_ShouldNegateCorrectly()
    {
        // Arrange - value = 5, inner spec checks > 5, so inner returns false
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);
        TestEntity entity = new() { Value = 5 };

        // Act
        bool result = notSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("NOT(5 > 5) = NOT(false) = true");
    }

    #endregion

    #region Boolean Property Tests

    [Fact]
    public void IsSatisfiedBy_WithTrueBooleanProperty_ShouldReturnFalse()
    {
        // Arrange
        IsActiveSpecification inner = new();
        NotSpecification<TestEntity> notSpec = new(inner);
        TestEntity entity = new() { IsActive = true };

        // Act
        bool result = notSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("NOT(isActive) should be false when isActive is true");
    }

    [Fact]
    public void IsSatisfiedBy_WithFalseBooleanProperty_ShouldReturnTrue()
    {
        // Arrange
        IsActiveSpecification inner = new();
        NotSpecification<TestEntity> notSpec = new(inner);
        TestEntity entity = new() { IsActive = false };

        // Act
        bool result = notSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("NOT(isActive) should be true when isActive is false");
    }

    #endregion

    #region Double Negation Tests

    [Fact]
    public void DoubleNegation_ShouldRestoreOriginalBehavior()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);
        NotSpecification<TestEntity> doubleNotSpec = new(notSpec);
        TestEntity entity = new() { Value = 10 };

        // Act
        bool result = doubleNotSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("NOT(NOT(value > 5)) should equal (value > 5)");
    }

    [Fact]
    public void DoubleNegation_WithFalseCondition_ShouldRestoreOriginalBehavior()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);
        NotSpecification<TestEntity> doubleNotSpec = new(notSpec);
        TestEntity entity = new() { Value = 3 };

        // Act
        bool result = doubleNotSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("NOT(NOT(3 > 5)) = NOT(NOT(false)) = false");
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void ToExpression_CompiledWithLinq_ShouldFilterCorrectly()
    {
        // Arrange - NOT(value > 10) = value <= 10
        GreaterThanSpecification inner = new(10);
        NotSpecification<TestEntity> notSpec = new(inner);

        List<TestEntity> entities =
        [
            new() { Value = 5 }, // Included: NOT(5 > 10) = true
            new() { Value = 10 }, // Included: NOT(10 > 10) = true
            new() { Value = 15 }, // Excluded: NOT(15 > 10) = false
            new() { Value = 20 }, // Excluded: NOT(20 > 10) = false
        ];

        // Act
        List<TestEntity> filtered = entities.Where(notSpec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(e => e.Value <= 10).Should().BeTrue();
        filtered.Should().Contain(e => e.Value == 5);
        filtered.Should().Contain(e => e.Value == 10);
    }

    #endregion

    #region Composition with And/Or Tests

    [Fact]
    public void Composition_NotWithAnd_ShouldWorkCorrectly()
    {
        // Arrange - NOT(value > 5) AND isActive = (value <= 5) AND isActive
        GreaterThanSpecification greaterThan5 = new(5);
        NotSpecification<TestEntity> notGreaterThan5 = new(greaterThan5);
        IsActiveSpecification isActive = new();
        AndSpecification<TestEntity> andSpec = new(notGreaterThan5, isActive);

        TestEntity entity1 = new() { Value = 3, IsActive = true }; // Satisfies both
        TestEntity entity2 = new() { Value = 3, IsActive = false }; // Satisfies first only
        TestEntity entity3 = new() { Value = 10, IsActive = true }; // Satisfies second only

        // Act & Assert
        andSpec.IsSatisfiedBy(entity1).Should().BeTrue("(value <= 5) AND isActive");
        andSpec.IsSatisfiedBy(entity2).Should().BeFalse("(value <= 5) BUT not active");
        andSpec.IsSatisfiedBy(entity3).Should().BeFalse("(value > 5) fails NOT condition");
    }

    [Fact]
    public void Composition_NotWithOr_ShouldWorkCorrectly()
    {
        // Arrange - NOT(value > 10) OR name contains "Test"
        GreaterThanSpecification greaterThan10 = new(10);
        NotSpecification<TestEntity> notGreaterThan10 = new(greaterThan10);
        NameContainsSpecification nameContainsTest = new("Test");
        OrSpecification<TestEntity> orSpec = new(notGreaterThan10, nameContainsTest);

        TestEntity entity1 = new() { Value = 5, Name = "Sample" }; // Satisfies NOT only
        TestEntity entity2 = new() { Value = 15, Name = "Test" }; // Satisfies name only
        TestEntity entity3 = new() { Value = 15, Name = "Sample" }; // Satisfies neither

        // Act & Assert
        orSpec.IsSatisfiedBy(entity1).Should().BeTrue("value <= 10");
        orSpec.IsSatisfiedBy(entity2).Should().BeTrue("name contains 'Test'");
        orSpec.IsSatisfiedBy(entity3).Should().BeFalse("(value > 10) AND name doesn't contain 'Test'");
    }

    [Fact]
    public void Composition_NotOfAnd_ShouldWorkCorrectly()
    {
        // Arrange - NOT(value > 5 AND value < 15) = (value <= 5 OR value >= 15)
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        AndSpecification<TestEntity> andSpec = new(greaterThan5, lessThan15);
        NotSpecification<TestEntity> notAndSpec = new(andSpec);

        TestEntity entity1 = new() { Value = 3 }; // <= 5, should satisfy NOT
        TestEntity entity2 = new() { Value = 10 }; // Between 5 and 15, should NOT satisfy NOT
        TestEntity entity3 = new() { Value = 20 }; // >= 15, should satisfy NOT

        // Act & Assert
        notAndSpec.IsSatisfiedBy(entity1).Should().BeTrue("value <= 5");
        notAndSpec.IsSatisfiedBy(entity2).Should().BeFalse("value is between 5 and 15");
        notAndSpec.IsSatisfiedBy(entity3).Should().BeTrue("value >= 15");
    }

    [Fact]
    public void Composition_NotOfOr_ShouldWorkCorrectly()
    {
        // Arrange - NOT(value > 20 OR name contains "Test") = (value <= 20 AND name doesn't contain "Test")
        GreaterThanSpecification greaterThan20 = new(20);
        NameContainsSpecification nameContainsTest = new("Test");
        OrSpecification<TestEntity> orSpec = new(greaterThan20, nameContainsTest);
        NotSpecification<TestEntity> notOrSpec = new(orSpec);

        TestEntity entity1 = new() { Value = 15, Name = "Sample" }; // Satisfies NOT
        TestEntity entity2 = new() { Value = 25, Name = "Sample" }; // Fails: value > 20
        TestEntity entity3 = new() { Value = 15, Name = "Test" }; // Fails: name contains "Test"

        // Act & Assert
        notOrSpec.IsSatisfiedBy(entity1).Should().BeTrue("value <= 20 AND name doesn't contain 'Test'");
        notOrSpec.IsSatisfiedBy(entity2).Should().BeFalse("value > 20");
        notOrSpec.IsSatisfiedBy(entity3).Should().BeFalse("name contains 'Test'");
    }

    #endregion

    #region Expression Tree Tests

    [Fact]
    public void ToExpression_ShouldUseNotOperator()
    {
        // Arrange
        GreaterThanSpecification inner = new(5);
        NotSpecification<TestEntity> notSpec = new(inner);

        // Act
        Expression<Func<TestEntity, bool>> expression = notSpec.ToExpression();

        // Assert
        expression.Body.Should().NotBeNull();
        expression.Body.NodeType.Should().Be(ExpressionType.Not, "should use Not operator");
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void ComplexScenario_DeMorgansLaw_AndToOr()
    {
        // De Morgan's Law: NOT(A AND B) = NOT(A) OR NOT(B)
        // Testing: NOT(value > 5 AND value < 15) should behave like (value <= 5 OR value >= 15)

        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        AndSpecification<TestEntity> andSpec = new(greaterThan5, lessThan15);
        NotSpecification<TestEntity> notAnd = new(andSpec);

        TestEntity entity1 = new() { Value = 3 }; // <= 5
        TestEntity entity2 = new() { Value = 10 }; // Between 5 and 15
        TestEntity entity3 = new() { Value = 20 }; // >= 15

        // Act & Assert
        notAnd.IsSatisfiedBy(entity1).Should().BeTrue("satisfies NOT(A AND B) via NOT(A)");
        notAnd.IsSatisfiedBy(entity2).Should().BeFalse("satisfies (A AND B), so NOT(A AND B) fails");
        notAnd.IsSatisfiedBy(entity3).Should().BeTrue("satisfies NOT(A AND B) via NOT(B)");
    }

    #endregion
}
