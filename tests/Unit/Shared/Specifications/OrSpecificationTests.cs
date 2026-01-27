using System.Linq.Expressions;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Specifications;

/// <summary>
/// Unit tests for <see cref="OrSpecification{T}"/>.
/// </summary>
public class OrSpecificationTests
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
    public void Constructor_WithTwoSpecifications_ShouldCreateOrSpecification()
    {
        // Arrange
        GreaterThanSpecification left = new(20);
        NameContainsSpecification right = new("Test");

        // Act
        OrSpecification<TestEntity> orSpec = new(left, right);

        // Assert
        orSpec.Should().NotBeNull();
        orSpec.ToExpression().Should().NotBeNull();
    }

    [Fact]
    public void ToExpression_ShouldReturnValidExpression()
    {
        // Arrange
        GreaterThanSpecification left = new(20);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);

        // Act
        Expression<Func<TestEntity, bool>> expression = orSpec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region Logical OR Behavior Tests

    [Fact]
    public void IsSatisfiedBy_WithBothSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);
        TestEntity entity = new() { Value = 10, Name = "Test Entity" };

        // Act
        bool result = orSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("both value > 5 and name contains 'Test'");
    }

    [Fact]
    public void IsSatisfiedBy_WithOnlyLeftSpecificationSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);
        TestEntity entity = new() { Value = 10, Name = "Sample" };

        // Act
        bool result = orSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("value > 5, even though name doesn't contain 'Test'");
    }

    [Fact]
    public void IsSatisfiedBy_WithOnlyRightSpecificationSatisfied_ShouldReturnTrue()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);
        TestEntity entity = new() { Value = 3, Name = "Test Entity" };

        // Act
        bool result = orSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("name contains 'Test', even though value is not > 5");
    }

    [Fact]
    public void IsSatisfiedBy_WithNeitherSpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);
        TestEntity entity = new() { Value = 3, Name = "Sample" };

        // Act
        bool result = orSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse("neither value > 5 nor name contains 'Test'");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void IsSatisfiedBy_WithBoundaryValueOnLeft_ShouldStillCheckRight()
    {
        // Arrange - value exactly at boundary (5), should fail left but check right
        GreaterThanSpecification left = new(5);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);
        TestEntity entity = new() { Value = 5, Name = "Test Entity" };

        // Act
        bool result = orSpec.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue("fails left (5 is not > 5) but passes right (name contains 'Test')");
    }

    #endregion

    #region Different Property Tests

    [Fact]
    public void IsSatisfiedBy_WithCompletelyDifferentProperties_ShouldWorkCorrectly()
    {
        // Arrange
        GreaterThanSpecification greaterThan20 = new(20);
        NameContainsSpecification nameContainsSpecial = new("Special");
        OrSpecification<TestEntity> orSpec = new(greaterThan20, nameContainsSpecial);

        TestEntity entity1 = new() { Value = 25, Name = "Regular" }; // Satisfies left only
        TestEntity entity2 = new() { Value = 15, Name = "Special Case" }; // Satisfies right only
        TestEntity entity3 = new() { Value = 15, Name = "Regular" }; // Satisfies neither

        // Act & Assert
        orSpec.IsSatisfiedBy(entity1).Should().BeTrue("value > 20");
        orSpec.IsSatisfiedBy(entity2).Should().BeTrue("name contains 'Special'");
        orSpec.IsSatisfiedBy(entity3).Should().BeFalse("satisfies neither");
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void ToExpression_CompiledWithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        GreaterThanSpecification left = new(15);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);

        List<TestEntity> entities =
        [
            new() { Value = 10, Name = "Sample" }, // Excluded: neither condition
            new() { Value = 20, Name = "Sample" }, // Included: value > 15
            new() { Value = 10, Name = "Test Entity" }, // Included: name contains "Test"
            new() { Value = 25, Name = "Test Case" }, // Included: both conditions
        ];

        // Act
        List<TestEntity> filtered = entities.Where(orSpec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(3);
        filtered.Should().NotContain(e => e.Value == 10 && e.Name == "Sample");
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void Chaining_MultipleOrSpecifications_ShouldWorkCorrectly()
    {
        // Arrange
        GreaterThanSpecification greaterThan20 = new(20);
        LessThanSpecification lessThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");

        // Create nested OR: (greaterThan20 OR lessThan5) OR nameContainsTest
        OrSpecification<TestEntity> firstOr = new(greaterThan20, lessThan5);
        OrSpecification<TestEntity> secondOr = new(firstOr, nameContainsTest);

        TestEntity entity1 = new() { Value = 25, Name = "Sample" }; // Satisfies first spec
        TestEntity entity2 = new() { Value = 3, Name = "Sample" }; // Satisfies second spec
        TestEntity entity3 = new() { Value = 10, Name = "Test" }; // Satisfies third spec
        TestEntity entity4 = new() { Value = 10, Name = "Sample" }; // Satisfies none

        // Act & Assert
        secondOr.IsSatisfiedBy(entity1).Should().BeTrue("value > 20");
        secondOr.IsSatisfiedBy(entity2).Should().BeTrue("value < 5");
        secondOr.IsSatisfiedBy(entity3).Should().BeTrue("name contains 'Test'");
        secondOr.IsSatisfiedBy(entity4).Should().BeFalse("satisfies no condition");
    }

    #endregion

    #region Mixed Composition Tests

    [Fact]
    public void MixedComposition_OrWithinAnd_ShouldWorkCorrectly()
    {
        // Arrange - (value > 5 OR name contains "Test") AND value < 20
        GreaterThanSpecification greaterThan5 = new(5);
        NameContainsSpecification nameContainsTest = new("Test");
        LessThanSpecification lessThan20 = new(20);

        OrSpecification<TestEntity> orSpec = new(greaterThan5, nameContainsTest);
        AndSpecification<TestEntity> andSpec = new(orSpec, lessThan20);

        TestEntity entity1 = new() { Value = 10, Name = "Sample" }; // (true OR false) AND true = true
        TestEntity entity2 = new() { Value = 3, Name = "Test" }; // (false OR true) AND true = true
        TestEntity entity3 = new() { Value = 25, Name = "Sample" }; // (true OR false) AND false = false

        // Act & Assert
        andSpec.IsSatisfiedBy(entity1).Should().BeTrue("(value > 5) AND (value < 20)");
        andSpec.IsSatisfiedBy(entity2).Should().BeTrue("(name has Test) AND (value < 20)");
        andSpec.IsSatisfiedBy(entity3).Should().BeFalse("value not < 20");
    }

    #endregion

    #region Expression Tree Tests

    [Fact]
    public void ToExpression_ShouldUseOrElseOperator()
    {
        // Arrange
        GreaterThanSpecification left = new(5);
        NameContainsSpecification right = new("Test");
        OrSpecification<TestEntity> orSpec = new(left, right);

        // Act
        Expression<Func<TestEntity, bool>> expression = orSpec.ToExpression();

        // Assert
        expression.Body.Should().NotBeNull();
        expression.Body.NodeType.Should().Be(ExpressionType.OrElse, "should use OrElse operator");
    }

    #endregion
}
