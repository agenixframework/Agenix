namespace Agenix.Api.Util;

/// <summary>
///     Tests if a string represents a JSON. An empty string is considered to be a
///     valid JSON.
/// </summary>
public class IsJsonPredicate
{
    private IsJsonPredicate()
    {
        // Singleton
    }

    /// <summary>
    ///     Singleton instance of the IsJsonPredicate class.
    /// </summary>
    public static IsJsonPredicate Instance { get; } = new();

    /// <summary>
    ///     Tests if a string represents a JSON. An empty string is considered to be a
    ///     valid JSON.
    /// </summary>
    /// <param name="toTest">The string to test for JSON format.</param>
    /// <returns>True if the string is a valid JSON or an empty string, otherwise false.</returns>
    public bool Test(string toTest)
    {
        if (toTest != null) toTest = toTest.Trim();

        return toTest != null && (toTest.Length == 0 || toTest.StartsWith('{') || toTest.StartsWith('['));
    }
}