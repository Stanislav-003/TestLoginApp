using FluentValidation;
using System.Text.Json;
using TestLoginAppBackend.Abstractions;
using TestLoginAppBackend.Endpoints;
using TestLoginAppBackend.Middleware;
using TestLoginAppBackend.Services;

namespace TestLoginAppBackend.Features.Users;

public static class GetUser
{
    public record Request(
        int EntityID,
        string Username,
        string Password) : ISoapRequest
    {
        public string ToSoapXml() =>
            $@"<?xml version=""1.0"" encoding=""UTF-8""?>
            <env:Envelope xmlns:env=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns1=""urn:ICUTech.Intf-IICUTech"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:enc=""http://www.w3.org/2003/05/soap-encoding"">
              <env:Body>
                <ns1:GetCustomerInfo env:encodingStyle=""http://www.w3.org/2003/05/soap-encoding"">
                  <EntityID xsi:type=""xsd:int"">{EntityID}</EntityID>
                  <Username xsi:type=""xsd:string"">{Username}</Username>
                  <Password xsi:type=""xsd:string"">{Password}</Password>
                </ns1:GetCustomerInfo>
              </env:Body>
            </env:Envelope>";

        public string SoapAction => "urn:ICUTech.Intf-IICUTech#GetCustomerInfo";
        public string ResponseElementName => "GetCustomerInfoResponse";
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.EntityID).NotEmpty().GreaterThan(0);
            RuleFor(r => r.Username).NotEmpty();
            RuleFor(r => r.Password).NotEmpty();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("user", Handler).WithTags("Users").WithMetadata(new EndpointMetadata { RequestType = typeof(Request) });
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