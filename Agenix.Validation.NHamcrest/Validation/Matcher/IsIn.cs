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
///     A matcher that checks if a given item is present in a collection of elements.
/// </summary>
/// <typeparam name="T">The type of items that this matcher handles.</typeparam>
public class IsIn<T> : IMatcher<T>
{
    private readonly ICollection<T> _collection;

    // Constructor that accepts a collection
    public IsIn(ICollection<T> collection)
    {
        // Use the original collection if no nested collection structure is detected
        _collection = collection;
    }

    // Constructor that accepts an array and converts it into a collection
    public IsIn(params T[] elements)
    {
        // Flatten nested collections into a single list
        _collection = elements.ToList();
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("one of ");
        description.AppendValueList("{", ", ", "}", _collection);
    }

    public bool Matches(T actual)
    {
        return _collection.Contains(actual);
    }

    public void DescribeMismatch(T item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ").AppendValue(item);
    }
}
