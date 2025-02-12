using System;
using System.Collections.Generic;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Core.Validation.Matcher.Hamcrest;

/// <summary>
/// Provides utility methods to create and compose matchers for performing assertions and validation logic.
/// This class contains static methods to facilitate common matching scenarios such as equality checks,
/// pattern matching, substring checks, and logical operations on matchers.
/// </summary>
public class Matchers
{
    public static IMatcher<T> DescribedAs<T>(string description, IMatcher<T> matcher, params object[] values)
    {
        return new DescribedAs<T>(description, matcher, values);
    }

    public static IMatcher<T> EqualTo<T>(T value)
    {
        return Is.EqualTo(value);
    }
    
    public static IMatcher<string> EqualToIgnoringCase(string expected)
    {
        return new EqualToIgnoringCaseMatcher(expected);
    }

    public static IMatcher<string> EqualToIgnoringWhiteSpace(string expected)
    {
        return new EqualToIgnoringWhiteSpaceMatcher(expected);
    }

    public static IMatcher<T> Not<T>(IMatcher<T> matcher)
    {
        return new IsNotMatcher<T>(matcher);
    }

    public static IMatcher<string> ContainsString(string substring)
    {
        return Contains.String(substring);
    }

    public static IMatcher<string> ContainsStringIgnoringCase(string substring)
    {
        return Contains.String(substring).CaseInsensitive();
    }

    public static IMatcher<string> StartsWith(string prefix)
    {
        return Starts.With(prefix);
    }

    public static IMatcher<string> StartsWithIgnoringCase(string prefix)
    {
        return Starts.With(prefix).CaseInsensitive();
    }

    public static IMatcher<string> EndsWith(string prefix)
    {
        return Ends.With(prefix);
    }

    public static IMatcher<string> EndsWithIgnoringCase(string prefix)
    {
        return Ends.With(prefix).CaseInsensitive();
    }
    
    public static IMatcher<string> MatchesPattern(string pattern)
    {
        return new MatchesPatternMatcher(pattern);
    }
    
    public static IMatcher<T> AnyOf<T>(IEnumerable<IMatcher<T>> matchers)
    {
        return new AnyOfMatcher<T>(matchers);
    }
    
    /// <summary>
    /// Creates a matcher that matches when the examined object is found within
    /// the specified collection.
    /// For example: <c>Assert.That("foo", IsIn.IsIn(new List<string> { "bar", "foo" }));</c>
    /// </summary>
    /// <remarks>
    /// This method is deprecated. Use <see cref="In{T}(ICollection{T})"/> instead.
    /// </remarks>
    /// <typeparam name="T">The type of the matcher.</typeparam>
    /// <param name="collection">
    /// The collection in which matching items must be found.
    /// </param>
    /// <returns>Returns the matcher.</returns>
    [Obsolete("Use In(ICollection<T>) instead.")]
    public static IMatcher<T> IsIn<T>(ICollection<T> collection)
    {
        return In(collection);
    }
    
    /// <summary>
    /// Creates a matcher that matches when the examined object is found within 
    /// the specified collection.
    /// For example: <c>Assert.That("foo", IsIn.In(new List<string> { "bar", "foo" }));</c>
    /// </summary>
    /// <typeparam name="T">The type of the matcher.</typeparam>
    /// <param name="collection">The collection in which matching items must be found.</param>
    /// <returns>Returns the matcher.</returns>
    public static IMatcher<T> In<T>(ICollection<T> collection)
    {
        return new IsIn<T>(collection);
    }
    
    /// <summary>
    /// Creates a matcher that matches when the examined object is found within the
    /// specified array.
    /// For example:
    /// <c>Assert.That("foo", IsIn.IsIn(new[] { "bar", "foo" }));</c>
    /// </summary>
    /// <remarks>
    /// This method is deprecated. Use <see cref="In{T}(T[])" /> instead.
    /// </remarks>
    /// <typeparam name="T">The type of the matcher.</typeparam>
    /// <param name="elements">The array in which matching items must be found.</param>
    /// <returns>Returns the matcher.</returns>
    [Obsolete("Use In(T[]) instead.")]
    public static IMatcher<T> IsIn<T>(params T[] elements)
    {
        return In(elements);
    }
    
    /// <summary>
    /// Creates a matcher that matches when the examined object is found within 
    /// the specified array.
    /// For example: 
    /// <c>Assert.That("foo", IsIn.In(new[] { "bar", "foo" }));</c>
    /// </summary>
    /// <typeparam name="T">The type of the matcher.</typeparam>
    /// <param name="elements">The array in which matching items must be found.</param>
    /// <returns>Returns the matcher.</returns>
    public static IMatcher<T> In<T>(params T[] elements)
    {
        return new IsIn<T>(elements);
    }
    
    /// <summary>
    /// Creates a matcher that matches when the examined object is equal to one of the
    /// specified elements.
    /// For example:
    /// <c>Assert.That("foo", IsIn.IsOneOf("bar", "foo"));</c>
    /// </summary>
    /// <remarks>
    /// This method is deprecated. Use <see cref="OneOf{T}(T[])"/> instead.
    /// </remarks>
    /// <typeparam name="T">The type of the matcher.</typeparam>
    /// <param name="elements">The elements amongst which matching items will be found.</param>
    /// <returns>Returns the matcher.</returns>
    [Obsolete("Use OneOf(T[]) instead.")]
    public static IMatcher<T> IsOneOf<T>(params T[] elements)
    {
        return OneOf(elements);
    }

    /// <summary>
    /// Creates a matcher that matches when the examined object is equal to one of the
    /// specified elements.
    /// For example:
    /// <c>Assert.That("foo", IsIn.OneOf("bar", "foo"));</c>
    /// </summary>
    /// <typeparam name="T">The type of the matcher.</typeparam>
    /// <param name="elements">The elements amongst which matching items will be found.</param>
    /// <returns>Returns the matcher.</returns>
    public static IMatcher<T> OneOf<T>(params T[] elements)
    {
        return In(elements);
    }

    /// <summary>
    /// Creates a matcher that matches when the examined object is null.
    /// </summary>
    /// <returns>Returns a matcher that matches null values.</returns>
    public static IMatcher<object> NullValue()
    {
        return Is.Null();
    }

    /// <summary>
    /// Creates a matcher that checks if the examined object is not null.
    /// </summary>
    /// <returns>Returns a matcher that evaluates to true if the examined object is not null.</returns>
    public static IMatcher<object> NotNullValue() {
        return Is.NotNull();
    }

    /// <summary>
    ///     Creates a matcher of type <see cref="string" /> that matches when the examined string has zero length.
    /// </summary>
    /// <returns>The matcher instance.</returns>
    public static IMatcher<string> IsEmptyString()
    {
        return Is.EqualTo(string.Empty);
    }
    
    /// <summary>
    ///     Creates a matcher of type <see cref="string" /> that matches when the examined string is null or has zero length.
    /// </summary>
    /// <returns>The matcher instance.</returns>
    public static IMatcher<string> IsEmptyOrNullString()
    {
        return Matches.AnyOf(Is.Null(), IsEmptyString());
    }
    
    /// <summary>
    /// Returns a matcher for a blank string (zero or more whitespace characters).
    /// </summary>
    /// <returns>An instance of <see cref="IsBlankString"/>.</returns>
    public static IsBlankString BlankString()
    {
        return IsBlankString.BlankInstance;
    }

    /// <summary>
    /// Returns a matcher that checks if a string is either null or a blank string.
    /// </summary>
    /// <returns>A matcher to validate strings that are null or consist only of whitespace.</returns>
    public static IMatcher<string> BlankOrNullString()
    {
        return IsBlankString.NullOrBlankInstance;
    }
    
    public static IMatcher<IEnumerable<dynamic>> Empty()
    {
        return new IsEmptyCollection<dynamic>();
    }
    
    public static IMatcher<T> GreaterThan<T>(T value) where T : IComparable<T>
    {
        return (IMatcher<T>) new IsGreaterThan<T>(value);
    }


}