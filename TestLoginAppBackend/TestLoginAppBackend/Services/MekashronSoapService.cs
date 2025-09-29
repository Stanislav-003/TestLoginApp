using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using TestLoginAppBackend.Abstractions;

namespace TestLoginAppBackend.Services;

public class MekashronSoapService
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly string _namespace;

    public MekashronSoapService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _url = config["MekashronSoapSettings:Url"] ?? throw new ArgumentNullException("MekashronSoapSettings:Url not configured");
        _namespace = config["MekashronSoapSettings:Namespace"] ?? throw new ArgumentNullException("MekashronSoapSettings:Namespace not configured");
    }

    public async Task<JsonElement> SendAsync<TRequest>(TRequest request)
        where TRequest : ISoapRequest
    {
        var soapXml = request.ToSoapXml();
        var content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", request.SoapAction);

        var response = await _httpClient.PostAsync(_url, content);
        var responseXml = await response.Content.ReadAsStringAsync();

        var xdoc = XDocument.Parse(responseXml);
        XNamespace ns = _namespace;
        var returnElement = xdoc.Descendants(ns + request.ResponseElementName).FirstOrDefault()?.Element("return");

        if (returnElement == null)
        {
            throw new InvalidOperationException("Failed to get result from SOAP response");
        }

        var rawJson = returnElement.Value.Trim();
        return JsonSerializer.Deserialize<JsonElement>(rawJson);
    }
}
