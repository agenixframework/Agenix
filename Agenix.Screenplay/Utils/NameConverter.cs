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

using System.Globalization;
using System.Text;

namespace Agenix.Screenplay.Utils;

/// <summary>
///     A utility class that provides methods for converting names into more human-readable formats.
///     Handles various naming conventions such as camel case, underscores, and parameterized names.
/// </summary>
public class NameConverter
{
    private static readonly string[] Abbreviations = { "CSV", "XML", "JSON" };

    private NameConverter()
    {
    }

    /// <summary>
    ///     Restores specific abbreviations in the given sentence by replacing them
    ///     with their uppercase form, if they appear in a title-case format.
    /// </summary>
    /// <param name="sentence">The input sentence to process and restore abbreviations in.</param>
    /// <returns>A string with specific abbreviations restored to their uppercase form.</returns>
    private static string RestoreAbbreviations(string sentence)
    {
        var processing = sentence;
        foreach (var abbreviation in Abbreviations)
        {
            processing = processing.Replace(
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(abbreviation.ToLower()),
                abbreviation);
        }

        return processing;
    }

    /// <summary>
    ///     Converts a given name into a more readable format by handling various formatting styles such as camel case,
    ///     underscores, and parameters.
    /// </summary>
    /// <param name="name">The input string to be humanized into a more readable format.</param>
    /// <returns>A formatted and human-readable string representation of the input name.</returns>
    public static string Humanize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        if (name.Contains(' ', StringComparison.Ordinal) && !ContainsParameters(name))
        {
            return name;
        }

        if (ContainsParameters(name))
        {
            return HumanizeNameWithParameters(name);
        }

        var noUnderscores = name.Replace("_", " ", StringComparison.Ordinal);
        var splitCamelCase = SplitCamelCase(noUnderscores);

        var acronyms = Acronym.AcronymsIn(splitCamelCase);
        var capitalized = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(splitCamelCase.ToLower());

        capitalized = acronyms.Aggregate(capitalized, (current, acronym) => acronym.RestoreIn(current));

        return RestoreAbbreviations(capitalized);
    }


    /// <summary>
    ///     Processes a string representation of a name that may include parameters,
    ///     adjusts the main name part to a human-readable format while preserving the original parameters.
    /// </summary>
    /// <param name="name">The input string containing a name and its parameters, separated by ": ".</param>
    /// <returns>A string with the human-readable version of the name, followed by its original parameters.</returns>
    private static string HumanizeNameWithParameters(string name)
    {
        const string parameterSeparator = ": ";
        var parametersStartAt = name.IndexOf(parameterSeparator, StringComparison.Ordinal);
        var bareName = name[..parametersStartAt];
        var humanizedBareName = Humanize(bareName);
        var parameters = name[parametersStartAt..];
        return humanizedBareName + parameters;
    }

    /// <summary>
    ///     Determines whether the specified string contains parameters in the form of ": ".
    /// </summary>
    /// <param name="name">The string to check for parameters.</param>
    /// <returns>True if the string contains parameters; otherwise, false.</returns>
    private static bool ContainsParameters(string name)
    {
        return name.Contains(": ", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Splits a given string into separate words based on camel casing and removes any extra spaces.
    /// </summary>
    /// <param name="name">The camel case string to split into individual words.</param>
    /// <returns>A single string with words separated by spaces, derived from the given camel case string.</returns>
    public static string SplitCamelCase(string name)
    {
        var splitWords = new List<string>();
        var phrases = name.Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p));

        foreach (var phrase in phrases)
        {
            splitWords.AddRange(SplitWordsIn(phrase));
        }

        return string.Join(" ", splitWords).Trim();
    }

    /// <summary>
    ///     Splits a given phrase into its constituent words based on camel casing or other word boundaries.
    /// </summary>
    /// <param name="phrase">The string to split into individual words.</param>
    /// <returns>A list of strings representing the individual words extracted from the input phrase.</returns>
    private static List<string> SplitWordsIn(string phrase)
    {
        var splitWords = new List<string>();
        var currentWord = new StringBuilder();

        for (var index = 0; index < phrase.Length; index++)
        {
            if (OnWordBoundary(phrase, index))
            {
                splitWords.Add(LowercaseOrAcronym(currentWord.ToString()));
                currentWord.Clear();
                currentWord.Append(phrase[index]);
            }
            else
            {
                currentWord.Append(phrase[index]);
            }
        }

        splitWords.Add(LowercaseOrAcronym(currentWord.ToString()));

        return splitWords.Where(word => !string.IsNullOrWhiteSpace(word)).ToList();
    }

    /// <summary>
    ///     Converts the provided word to lowercase if it is not an acronym.
    /// </summary>
    /// <param name="word">The word to evaluate and transform.</param>
    /// <returns>The original word if it is an acronym; otherwise, the word converted to lowercase.</returns>
    private static string LowercaseOrAcronym(string word)
    {
        return Acronym.IsAnAcronym(word)
            ? word
            : word.ToLower(CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///     Determines whether the specified index in a given string marks a word boundary.
    /// </summary>
    /// <param name="name">The string to evaluate for word boundaries.</param>
    /// <param name="index">The index within the string to check for a word boundary.</param>
    /// <returns>True if the specified index represents a word boundary; otherwise, false.</returns>
    private static bool OnWordBoundary(string name, int index)
    {
        return UppercaseLetterAt(name, index)
               && (LowercaseLetterAt(name, index - 1) || LowercaseLetterAt(name, index + 1));
    }

    /// <summary>
    ///     Determines whether the character at the specified index in the given string is an uppercase letter.
    /// </summary>
    /// <param name="name">The string to check.</param>
    /// <param name="index">The index of the character to evaluate.</param>
    /// <returns>True if the character at the specified index is an uppercase letter; otherwise, false.</returns>
    private static bool UppercaseLetterAt(string name, int index)
    {
        return char.IsAscii(name[index]) && char.IsUpper(name[index]);
    }

    /// <summary>
    ///     Determines whether the character at the specified index in the given string is a lowercase letter.
    /// </summary>
    /// <param name="name">The string to check.</param>
    /// <param name="index">The index of the character to evaluate.</param>
    /// <returns>True if the character at the specified index is a lowercase letter; otherwise, false.</returns>
    private static bool LowercaseLetterAt(string name, int index)
    {
        return index >= 0
               && index < name.Length
               && char.IsAscii(name[index])
               && char.IsLower(name[index]);
    }
}
