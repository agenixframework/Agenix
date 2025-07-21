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

using Agenix.Api.Functions;
using Agenix.Api.Log;
using Agenix.Core.Functions.Core;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Functions;

/// <summary>
///     Provides a default library of predefined functions for common operations.
/// </summary>
/// <remarks>
///     This class serves as a collection of standard function implementations that
///     can be used in systems requiring predefined functionality.
///     It includes functions for operations such as string manipulation,
///     random value generation, date retrieval, and encoding/decoding.
/// </remarks>
public class DefaultFunctionLibrary : FunctionLibrary
{
    /// Represents a logger used for logging messages and events within the DefaultFunctionLibrary.
    /// This logger is initialized with the type of DefaultFunctionLibrary and provides logging functionality
    /// for debugging, informational messages, warnings, and errors.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultFunctionLibrary));

    /// A default library for predefined functions.
    /// Provides a collection of standard function implementations such as string manipulation,
    /// random value generation, date retrieval, and encoding/decoding operations.
    /// /
    public DefaultFunctionLibrary()
    {
        Name = "agenixFunctionLibrary";

        Members.Add("RandomUUID", new RandomUuidFunction());
        Members.Add("Concat", new ConcatFunction());
        Members.Add("UpperCase", new UpperCaseFunction());
        Members.Add("CurrentDate", new CurrentDateFunction());
        Members.Add("ChangeDate", new ChangeDateFunction());
        Members.Add("LowerCase", new LowerCaseFunction());
        Members.Add("RandomString", new RandomStringFunction());
        Members.Add("RandomNumber", new RandomNumberFunction());
        Members.Add("RandomNumberGenerator", new AdvancedRandomNumberFunction());
        Members.Add("EncodeBase64", new EncodeBase64Function());
        Members.Add("DecodeBase64", new DecodeBase64Function());
        Members.Add("Translate", new TranslateFunction());
        Members.Add("Absolute", new AbsoluteFunction());
        Members.Add("Average", new AvgFunction());
        Members.Add("Ceiling", new CeilingFunction());
        Members.Add("DigestAuthHeader", new DigestAuthHeaderFunction());
        Members.Add("EscapeJson", new EscapeJsonFunction());
        Members.Add("Floor", new FloorFunction());
        Members.Add("Message", new LoadMessageFunction());
        Members.Add("LocalHostAddress", new LocalHostAddressFunction());
        Members.Add("Maximum", new MaxFunction());
        Members.Add("Minimum", new MinFunction());
        Members.Add("RandomEnumValue", new RandomEnumValueFunction());
        Members.Add("ReadFile", new ReadFileResourceFunction());
        Members.Add("StringLength", new StringLengthFunction());
        Members.Add("SubstringAfter", new SubstringAfterFunction());
        Members.Add("SubstringBefore", new SubstringBeforeFunction());
        Members.Add("Substring", new SubstringFunction());
        Members.Add("Sum", new SumFunction());
        Members.Add("UnixTimestamp", new UnixTimestampFunction());
        Members.Add("UrlEncode", new UrlEncodeFunction());
        Members.Add("UrlDecode", new UrlDecodeFunction());

        LookupFunctions();
    }

    /// <summary>
    ///     Registers custom function implementations by iterating over the available functions
    ///     retrieved through the IFunction lookup mechanism and adding them to the function library.
    /// </summary>
    private void LookupFunctions()
    {
        foreach (var pair in IFunction.Lookup())
        {
            Members.Add(pair.Key, pair.Value);
            Log.LogDebug("Register function '{PairKey}' as {Type}", pair.Key, pair.Value.GetType());
        }
    }
}
