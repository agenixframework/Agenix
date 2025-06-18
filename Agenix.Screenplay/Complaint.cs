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

using NUnit.Framework;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a class for generating exceptions of a specific type based on complaint details and actual error
///     information.
/// </summary>
public class Complaint
{
    private const string NoValidConstructor =
        "{0} should have a constructor with the signature MyException(string message) and MyException(string message, Exception cause)";

    /// <summary>
    ///     Creates an exception of the specified type using the provided complaint details and the actual error information.
    /// </summary>
    /// <param name="complaintType">
    ///     The type of the exception to be created. Must have a constructor that accepts a string and
    ///     an exception as parameters.
    /// </param>
    /// <param name="complaintDetails">
    ///     Optional details about the complaint. Can be null if only the actual error message is
    ///     needed.
    /// </param>
    /// <param name="actualError">The original exception that caused the complaint. Must not be null.</param>
    /// <returns>
    ///     Returns an exception of the specified type with the provided details, or an <see cref="AssertionException" />
    ///     if a suitable constructor is not found.
    /// </returns>
    public static Exception From(Type complaintType, string complaintDetails, Exception actualError)
    {
        if (complaintDetails == null && actualError.Message == null)
        {
            return From(complaintType, actualError);
        }

        complaintDetails = ErrorMessageFrom(complaintDetails, actualError);

        try
        {
            var constructor = complaintType.GetConstructor(new[] { typeof(string), typeof(Exception) });
            return (Exception)constructor.Invoke([complaintDetails, actualError]);
        }
        catch (Exception)
        {
            return new AssertionException(string.Format(NoValidConstructor, complaintType.Name));
        }
    }

    /// <summary>
    ///     Constructs an error message by combining the provided complaint details and the message from the actual error.
    /// </summary>
    /// <param name="complaintDetails">The additional details about the complaint. Can be null.</param>
    /// <param name="actualError">The original exception that caused the complaint. Must not be null.</param>
    /// <returns>
    ///     Returns a string that combines the complaint details and the actual error message. If one of them is null, the
    ///     available message is returned.
    /// </returns>
    private static string ErrorMessageFrom(string complaintDetails, Exception actualError)
    {
        if (complaintDetails == null)
        {
            return actualError.Message;
        }

        if (actualError.Message == null)
        {
            return complaintDetails;
        }

        return complaintDetails + " - " + actualError.Message;
    }

    /// <summary>
    ///     Creates an exception of the specified type using the provided complaint details and/or the actual error details.
    /// </summary>
    /// <param name="complaintType">
    ///     The type of the exception to create. It must have a valid constructor to accept the
    ///     provided parameters.
    /// </param>
    /// <param name="complaintDetails">
    ///     The additional details to describe the complaint. Can be null if the actual error
    ///     message is sufficient.
    /// </param>
    /// <param name="actualError">The original exception that caused this complaint.</param>
    /// <returns>
    ///     Returns an exception object of the specified type with the provided details or throws an
    ///     <see cref="AssertionException" /> if a valid constructor is not found.
    /// </returns>
    public static Exception From(Type complaintType, Exception actualError)
    {
        try
        {
            var constructor = complaintType.GetConstructor(new[] { typeof(Exception) });
            return (Exception)constructor.Invoke([actualError]);
        }
        catch (Exception)
        {
            try
            {
                var constructor = complaintType.GetConstructor(new[] { typeof(string), typeof(Exception) });
                return (Exception)constructor.Invoke([actualError.Message, actualError]);
            }
            catch (Exception)
            {
                return new AssertionException(string.Format(NoValidConstructor, complaintType.Name));
            }
        }
    }
}
