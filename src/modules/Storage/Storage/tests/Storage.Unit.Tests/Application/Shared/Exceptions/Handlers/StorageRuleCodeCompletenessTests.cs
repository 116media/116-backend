using System.Reflection;
using _116.Storage.Application.Shared.Exceptions.Handlers;
using _116.Storage.Domain.StateMachines;
using AwesomeAssertions;
using Xunit;

namespace _116.Storage.Unit.Tests.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Guards that every rule code declared on <see cref="StorageRuleCodes"/> has a response in the
/// strategy's table, so a new rule cannot silently fall to the 400 fallback with a wrong status.
/// </summary>
public class StorageRuleCodeCompletenessTests
{
    [Fact]
    public void EveryDeclaredRuleCode_ShouldHaveAStrategyResponse()
    {
        // Arrange
        IEnumerable<string> declared = typeof(StorageRuleCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);

        // Act & Assert
        declared.Should().OnlyContain(code => DomainRuleExceptionStrategy.Handles(code));
    }
}
