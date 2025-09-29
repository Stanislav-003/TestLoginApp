using FluentValidation;
using System.Text.Json;
using TestLoginAppBackend.Abstractions;
using TestLoginAppBackend.Endpoints;
using TestLoginAppBackend.Middleware;
using TestLoginAppBackend.Services;

namespace TestLoginAppBackend.Features.Users;

public static class CreateUser
{
    public record Request(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Mobile,
        int CountryID,
        int aID,
        string SignupIP) : ISoapRequest
    {
        public string ToSoapXml() =>
            $@"<?xml version=""1.0"" encoding=""UTF-8""?>
            <env:Envelope xmlns:env=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns1=""urn:ICUTech.Intf-IICUTech"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:enc=""http://www.w3.org/2003/05/soap-encoding"">
              <env:Body>
                <ns1:RegisterNewCustomer env:encodingStyle=""http://www.w3.org/2003/05/soap-encoding"">
                  <Email xsi:type=""xsd:string"">{Email}</Email>
                  <Password xsi:type=""xsd:string"">{Password}</Password>
                  <FirstName xsi:type=""xsd:string"">{FirstName}</FirstName>
                  <LastName xsi:type=""xsd:string"">{LastName}</LastName>
                  <Mobile xsi:type=""xsd:string"">{Mobile}</Mobile>
                  <CountryID xsi:type=""xsd:int"">{CountryID}</CountryID>
                  <aID xsi:type=""xsd:int"">{aID}</aID>
                  <SignupIP xsi:type=""xsd:string"">{SignupIP}</SignupIP>
                </ns1:RegisterNewCustomer>
              </env:Body>
            </env:Envelope>";

        public string SoapAction => "urn:ICUTech.Intf-IICUTech#RegisterNewCustomer";
        public string ResponseElementName => "RegisterNewCustomerResponse";
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Email).NotEmpty();
            RuleFor(r => r.Password).NotEmpty();
            RuleFor(r => r.FirstName).NotEmpty();
            RuleFor(r => r.LastName).NotEmpty();
            RuleFor(r => r.Mobile).NotEmpty();
            RuleFor(r => r.CountryID).NotEmpty().GreaterThan(0);
            RuleFor(r => r.aID).NotEmpty().GreaterThan(0);
            RuleFor(r => r.SignupIP).NotEmpty();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("users", Handler).WithTags("Users").WithMetadata(new EndpointMetadata { RequestType = typeof(Request) });
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
