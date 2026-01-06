using System.Collections.Concurrent;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Shared.Application.Exceptions.Handlers;

/// <summary>
/// Thread-safe registry that maps exception types to their corresponding handling strategies.
/// Implements the Strategy pattern with runtime type resolution for maximum maintainability.
/// </summary>
/// <remarks>
/// This registry automatically discovers exception strategies and provides O(1) lookup performance.
/// Adding new exception types only requires creating a new strategy class - no modifications needed here.
/// </remarks>
public sealed class ExceptionStrategyRegistry
{
    private readonly IExceptionStrategy _defaultStrategy;
    private readonly ConcurrentDictionary<Type, IExceptionStrategy> _strategies = new();

    /// <summary>
    /// Initializes the registry with all available exception strategies.
    /// </summary>
    /// <param name="strategies">Collection of all registered exception strategies.</param>
    /// <param name="defaultStrategy">Fallback strategy for unregistered exception types.</param>
    public ExceptionStrategyRegistry(
        IEnumerable<IExceptionStrategy> strategies,
        DefaultExceptionHandler defaultStrategy
    )
    {
        _defaultStrategy = defaultStrategy;

        // Register all strategies by their handled exception type
        foreach (IExceptionStrategy strategy in strategies.Where(s => s.GetType() != typeof(DefaultExceptionHandler)))
        {
            _strategies.TryAdd(strategy.ExceptionType, strategy);
        }
    }

    /// <summary>
    /// Gets the appropriate strategy for handling the given exception type.
    /// Uses inheritance hierarchy traversal to find the most specific handler.
    /// </summary>
    /// <param name="exceptionType">The type of exception to handle.</param>
    /// <returns>The strategy that can handle this exception type, or the default strategy.</returns>
    private IExceptionStrategy GetStrategy(Type exceptionType)
    {
        // First, try direct type lookup for O(1) performance
        if (_strategies.TryGetValue(exceptionType, out IExceptionStrategy? strategy))
        {
            return strategy;
        }

        // Walk up the inheritance hierarchy to find a compatible handler
        Type? currentType = exceptionType.BaseType;
        while (currentType != null && currentType != typeof(object))
        {
            if (_strategies.TryGetValue(currentType, out strategy))
            {
                // Cache this mapping for future O(1) lookups
                _strategies.TryAdd(exceptionType, strategy);
                return strategy;
            }
            currentType = currentType.BaseType;
        }

        // No specific handler found, use default strategy
        return _defaultStrategy;
    }

    /// <summary>
    /// Creates a ProblemDetails response for the given exception using the appropriate strategy.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A ProblemDetails response for the exception.</returns>
    public ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        IExceptionStrategy strategy = GetStrategy(exception.GetType());
        return strategy.CreateProblemDetails(exception, context);
    }
}
