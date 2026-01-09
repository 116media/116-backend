using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace _116.Shared.Application.Middleware;

/// <summary>
/// Middleware that converts \n to newlines in Swagger JSON description fields for UI rendering.
/// Does not modify the actual JSON files.
/// </summary>
public partial class SwaggerDescriptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Only process Swagger JSON endpoint requests
        if (
            !context.Request.Path.StartsWithSegments("/swagger")
            || !context.Request.Path.Value!.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
        )
        {
            await next(context);
            return;
        }

        // Capture response in memory stream to process before sending to the client
        Stream originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        // Read the generated Swagger JSON
        memoryStream.Seek(0, SeekOrigin.Begin);
        string json = await new StreamReader(memoryStream).ReadToEndAsync();

        // Replace \\n (4 chars: backslash-backslash-n) with \n (2 chars: backslash-n)
        // This converts escaped newlines in C# strings to proper JSON newline escapes
        string processed = DescriptionRegex()
            .Replace(
                json,
                match =>
                {
                    string fullMatch = match.Value;
                    return fullMatch.Replace(@"\\n", "\\n");
                }
            );

        context.Response.Body = originalBody;

        byte[] bytes = Encoding.UTF8.GetBytes(processed);
        context.Response.ContentLength = bytes.Length;
        await context.Response.Body.WriteAsync(bytes);
    }

    [GeneratedRegex(@"""description""\s*:\s*""[^""]*(?:\\.[^""]*)*""", RegexOptions.Compiled)]
    private static partial Regex DescriptionRegex();
}
