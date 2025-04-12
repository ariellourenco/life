using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Life.Api.Extensions;

/// <summary>
/// Extension methods for enabling <see cref="ExceptionHandlerExtensions"/>.
/// </summary>
internal static class ExceptionHandlerExtensions
{

    /// <summary>
    /// Adds a middleware to the pipeline that will catch exceptions, log them, and returns .
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
    /// <returns>The provided <see cref="IApplicationBuilder"/> to chain configuration.</returns>
    internal static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder builder) =>
        builder.UseExceptionHandler(app => app.Run(async context =>
            await HandleExceptionAsync(context)));

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is null)
        {
            return;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var details = new ProblemDetails
        {
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Type = exception.GetType().Name
        };

        details.Extensions["traceId"] = context.TraceIdentifier ?? Activity.Current?.Id;

        await context.Response.WriteAsJsonAsync(details, cancellationToken: context.RequestAborted);
    }
}