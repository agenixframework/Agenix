namespace Agenix.Screenplay.Formatting;

/// <summary>
/// A utility class for removing redundant terms from an expression.
/// </summary>
/// <remarks>
/// This class provides a static method to streamline and clean up expressions
/// by stripping predefined redundant prefixes. It is particularly tailored to
/// identify the common terms that may add unnecessary verbosity to text.
/// </remarks>
public class StripRedundantTerms
{
    private static readonly IReadOnlyList<string> RedundantHamcrestPrefixes =
        ["is ", "be ", "should be"];

    public static string From(string expression)
    {
        return RedundantHamcrestPrefixes.Aggregate(expression, RemovePrefix);
    }

    private static string RemovePrefix(string expression, string prefix)
    {
        return expression.StartsWith(prefix) 
            ? expression.Substring(prefix.Length) 
            : expression;
    }
}
