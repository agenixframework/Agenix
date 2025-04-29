using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Agenix.Core;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;
using Agenix.Core.Validation.Matcher;
using Agenix.Core.Variable;
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
/// HamcrestValidationMatcher performs validation by leveraging Hamcrest-style matchers
/// to validate values against a specified control expression. It supports dynamic
/// content replacement, extraction of control values, and advanced matching logic
/// for various data types and scenarios.
/// </summary>
public class NHamcrestValidationMatcher : IValidationMatcher, IControlExpressionParser
{
    private readonly List<string> _collectionMatchers =
        ["HasSize", "HasItem", "HasItems", "Contains", "ContainsInAnyOrder"];

    private readonly List<string> _containerMatchers = ["Is", "Not", "EveryItem"];

    private readonly List<string> _iterableMatchers = ["AnyOf", "AllOf"];

    private readonly List<string> _mapMatchers = ["HasEntry", "HasKey", "HasValue"];

    private readonly List<string> _matchers =
    [
        "EqualTo", "EqualToIgnoringCase", "EqualToIgnoringWhiteSpace", "Not", "ContainsString", "ContainsStringIgnoringCase",
        "StartsWith", "StartsWithIgnoringCase", "EndsWith", "EndsWithIgnoringCase", "MatchesPattern"
    ];

    private readonly List<string> _noArgumentCollectionMatchers = ["Empty"];

    private readonly List<string> _noArgumentMatchers =
        ["IsEmptyString", "IsEmptyOrNullString", "NullValue", "NotNullValue", "Anything", "BlankString", "BlankOrNullString"];

    private readonly List<string> _numericMatchers =
        ["GreaterThan", "GreaterThanOrEqualTo", "LessThan", "LessThanOrEqualTo", "CloseTo"];

    private readonly List<string> _optionMatchers = ["IsOneOf", "IsIn"];

    /// <summary>
    /// Extracts individual control values from a given control expression using
    /// a specified delimiter.
    /// </summary>
    /// <param name="controlExpression">The control expression to extract values from.</param>
    /// <param name="delimiter">The character used to separate values in the control expression.</param>
    /// <returns>A list of extracted control values.</returns>
    public List<string> ExtractControlValues(string controlExpression, char delimiter)
    {
        if (controlExpression.StartsWith($"'") && controlExpression.Contains("',"))
            return new DefaultControlExpressionParser().ExtractControlValues(controlExpression, delimiter);

        return [controlExpression];
    }

    /// <summary>
    /// Validates a field's value against specified control parameters and matcher expressions within the given test context.
    /// </summary>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="value">The value of the field to validate.</param>
    /// <param name="controlParameters">A list of control parameters used for validation, including matcher expressions.</param>
    /// <param name="context">The test context in which the validation occurs.</param>
    /// <exception cref="ValidationException">
    /// Thrown when the field value does not satisfy the specified matcher expression.
    /// </exception>
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        string matcherExpression;
        var matcherValue = value;

        if (controlParameters.Count > 1)
        {
            matcherValue = context.ReplaceDynamicContentInString(controlParameters[0]);
            matcherExpression = controlParameters[1];
        }
        else
        {
            matcherExpression = controlParameters[0];
        }

        var matcherName = matcherExpression.Trim()[..matcherExpression.Trim().IndexOf('(')];
        var matcherParameters = DetermineNestedMatcherParameters(
            matcherExpression.Trim()
                .Substring(matcherName.Length + 1, matcherExpression.Trim().Length - matcherName.Length - 2)
        );

        try
        {
            var matcher = GetMatcher(matcherName, matcherParameters, context);
            if (_noArgumentCollectionMatchers.Contains(matcherName) ||
                _collectionMatchers.Contains(matcherName) ||
                matcherName == "EveryItem")
            {
                MatcherAssert.AssertThat(GetCollection(matcherValue), matcher);
            }
            else if (_mapMatchers.Contains(matcherName))
            {
                MatcherAssert.AssertThat(GetDictionary(matcherValue), matcher);
            }
            else if (_numericMatchers.Contains(matcherName))
            {
                if (matcherName == "CloseTo")
                {
                    MatcherAssert.AssertThat(double.Parse(matcherValue), matcher);
                }
                else
                {
                    MatcherAssert.AssertThat(new NumericComparable(matcherValue), matcher);
                }
            }
            else if (_iterableMatchers.Contains(matcherName) && ContainsNumericMatcher(matcherExpression))
            {
                MatcherAssert.AssertThat(new NumericComparable(matcherValue), matcher);
            }
            else
            {
                MatcherAssert.AssertThat(matcherValue, matcher);
            }
        }
        catch (AssertionError e)
        {
            throw new ValidationException($"{GetType().Name} failed for field '{fieldName}'. " +
                                          $"Received value is '{value}' and did not match '{matcherExpression}'.", e);
        }
    }

    /// <summary>
    /// Retrieves a matcher based on the provided matcher name, parameters, and testing context.
    /// </summary>
    /// <param name="matcherName">The name of the matcher to retrieve.</param>
    /// <param name="matcherParameter">An array of parameters required for the matcher.</param>
    /// <param name="context">The testing context used for matcher operations.</param>
    /// <returns>A dynamic matcher object corresponding to the specified matcher name and parameters.</returns>
    private dynamic GetMatcher(string matcherName, string[] matcherParameter, TestContext context)
    {
        try
        {
            // No-argument matchers
            if (_noArgumentMatchers.Contains(matcherName))
            {
                var matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName);

                if (matcherMethod != null) return matcherMethod.Invoke(null, null);
            }

            // No-argument collection matchers
            if (_noArgumentCollectionMatchers.Contains(matcherName))
            {
                var matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName);

                if (matcherMethod != null) return matcherMethod.Invoke(null, null);
            }

            // Check for missing matcher parameter
            if (matcherParameter.Length == 0) throw new CoreSystemException("Missing matcher parameter");

            // Container matchers
            if (_containerMatchers.Contains(matcherName))
            {
                var matcherMethod = ReflectionHelper.FindMethod(typeof(Is), matcherName, typeof(IMatcher<dynamic>));

                if (matcherMethod != null)
                {
                    var matcherExpression = matcherParameter[0];

                    if (matcherExpression.Contains('(') && matcherExpression.Contains(')'))
                    {
                        var nestedMatcherName = matcherExpression[..matcherExpression.IndexOf('(')].Trim();
                        var nestedMatcherParameter = matcherExpression
                            .AsSpan(matcherExpression.IndexOf('(') + 1,
                                matcherExpression.LastIndexOf(')') - matcherExpression.IndexOf('(') - 1)
                            .ToString()
                            .Split(',');
                        
                        // Get the matcher from the provided name and parameters
                        var nestedMatcher = GetMatcher(nestedMatcherName, nestedMatcherParameter, context);
                        
                        return matcherMethod?.Name == nameof(Matchers.Not) ? Matchers.Not(nestedMatcher) :
                            // Handle specific type T
                            matcherMethod.Invoke(null, [nestedMatcher]);
                    }
                }
            }

            // Iterable matchers
            if (_iterableMatchers.Contains(matcherName))
            {
                var matcherMethod =
                    ReflectionHelper.FindMethod(typeof(Matches), matcherName, typeof(IEnumerable<IMatcher<dynamic>>));

                if (matcherMethod != null)
                {
                    var nestedMatchers = new List<IMatcher<object>>();
                    foreach (var matcherExpression in matcherParameter)
                    {
                        var nestedMatcherName = matcherExpression[..matcherExpression.IndexOf('(')].Trim();
                        var nestedMatcherParameters = DetermineNestedMatcherParameters(
                            matcherExpression[(matcherExpression.IndexOf('(') + 1)..matcherExpression.LastIndexOf(')')]
                                .Trim());

                        var matcher = GetMatcher(nestedMatcherName, nestedMatcherParameters, context);

                        switch (matcher)
                        {
                            case IMatcher<object> objectMatcher:
                                nestedMatchers.Add(objectMatcher);
                                break;
                            case IMatcher<string> stringMatcher:
                                nestedMatchers.Add(new StringToObjectMatcherWrapper(stringMatcher));
                                break;
                            default:
                                nestedMatchers.Add(matcher);
                                break;
                        }
                    }

                    return matcherMethod?.Name switch
                    {
                        nameof(Matches.AnyOf) => Matches.AnyOf(nestedMatchers.ToArray()),
                        nameof(Matches.AllOf) => Matches.AllOf(nestedMatchers.ToArray()),
                        _ => matcherMethod.Invoke(null, [nestedMatchers.ToArray()])
                    };
                }
            }

            // Regular matchers
            if (_matchers.Contains(matcherName))
            {
                UnescapeQuotes(matcherParameter);

                var matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(string)) ??
                                    ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(object));

                if (matcherMethod != null)
                    return matcherMethod.Invoke(null, [matcherParameter[0]]);
            }

            // Numeric matchers
            if (_numericMatchers.Contains(matcherName))
            {
                var matcherMethod =
                    ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(double), typeof(double));

                if (matcherMethod != null)
                    return matcherMethod.Invoke(
                        null,
                        [
                            double.Parse(matcherParameter[0]),
                            matcherParameter.Length > 1 ? double.Parse(matcherParameter[1]) : 0.0
                        ]);

                matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(System.IComparable));

                if (matcherMethod != null)
                    return matcherMethod.Invoke(null, [matcherParameter[0]]);
            }

            // Collection matchers
            if (_collectionMatchers.Contains(matcherName))
            {
                UnescapeQuotes(matcherParameter);

                var matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(int)) ??
                                    ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(object)) ??
                                    ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(object[]));

                if (matcherMethod != null)
                    return matcherMethod.Invoke(null,
                        matcherMethod.GetParameters().Length == 1 &&
                        matcherMethod.GetParameters()[0].ParameterType == typeof(object[])
                            ? [matcherParameter]
                            : [matcherParameter[0]]);
            }

            // Map matchers
            if (_mapMatchers.Contains(matcherName))
            {
                UnescapeQuotes(matcherParameter);

                var matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(object)) ??
                                    ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(object),
                                        typeof(object));

                if (matcherMethod != null)
                    return matcherMethod.Invoke(null, matcherMethod.GetParameters().Length == 2
                        ? [matcherParameter[0], matcherParameter[1]]
                        : [matcherParameter[0]]);
            }

            // Option matchers
            if (_optionMatchers.Contains(matcherName))
            {
                UnescapeQuotes(matcherParameter);

                var matcherMethod = ReflectionHelper.FindMethod(typeof(Matchers), matcherName, typeof(object[])) ??
                                    ReflectionHelper.FindMethod(typeof(Matchers), matcherName,
                                        typeof(IEnumerable<object>));

                if (matcherMethod != null)
                {

                    var isObjectArray = matcherMethod.GetParameters()[0].ParameterType == typeof(object[]);
                    var matcherParameterArray = isObjectArray
                        ? matcherParameter
                        : GetCollection(string.Join(",", matcherParameter)).ToArray();


                    return matcherMethod?.Name switch
                    {
                        nameof(Matchers.IsOneOf) => Matchers.IsOneOf(matcherParameterArray),
                        nameof(Matchers.OneOf) => Matchers.OneOf(matcherParameterArray),
                        nameof(Matchers.IsIn) => Matchers.IsIn(matcherParameterArray),
                        nameof(Matchers.In) => Matchers.In(matcherParameterArray),
                        _ => matcherMethod.Invoke(null, [matcherParameterArray])
                    };
                }
            }
        }
        catch (TargetInvocationException e)
        {
            throw new CoreSystemException("Failed to invoke matcher", e);
        }

        throw new CoreSystemException($"Unsupported matcher: {matcherName}");
    }

    /// <summary>
    ///     Unescape the quotes in search expressions (\\' -> ').
    /// </summary>
    /// <param name="matcherParameters">Array of strings to unescape.</param>
    private static void UnescapeQuotes(string[] matcherParameters)
    {
        if (matcherParameters != null)
            for (var i = 0; i < matcherParameters.Length; i++)
                matcherParameters[i] = matcherParameters[i].Replace("\\'", "'");
    }

    /// <summary>
    ///     Construct collection from delimited string expression.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private List<string> GetCollection(string value)
    {
        if (value == "[]") return [];

        var arrayString = value;
        if (arrayString.StartsWith($"[") && arrayString.EndsWith($"]")) arrayString = arrayString[1..^1];

        return arrayString.Split(',')
            .Select(element => element.Trim())
            .Select(VariableUtils.CutOffDoubleQuotes)
            .Where(StringUtils.HasText)
            .ToList();
    }

    /// <summary>
    ///     Construct collection from delimited string expression.
    /// </summary>
    /// <param name="dictionaryString"></param>
    /// <returns></returns>
    /// <exception cref="CoreSystemException"></exception>
    private Dictionary<string, object> GetDictionary(string dictionaryString)
    {
        var properties = new Dictionary<string, object>();

        try
        {
            // Remove curly braces and replace ',' with newlines
            var formattedMapString = dictionaryString[1..^1].Replace(", ", "\n");

            // Load the string into a dictionary (similar to Java Properties behavior)
            foreach (var line in formattedMapString.Split('\n'))
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var keyValue = line.Split('=', 2); // Split into key-value pair
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();

                        // Add to dictionary, with double quote removal if necessary
                        properties[VariableUtils.CutOffDoubleQuotes(key)] =
                            VariableUtils.CutOffDoubleQuotes(value).Trim();
                    }
                }
        }
        catch (Exception ex)
        {
            throw new CoreSystemException("Failed to reconstruct object of type map", ex);
        }

        return properties;
    }

    private bool ContainsNumericMatcher(string matcherExpression)
    {
        return _numericMatchers.Any(matcherExpression.Contains);
    }

    /**
     * Extracts parameters for a matcher from the raw parameter expression.
     * Parameters refer to the contained parameters and matchers (first level),
     * excluding nested ones.
     * <p />
     * For example, given the expression:
     * <br />
     * {@code "oneOf(greaterThan(5.0), allOf(lessThan(-1.0), greaterThan(-2.0)))"}
     * <p />
     * The extracted parameters are:
     * <br />
     * {@code "greaterThan(5.0)", "allOf(lessThan(-1.0), greaterThan(-2.0))"}.
     * <p />
     * Note that nested container expressions "allOf(lessThan(-1.0), greaterThan(-2.0))" in
     * the second parameter are treated as a single expression. They need to be treated
     * separately in a recursive call to this method, when the parameters for the
     * respective allOf() expression are extracted.
     * 
     * @param rawExpression the full parameter expression of a container matcher
     */
    private string[] DetermineNestedMatcherParameters(string rawExpression)
    {
        if (string.IsNullOrWhiteSpace(rawExpression)) return [];

        var tokenizer = new Tokenizer();
        var tokenizedExpression = tokenizer.Tokenize(rawExpression);
        return tokenizer.RestoreInto(tokenizedExpression.Split(','));
    }

    /// <summary>
    ///     Numeric value comparable automatically converts types to numeric values for comparison.
    /// </summary>
    private class NumericComparable : IComparable<object>
    {
        private readonly double? _decimalValue;
        private readonly long? _number;

        /// <summary>
        ///     Constructor that initializes a numeric value from a string.
        /// </summary>
        /// <param name="value">The input value as a string.</param>
        public NumericComparable(string value)
        {
            if (value.Contains('.'))
                _decimalValue = double.Parse(value);
            else
                try
                {
                    _number = long.Parse(value);
                }
                catch (FormatException e)
                {
                    throw new InvalidOperationException("Invalid numeric string format.", e);
                }
        }

        /// <summary>
        ///     Compare this instance to another object.
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>Negative if less, positive if greater, or zero if equal.</returns>
        public int CompareTo(object obj)
        {
            if (_number != null)
                switch (obj)
                {
                    case string or NumericComparable:
                        return _number.Value.CompareTo(long.Parse(obj.ToString() ?? string.Empty));
                    case long l:
                        return _number.Value.CompareTo(l);
                }

            if (_decimalValue != null)
                switch (obj)
                {
                    case string or NumericComparable:
                        return _decimalValue.Value.CompareTo(double.Parse(obj.ToString() ?? string.Empty));
                    case double d:
                        return _decimalValue.Value.CompareTo(d);
                }

            return 0;
        }

        /// <summary>
        ///     Convert this numeric value to a string.
        /// </summary>
        /// <returns>The numeric value as a string.</returns>
        public override string ToString()
        {
            return _number != null
                ? _number.Value.ToString()
                : _decimalValue?.ToString(CultureInfo.CurrentCulture);
        }
    }

    /// <summary>
    ///     Class that provides functionality to replace expressions that match TEXT_PARAMETER_PATTERN with simple tokens of
    ///     the form $$n$$. The reason for this is, that complex nested expressions may contain characters that interfere with
    ///     further processing - e.g. ''', '(' and ')'
    /// </summary>
    private class Tokenizer
    {
        private const string StartToken = "_TOKEN-";
        private const string EndToken = "-TOKEN_";

        /// <summary>
        ///     Regular expression with three alternative parts (ORed) to match:
        ///     1. ('sometext') - Quoted parameter block of a matcher.
        ///     2. 'sometext' - Quoted text used as a parameter to a string matcher.
        ///     3. (unquotedtext) - Unquoted text used as a parameter to a string matcher.
        /// </summary>
        private static readonly Regex TextParameterPattern = new(
            @"(?<quoted1>\('(?:[^']|\\')*[^\\]'\))" +
            @"|(?<quoted2>('(?:[^']|\\')*[^\\]'))" +
            @"|(?<unquoted>\(((?:[^']|\\')*?)[^\\]?\))",
            RegexOptions.Compiled
        );

        private readonly List<string> _originalTokenValues = [];

        /// <summary>
        ///     Tokenize the given raw expression.
        /// </summary>
        /// <param name="rawExpression">The raw input string.</param>
        /// <returns>The string with all relevant subexpressions replaced by tokens.</returns>
        public string Tokenize(string rawExpression)
        {
            var matcher = TextParameterPattern.Matches(rawExpression);
            var builder = new StringBuilder();
            var lastIndex = 0;

            foreach (Match match in matcher)
            {
                var matchedValue = FindMatchedValue(match);
                _originalTokenValues.Add(matchedValue);

                // Append the part before the current match and the token
                builder.Append(rawExpression, lastIndex, match.Index - lastIndex);
                builder.Append(StartToken).Append(_originalTokenValues.Count).Append(EndToken);

                lastIndex = match.Index + match.Length;
            }

            // Append the part of the string after the last match
            builder.Append(rawExpression, lastIndex, rawExpression.Length - lastIndex);

            return builder.ToString();
        }

        /// <summary>
        ///     Restore the tokens back into the given expressions.
        /// </summary>
        /// <param name="expressions">Array of expressions generated by this tokenizer.</param>
        /// <returns>Expressions with tokens restored to their original values.</returns>
        public string[] RestoreInto(string[] expressions)
        {
            for (var i = 0; i < expressions.Length; i++)
                expressions[i] = VariableUtils.CutOffSingleQuotes(
                    ReplaceTokens(expressions[i], _originalTokenValues).Trim()
                );

            return expressions;
        }

        private string ReplaceTokens(string expression, List<string> paramsList)
        {
            for (var i = 0; i < paramsList.Count; i++)
                expression = expression.Replace(StartToken + (i + 1) + EndToken, paramsList[i]);
            return expression;
        }

        /// <summary>
        ///     Finds the value of the group that was actually matched.
        /// </summary>
        /// <param name="match">The match object.</param>
        /// <returns>The value of the matched group.</returns>
        private string FindMatchedValue(Match match)
        {
            var matchedValue = match.Groups["quoted1"].Value;
            if (string.IsNullOrEmpty(matchedValue)) matchedValue = match.Groups["quoted2"].Value;
            if (string.IsNullOrEmpty(matchedValue)) matchedValue = match.Groups["unquoted"].Value;
            return matchedValue;
        }
    }
}