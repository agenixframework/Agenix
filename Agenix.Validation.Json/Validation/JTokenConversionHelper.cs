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

using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Provides helper methods for converting JTokens to dynamic objects.
/// </summary>
public static class JTokenConversionHelper
{
    /// <summary>
    ///     Converts a given JToken to a dynamic object.
    /// </summary>
    /// <param name="token">The JToken to be converted.</param>
    /// <returns>A dynamic object that represents the JToken data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the JToken is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the JToken type is unsupported.</exception>
    public static object? ConvertToDynamic(JToken token)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token), "JToken cannot be null.");
        }

        switch (token.Type)
        {
            case JTokenType.Object:
                return token.ToObject<Dictionary<string, object>?>();

            case JTokenType.Array:
                var array = token.ToObject<List<JToken>?>() ?? [];
                List<object> convertedList = [];
                convertedList.AddRange(array.Select(ConvertToDynamic)
                    .Where(item => item != null)
                    .Cast<object>());

                return convertedList;

            case JTokenType.Integer:
                return token.ToObject<int>();

            case JTokenType.Float:
                return token.ToObject<double>();

            case JTokenType.String:
                return token.ToObject<string>();

            case JTokenType.Boolean:
                return token.ToObject<bool>();

            case JTokenType.Null:
                return null;

            case JTokenType.Date:
                return token.ToObject<DateTime>();

            case JTokenType.TimeSpan:
                return token.ToObject<TimeSpan>();

            case JTokenType.Uri:
                return token.ToObject<Uri>();

            case JTokenType.Guid:
                return token.ToObject<Guid>();

            case JTokenType.None:
            case JTokenType.Constructor:
            case JTokenType.Property:
            case JTokenType.Comment:
            case JTokenType.Undefined:
            case JTokenType.Raw:
            case JTokenType.Bytes:
            default:
                throw new InvalidOperationException($"Unsupported JToken type: {token.Type}");
        }
    }
}
