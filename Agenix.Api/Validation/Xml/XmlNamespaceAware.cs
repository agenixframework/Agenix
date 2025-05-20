namespace Agenix.Api.Validation.Xml;

/// <summary>
///     Provides functionality to manage and configure XML namespaces within an implementation.
/// </summary>
public interface IXmlNamespaceAware
{
    /// Configures the XML namespaces using a dictionary where the key represents the namespace prefix and the value represents the namespace URI.
    /// @param namespaces A dictionary containing namespace prefixes and their corresponding URIs.
    /// /
    void SetNamespaces(IDictionary<string, string> namespaces);
}