using System.Linq.Expressions;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Specifications;

/// <summary>
/// Unit tests for <see cref="Specification{T}"/>.
/// </summary>
public class SpecificationTests
{
    #region Test Models and Specifications

    private class TestEntity
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
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

    private class NameContainsSpecification(string substring) : Specification<TestEntity>
    {
        public override Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Name.Contains(substring);
        }
    }

    #endregion

    #region IsSatisfiedBy Tests

    [Fact]
    public void IsSatisfiedBy_WithMatchingEntity_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification spec = new(5);
        TestEntity entity = new() { Value = 10 };

        // Act
        bool result = spec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity value (10) is greater than threshold (5)");
    }

    [Fact]
    public void IsSatisfiedBy_WithNonMatchingEntity_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification spec = new(5);
        TestEntity entity = new() { Value = 3 };

        // Act
        bool result = spec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity value (3) is not greater than threshold (5)");
    }

    [Fact]
    public void IsSatisfiedBy_WithBoundaryValue_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification spec = new(5);
        TestEntity entity = new() { Value = 5 };

        // Act
        bool result = spec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity value (5) equals threshold (5), not greater");
    }

    #endregion

    #region And Method Tests

    [Fact]
    public void And_WithBothSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        TestEntity entity = new() { Value = 10 };

        // Act
        Specification<TestEntity> combined = greaterThan5.And(lessThan15);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity value (10) is between 5 and 15");
    }

    [Fact]
    public void And_WithOnlyFirstSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        TestEntity entity = new() { Value = 20 };

        // Act
        Specification<TestEntity> combined = greaterThan5.And(lessThan15);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity value (20) fails second specification (< 15)");
    }

    [Fact]
    public void And_WithOnlySecondSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        TestEntity entity = new() { Value = 3 };

        // Act
        Specification<TestEntity> combined = greaterThan5.And(lessThan15);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity value (3) fails first specification (> 5)");
    }

    [Fact]
    public void And_WithNeitherSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan10 = new(10);
        LessThanSpecification lessThan5 = new(5);
        TestEntity entity = new() { Value = 7 };

        // Act
        Specification<TestEntity> combined = greaterThan10.And(lessThan5);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity value (7) satisfies neither specification");
    }

    #endregion

    #region Or Method Tests

    [Fact]
    public void Or_WithBothSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = greaterThan5.Or(nameContainsTest);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity satisfies both specifications");
    }

    [Fact]
    public void Or_WithOnlyFirstSpecificationSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 10, Name = "Sample" };

        // Act
        Specification<TestEntity> combined = greaterThan5.Or(nameContainsTest);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity satisfies first specification");
    }

    [Fact]
    public void Or_WithOnlySecondSpecificationSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 3, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = greaterThan5.Or(nameContainsTest);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity satisfies second specification");
    }

    [Fact]
    public void Or_WithNeitherSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 3, Name = "Sample" };

        // Act
        Specification<TestEntity> combined = greaterThan5.Or(nameContainsTest);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity satisfies neither specification");
    }

    #endregion

    #region Not Method Tests

    [Fact]
    public void Not_WithSatisfiedSpecification_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        TestEntity entity = new() { Value = 10 };

        // Act
        Specification<TestEntity> negated = greaterThan5.Not();
        bool result = negated.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("negated specification should fail when original succeeds");
    }

    [Fact]
    public void Not_WithUnsatisfiedSpecification_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        TestEntity entity = new() { Value = 3 };

        // Act
        Specification<TestEntity> negated = greaterThan5.Not();
        bool result = negated.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("negated specification should succeed when original fails");
    }

    [Fact]
    public void Not_DoubleNegation_ShouldRestoreOriginalBehavior()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        TestEntity entity = new() { Value = 10 };

        // Act
        Specification<TestEntity> doubleNegated = greaterThan5.Not().Not();
        bool result = doubleNegated.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("double negation should restore original behavior");
    }

    #endregion

    #region AndAll Static Method Tests

    [Fact]
    public void AndAll_WithAllSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.AndAll(
            greaterThan5,
            lessThan15,
            nameContainsTest
        );
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity satisfies all three specifications");
    }

    [Fact]
    public void AndAll_WithOneSpecificationFailed_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        NameContainsSpecification nameContainsFoo = new("Foo");
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.AndAll(
            greaterThan5,
            lessThan15,
            nameContainsFoo
        );
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity fails third specification");
    }

    [Fact]
    public void AndAll_WithSingleSpecification_ShouldReturnSameResult()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        TestEntity entity = new() { Value = 10 };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.AndAll(greaterThan5);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("single specification should work as expected");
    }

    [Fact]
    public void AndAll_WithEmptyArray_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Specification<TestEntity>.AndAll([]);

        // Assert
        act.Should().Throw<ArgumentException>("empty specification array is invalid");
    }

    [Fact]
    public void AndAll_WithNullArray_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Specification<TestEntity>.AndAll(null!);

        // Assert
        act.Should().Throw<ArgumentException>("null specification array is invalid");
    }

    #endregion

    #region OrAll Static Method Tests

    [Fact]
    public void OrAll_WithAllSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.OrAll(
            greaterThan5,
            lessThan15,
            nameContainsTest
        );
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity satisfies all specifications");
    }

    [Fact]
    public void OrAll_WithOneSpecificationSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification greaterThan20 = new(20);
        LessThanSpecification lessThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.OrAll(
            greaterThan20,
            lessThan5,
            nameContainsTest
        );
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("entity satisfies at least one specification (name contains Test)");
    }

    [Fact]
    public void OrAll_WithNoSpecificationsSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification greaterThan20 = new(20);
        LessThanSpecification lessThan5 = new(5);
        NameContainsSpecification nameContainsFoo = new("Foo");
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.OrAll(greaterThan20, lessThan5, nameContainsFoo);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("entity satisfies no specifications");
    }

    [Fact]
    public void OrAll_WithSingleSpecification_ShouldReturnSameResult()
    {
        // Arrange
        GreaterThanSpecification greaterThan5 = new(5);
        TestEntity entity = new() { Value = 10 };

        // Act
        Specification<TestEntity> combined = Specification<TestEntity>.OrAll(greaterThan5);
        bool result = combined.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("single specification should work as expected");
    }

    [Fact]
    public void OrAll_WithEmptyArray_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Specification<TestEntity>.OrAll([]);

        // Assert
        act.Should().Throw<ArgumentException>("empty specification array is invalid");
    }

    [Fact]
    public void OrAll_WithNullArray_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Specification<TestEntity>.OrAll(null!);

        // Assert
        act.Should().Throw<ArgumentException>("null specification array is invalid");
    }

    #endregion

    #region ToExpression Tests

    [Fact]
    public void ToExpression_ShouldReturnValidExpression()
    {
        // Arrange
        GreaterThanSpecification spec = new(5);

        // Act
        Expression<Func<TestEntity, bool>> expression = spec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    [Fact]
    public void ToExpression_CompiledExpression_ShouldWorkWithLinq()
    {
        // Arrange
        GreaterThanSpecification spec = new(5);
        List<TestEntity> entities =
        [
            new() { Value = 3 },
            new() { Value = 7 },
            new() { Value = 10 },
            new() { Value = 2 },
        ];

        // Act
        List<TestEntity> filtered = entities.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(e => e.Value > 5).Should().BeTrue();
    }

    #endregion

    #region Complex Composition Tests

    [Fact]
    public void ComplexComposition_AndOrNot_ShouldWorkCorrectly()
    {
        // Arrange - (Value > 5 AND Value < 15) OR Name contains "Special"
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        NameContainsSpecification nameContainsSpecial = new("Special");

        Specification<TestEntity> complex = greaterThan5.And(lessThan15).Or(nameContainsSpecial);

        TestEntity entity1 = new() { Value = 10, Name = "Test" }; // Satisfies first part
        TestEntity entity2 = new() { Value = 20, Name = "Special Entity" }; // Satisfies second part
        TestEntity entity3 = new() { Value = 20, Name = "Test" }; // Satisfies neither

        // Act & Assert
        complex.IsSatisfiedBy(entity1).Should().BeTrue("value 10 is between 5 and 15");
        complex.IsSatisfiedBy(entity2).Should().BeTrue("name contains 'Special'");
        complex.IsSatisfiedBy(entity3).Should().BeFalse("satisfies neither condition");
    }

    [Fact]
    public void ComplexComposition_NestedAnd_ShouldWorkCorrectly()
    {
        // Arrange - ((Value > 5 AND Value < 15) AND Name contains "Test")
        GreaterThanSpecification greaterThan5 = new(5);
        LessThanSpecification lessThan15 = new(15);
        NameContainsSpecification nameContainsTest = new("Test");

        Specification<TestEntity> complex = greaterThan5.And(lessThan15).And(nameContainsTest);

        TestEntity matchingEntity = new() { Value = 10, Name = "Test Entity" };
        TestEntity nonMatchingEntity = new() { Value = 10, Name = "Sample" };

        // Act & Assert
        complex.IsSatisfiedBy(matchingEntity).Should().BeTrue("satisfies all three conditions");
        complex.IsSatisfiedBy(nonMatchingEntity).Should().BeFalse("name doesn't contain 'Test'");
    }

    #endregion
}
