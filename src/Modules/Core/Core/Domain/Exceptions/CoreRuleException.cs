using _116.Shared.Domain.Exceptions;

namespace _116.Core.Domain.Exceptions;

/// <summary>
/// A <see cref="DomainRuleException" /> raised by the Core domain, carrying a
/// <see cref="StateMachines.CoreRuleCodes" /> code. The module's strategy translates it.
/// </summary>
public class CoreRuleException(string code, params string[] args) : DomainRuleException(code, args);
