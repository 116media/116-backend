using _116.Shared.Application.Exceptions;

using Microsoft.AspNetCore.Http;

namespace _116.Shared.Application.Middleware;

/// <summary>
/// Middleware that intercepts 404 and 405 responses and throws appropriate exceptions to ensure
/// consistent error handling for non-existing endpoints and wrong HTTP methods.
/// </summary>
public class ResourceNotFoundMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ResourceNotFoundMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to check for 404 and 405 responses and converts them to exceptions.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // If no response has been written yet, check for error status codes
        if (!context.Response.HasStarted)
        {
            switch (context.Response.StatusCode)
            {
                // If the response is 404, throw ResourceNotFoundException
                case StatusCodes.Status404NotFound:
                    throw new ResourceNotFoundException(context.Request.Path, context.Request.Method);
                // If the response is 405, throw MethodNotAllowedException
                case StatusCodes.Status405MethodNotAllowed:
                    throw new MethodNotAllowedException(context.Request.Path, context.Request.Method);
            }
        }
    }
}
