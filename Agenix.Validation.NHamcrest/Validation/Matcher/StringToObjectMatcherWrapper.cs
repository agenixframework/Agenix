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

using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
///     A wrapper class that adapts an <see cref="IMatcher{}" /> instance
///     to become an <see cref="IMatcher{object}" /> by handling type casting internally.
/// </summary>
/// <remarks>
///     This class is used to bridge the gap between matchers expecting string input
///     and cases where the input type is object. It ensures that the encapsulated
///     string matcher can still be used in broader matching scenarios involving object types.
/// </remarks>
public class StringToObjectMatcherWrapper : IMatcher<object>
{
    private readonly IMatcher<string> _innerMatcher;

    public StringToObjectMatcherWrapper(IMatcher<string> innerMatcher)
    {
        _innerMatcher = innerMatcher;
    }

    public bool Matches(object item)
    {
        // Cast the input to string and pass it to the inner matcher's Matches method.
        return item is string str && _innerMatcher.Matches(str);
    }

    public void DescribeMismatch(object item, IDescription mismatchDescription)
    {
    }

    public void DescribeTo(IDescription description)
    {
        // Delegate to the inner matcher’s DescribeTo.
        _innerMatcher.DescribeTo(description);
    }
}
