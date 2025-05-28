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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents a question that retrieves the minimum value from a collection of items.
/// </summary>
/// <typeparam name="T">The type of items in the collection. This type must implement the IComparable interface.</typeparam>
public class MinQuestion<T>(IQuestion<ICollection<T>> listQuestion, IComparer<T>? comparer)
    : IQuestion<T>
    where T : IComparable<T>
{
    public MinQuestion(IQuestion<ICollection<T>> listQuestion)
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    public T AnsweredBy(Actor actor)
    {
        var collection = listQuestion.AnsweredBy(actor);
        return comparer != null
            ? collection.Min(comparer)
            : collection.Min();
    }
}
