using FluentValidation;
using System.Net;
using System.Text.Json;

namespace TestLoginAppBackend.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        object response;

        if (exception is ValidationException validationEx)
        {
            response = validationEx.Errors;
        }
        else
        {
            response = new
            {
                error = exception.Message,
                type = exception.GetType().Name
            };
        }

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
