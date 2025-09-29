namespace TestLoginAppBackend.Abstractions;

public interface ISoapRequest
{
    string ToSoapXml();
    string SoapAction { get; }
    string ResponseElementName { get; }
}