using _116.Shared.Domain.Exceptions;

namespace _116.Content.Domain.Exceptions;

/// <summary>
/// A <see cref="DomainRuleException" /> raised by the Content domain, carrying a
/// <see cref="StateMachines.ContentRuleCodes" /> code. The module's strategy translates it.
/// </summary>
public class ContentRuleException(string code, params string[] args) : DomainRuleException(code, args);
