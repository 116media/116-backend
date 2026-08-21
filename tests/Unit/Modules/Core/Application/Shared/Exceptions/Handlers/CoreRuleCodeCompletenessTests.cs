using System.Reflection;
using _116.Core.Application.Shared.Exceptions.Handlers;
using _116.Core.Domain.StateMachines;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Guards that every rule code declared on <see cref="CoreRuleCodes"/> has a response in the
/// strategy's table, so a new rule cannot silently fall to the 400 fallback with a wrong status.
/// </summary>
public class CoreRuleCodeCompletenessTests
{
    [Fact]
    public void EveryDeclaredRuleCode_ShouldHaveAStrategyResponse()
    {
        // Arrange
        IEnumerable<string> declared = typeof(CoreRuleCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);

        // Act & Assert
        declared.Should().OnlyContain(code => DomainRuleExceptionStrategy.Handles(code));
    }
}
