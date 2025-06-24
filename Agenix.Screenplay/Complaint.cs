#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
