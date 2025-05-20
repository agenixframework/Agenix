namespace Agenix.Api.Util;

/// Singleton class to test if a string represents a XML.
/// An empty string is considered to be a valid XML.
/// /
public class IsXmlPredicate
{
    /// <summary>
    ///     Lazy initialization object for the singleton instance of the <c>IsXmlPredicate</c> class.
    /// </summary>
    private static readonly Lazy<IsXmlPredicate> Lazy = new(() => new IsXmlPredicate());

    private IsXmlPredicate()
    {
        // Singleton
    }

    /// <summary>
    ///     Gets the singleton instance of the <c>IsXmlPredicate</c> class.
    /// </summary>
    public static IsXmlPredicate Instance => Lazy.Value;

    /// <summary>
    ///     Tests if a given string represents an XML. An empty string is considered to be valid XML.
    /// </summary>
    /// <param name="toTest">The string to test for XML validity.</param>
    /// <return>True if the string is valid XML or an empty string; false otherwise.</return>
    public bool Test(string toTest)
    {
        toTest = toTest?.Trim();
        return toTest != null && (toTest.Length == 0 || toTest.StartsWith('<'));
    }
}