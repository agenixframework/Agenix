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

using System.Text.RegularExpressions;

namespace Agenix.Screenplay.Utils;

public class Acronym : IEquatable<Acronym>
{
    private readonly string _acronymText;
    private readonly int _end;
    private readonly int _start;

    public Acronym(string acronym, int start, int end)
    {
        _acronymText = acronym;
        _start = start;
        _end = end;
    }

    public bool Equals(Acronym? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return _start == other._start &&
               _end == other._end &&
               _acronymText == other._acronymText;
    }

    public static HashSet<Acronym> AcronymsIn(string text)
    {
        var acronyms = new HashSet<Acronym>();
        var words = Regex.Split(text, @"\W").Where(w => !string.IsNullOrEmpty(w));

        foreach (var word in words)
            if (IsAnAcronym(word))
                acronyms.UnionWith(AppearancesOf(word, text));

        return acronyms;
    }

    public string RestoreIn(string text)
    {
        var prefix = _start > 0 ? text.Substring(0, _start) : "";
        var suffix = text.Substring(_end);
        return prefix + _acronymText + suffix;
    }

    private static HashSet<Acronym> AppearancesOf(string word, string text)
    {
        var acronyms = new HashSet<Acronym>();
        var startAt = 0;

        while (startAt < text.Length)
        {
            var wordFoundAt = text.IndexOf(word, startAt, StringComparison.Ordinal);
            if (wordFoundAt == -1) break;

            acronyms.Add(new Acronym(word, wordFoundAt, wordFoundAt + word.Length));
            startAt += word.Length;
        }

        return acronyms;
    }

    public static bool IsAnAcronym(string word)
    {
        return word.Length > 1 &&
               char.IsUpper(FirstLetterIn(word)) &&
               char.IsUpper(LastLetterIn(word));
    }

    private static char FirstLetterIn(string word)
    {
        var wordWithoutDigits = Regex.Replace(word, @"\d", "");
        return wordWithoutDigits.Length == 0 ? word[0] : wordWithoutDigits[0];
    }

    private static char LastLetterIn(string word)
    {
        var wordWithoutDigits = Regex.Replace(word, @"\d", "");
        return wordWithoutDigits.Length == 0
            ? word[word.Length - 1]
            : wordWithoutDigits[wordWithoutDigits.Length - 1];
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Acronym)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (_acronymText?.GetHashCode() ?? 0);
            hash = hash * 31 + _start;
            hash = hash * 31 + _end;
            return hash;
        }
    }

    public override string ToString()
    {
        return $"Acronym{{acronymText='{_acronymText}', start={_start}, end={_end}}}";
    }
}
