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

#region Imports

using System.Collections;
using System.Text;

#endregion

namespace Agenix.Api.Util;

/// <summary>
///     Various utility methods relating to the manipulation of arrays.
/// </summary>
public sealed class ArrayUtils
{
    /// <summary>
    ///     Checks if the given array or collection has elements and none of the elements is null.
    /// </summary>
    /// <param name="collection">the collection to be checked.</param>
    /// <returns>true if the collection has a length and contains only non-null elements.</returns>
    public static bool HasElements(ICollection collection)
    {
        return HasLength(collection) && collection.Cast<object>().All(item => item != null);
    }

    /// <summary>
    ///     Use this sort method instead of <see cref="Array.Sort(System.Array,System.Collections.IComparer)" /> to overcome
    ///     bugs in Mono.
    /// </summary>
    public static void Sort(Array array, IComparer comparer)
    {
        if (SystemUtils.MonoRuntime)
        {
            var list = new ArrayList(array);
            list.Sort(comparer);
            list.ToArray().CopyTo(array, list.Count);
            return;
        }

        Array.Sort(array, comparer);
    }

    /// <summary>
    ///     Checks if the given array or collection is null or has no elements.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static bool HasLength(ICollection collection)
    {
        return collection != null && collection.Count > 0;
    }

    /// <summary>
    ///     Tests equality of two single-dimensional arrays by checking each element
    ///     for equality.
    /// </summary>
    /// <param name="a">The first array to be checked.</param>
    /// <param name="b">The second array to be checked.</param>
    /// <returns>True if arrays are the same, false otherwise.</returns>
    public static bool AreEqual(Array a, Array b)
    {
        if (a == null && b == null)
        {
            return true;
        }

        if (a != null && b != null)
        {
            if (a.Length == b.Length)
            {
                for (var i = 0; i < a.Length; i++)
                {
                    var elemA = a.GetValue(i);
                    var elemB = b.GetValue(i);

                    if (elemA is Array && elemB is Array)
                    {
                        if (!AreEqual(elemA as Array, elemB as Array))
                        {
                            return false;
                        }
                    }
                    else if (!Equals(elemA, elemB))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns hash code for an array that is generated based on the elements.
    /// </summary>
    /// <remarks>
    ///     Hash code returned by this method is guaranteed to be the same for
    ///     arrays with equal elements.
    /// </remarks>
    /// <param name="array">
    ///     Array to calculate hash code for.
    /// </param>
    /// <returns>
    ///     A hash code for the specified array.
    /// </returns>
    public static int GetHashCode(Array array)
    {
        var hashCode = 0;

        if (array != null)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var el = array.GetValue(i);
                if (el != null)
                {
                    if (el is Array)
                    {
                        hashCode += 17 * GetHashCode(el as Array);
                    }
                    else
                    {
                        hashCode += 13 * el.GetHashCode();
                    }
                }
            }
        }

        return hashCode;
    }

    /// <summary>
    ///     Returns string representation of an array.
    /// </summary>
    /// <param name="array">
    ///     Array to return as a string.
    /// </param>
    /// <returns>
    ///     String representation of the specified <paramref name="array" />.
    /// </returns>
    public static string ToString(Array array)
    {
        if (array == null)
        {
            return "null";
        }

        var sb = new StringBuilder();
        sb.Append('{');

        for (var i = 0; i < array.Length; i++)
        {
            var val = array.GetValue(i);
            sb.Append(val == null ? "null" : val.ToString());

            if (i < array.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append('}');

        return sb.ToString();
    }

    /// <summary>
    ///     Concatenates 2 arrays of compatible element types
    /// </summary>
    /// <remarks>
    ///     If either of the arguments is null, the other array is returned as the result.
    ///     The array element types may differ as long as they are assignable. The result array will be of the "smaller"
    ///     element type.
    /// </remarks>
    public static Array Concat(Array first, Array second)
    {
        if (first == null)
        {
            return second;
        }

        if (second == null)
        {
            return first;
        }

        Type resultElementType;
        var firstElementType = first.GetType().GetElementType();
        var secondElementType = second.GetType().GetElementType();
        if (firstElementType.IsAssignableFrom(secondElementType))
        {
            resultElementType = firstElementType;
        }
        else if (secondElementType.IsAssignableFrom(firstElementType))
        {
            resultElementType = secondElementType;
        }
        else
        {
            throw new ArgumentException(
                $"Array element types '{firstElementType}' and '{secondElementType}' are not compatible");
        }

        var result = Array.CreateInstance(resultElementType, first.Length + second.Length);
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
    }
}
