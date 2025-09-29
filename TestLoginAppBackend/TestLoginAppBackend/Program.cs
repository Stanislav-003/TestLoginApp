using FluentValidation;
using TestLoginAppBackend.Endpoints;
using TestLoginAppBackend.Middleware;
using TestLoginAppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<MekashronSoapService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(t => t.FullName?.Replace('+', '.')));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpoints();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ValidationMiddleware>();
app.MapEndpoints();

app.MapGet("/", () => "Backend is running!");
app.Run();