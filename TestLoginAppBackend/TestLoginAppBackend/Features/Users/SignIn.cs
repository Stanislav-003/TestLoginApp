using FluentValidation;
using System.Text.Json;
using TestLoginAppBackend.Abstractions;
using TestLoginAppBackend.Endpoints;
using TestLoginAppBackend.Middleware;
using TestLoginAppBackend.Services;

namespace TestLoginAppBackend.Features.Users;

public static class SignIn
{
    public record Request(
        string Username,
        string Password,
        string IPs) : ISoapRequest
    {
        public string ToSoapXml() =>
            $@"<?xml version=""1.0"" encoding=""UTF-8""?>
            <env:Envelope xmlns:env=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns1=""urn:ICUTech.Intf-IICUTech"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:enc=""http://www.w3.org/2003/05/soap-encoding"">
              <env:Body>
                <ns1:Login env:encodingStyle=""http://www.w3.org/2003/05/soap-encoding"">
                  <UserName xsi:type=""xsd:string"">{Username}</UserName>
                  <Password xsi:type=""xsd:string"">{Password}</Password>
                  <IPs xsi:type=""xsd:string"">{IPs}</IPs>
                </ns1:Login>
              </env:Body>
            </env:Envelope>";

        public string SoapAction => "urn:ICUTech.Intf-IICUTech#Login";
        public string ResponseElementName => "LoginResponse";
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Username).NotEmpty();
            RuleFor(r => r.Password).NotEmpty();
            RuleFor(r => r.IPs).NotEmpty();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("users/sign-in", Handler).WithTags("Users").WithMetadata(new EndpointMetadata { RequestType = typeof(Request) });
        }
    }

    public static async Task<IResult> Handler(Request request, MekashronSoapService soapClient)
    {
        var result = await soapClient.SendAsync(request);

        if (result.TryGetProperty("ResultCode", out var resultCodeProp) &&
            resultCodeProp.ValueKind == JsonValueKind.Number &&
            resultCodeProp.GetInt32() == -1)
        {
            string? message =
                result.TryGetProperty("ResultMessage", out var msgProp) &&
                msgProp.ValueKind == JsonValueKind.String ? msgProp.GetString() : "Unknown error";

            throw new Exception(message);
        }

        return Results.Ok(result);
    }
}