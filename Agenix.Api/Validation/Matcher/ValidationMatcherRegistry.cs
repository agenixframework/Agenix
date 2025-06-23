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
///     ValidationMatcher registry holding all available validation matcher libraries.
/// </summary>
public class ValidationMatcherRegistry
{
    /// <summary>
    ///     List of libraries providing custom validation matchers
    /// </summary>
    private List<ValidationMatcherLibrary> _validationMatcherLibraries = new();

    /// <summary>
    ///     List of libraries providing custom validation matchers
    /// </summary>
    public List<ValidationMatcherLibrary> ValidationMatcherLibraries
    {
        get => _validationMatcherLibraries;
        set => _validationMatcherLibraries = value;
    }

    public ValidationMatcherLibrary GetLibraryForPrefix(string validationMatcherPrefix)
    {
        if (_validationMatcherLibraries == null)
        {
            throw new NoSuchValidationMatcherLibraryException(
                $"Can not find ValidatorMatcher library for prefix '{validationMatcherPrefix}'");
        }

        foreach (var validationMatcherLibrary in _validationMatcherLibraries.Where(validationMatcherLibrary =>
                     validationMatcherLibrary.Prefix.Equals(validationMatcherPrefix)))
        {
            return validationMatcherLibrary;
        }

        throw new NoSuchValidationMatcherLibraryException(
            $"Can not find ValidatorMatcher library for prefix '{validationMatcherPrefix}'");
    }

    /// <summary>
    ///     Adds given validation matcher library to this registry.
    /// </summary>
    /// <param name="validationMatcherLibrary"></param>
    public void AddValidationMatcherLibrary(ValidationMatcherLibrary validationMatcherLibrary)
    {
        var prefixAlreadyUsed =
            _validationMatcherLibraries.Any(lib => lib.Prefix.Equals(validationMatcherLibrary.Prefix));

        if (prefixAlreadyUsed)
        {
            throw new AgenixSystemException(
                $"Validation matcher library prefix '{validationMatcherLibrary.Prefix} is already bound to another instance. " +
                "Please choose another prefix.");
        }

        _validationMatcherLibraries.Add(validationMatcherLibrary);
    }
}
