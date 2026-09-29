using _116.Shared.Domain.Exceptions;

namespace _116.Identity.Domain.Exceptions;

/// <summary>
/// A <see cref="DomainRuleException" /> raised by the Identity domain, carrying a
/// <see cref="StateMachines.IdentityRuleCodes" /> code. The module's strategy translates it.
/// </summary>
public class IdentityRuleException(string code, params string[] args) : DomainRuleException(code, args);
