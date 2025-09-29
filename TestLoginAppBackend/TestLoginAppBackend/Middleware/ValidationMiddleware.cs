using FluentValidation;
using System.Text.Json;

namespace TestLoginAppBackend.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (context.Request.ContentLength > 0 && context.Request.ContentType?.Contains("application/json") == true)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<EndpointMetadata>() is { } metadata && metadata.RequestType != null)
            {
                context.Request.EnableBuffering();

                var requestObj = await JsonSerializer.DeserializeAsync(
                    context.Request.Body,
                    metadata.RequestType,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                context.Request.Body.Position = 0;

                if (requestObj != null)
                {
                    var validatorType = typeof(IValidator<>).MakeGenericType(metadata.RequestType);
                    var validator = serviceProvider.GetService(validatorType) as IValidator;

                    if (validator != null)
                    {
                        var validationResult = await validator.ValidateAsync(new ValidationContext<object>(requestObj));
                        if (!validationResult.IsValid)
                        {
                            throw new ValidationException(validationResult.Errors);
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}

public class EndpointMetadata
{
    public Type RequestType { get; set; } = null!;
}

