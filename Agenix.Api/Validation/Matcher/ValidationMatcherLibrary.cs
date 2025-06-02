#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.Api.Exceptions;

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Library holding a set of validation matchers. Each library defines a validation prefix as namespace, so
///     there will be no naming conflicts when using multiple libraries at a time.
/// </summary>
public class ValidationMatcherLibrary
{
    /// <summary>
    ///     Dictionary of validationMatchers in this library
    /// </summary>
    private Dictionary<string, IValidationMatcher> _members = [];

    /// <summary>
    ///     Name of ValidationMatcher library
    /// </summary>
    private string _name = "standard";

    /// <summary>
    ///     ValidationMatcher library prefix
    /// </summary>
    private string _prefix = "";

    /// <summary>
    ///     Dictionary of validationMatchers in this library
    /// </summary>
    public Dictionary<string, IValidationMatcher> Members
    {
        get => _members;
        set => _members = value;
    }

    /// <summary>
    ///     Name of ValidationMatcher library
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     ValidationMatcher library prefix
    /// </summary>
    public string Prefix
    {
        get => _prefix;
        set => _prefix = value;
    }

    /// <summary>
    ///     Try to find validationMatcher in library by name.
    /// </summary>
    /// <param name="validationMatcherName">validationMatcher name.</param>
    /// <returns>the validationMatcher instance.</returns>
    public IValidationMatcher GetValidationMatcher(string validationMatcherName)
    {
        if (!_members.ContainsKey(validationMatcherName))
        {
            throw new NoSuchValidationMatcherException(
                "Can not find validation matcher " + validationMatcherName + " in library " + _name + " (" +
                _prefix + ")");
        }

        return _members[validationMatcherName];
    }

    /// <summary>
    ///     Does this library know a validationMatcher with the given name.
    /// </summary>
    /// <param name="validationMatcherName">name to search for</param>
    /// <returns>boolean flag to mark existence.</returns>
    public bool KnowsValidationMatcher(string validationMatcherName)
    {
        if (validationMatcherName.Contains(":"))
        {
            var validationMatcherPrefix =
                validationMatcherName.Substring(0, validationMatcherName.IndexOf(':') + 1);

            var startIndex = validationMatcherName.IndexOf(':');
            var length = validationMatcherName.IndexOf('(') - startIndex - 1;

            return validationMatcherPrefix.Equals(_prefix) && _members.ContainsKey(
                validationMatcherName.Substring(startIndex + 1, length));
        }

        return _members.ContainsKey(validationMatcherName.Substring(0, validationMatcherName.IndexOf('(')));
    }
}
