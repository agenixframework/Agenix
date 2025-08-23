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

using System.Collections.Generic;
using System.Text;
using Agenix.Api.Context;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Utility class providing static helper methods for calling various functions.
/// </summary>
public static class Functions
{
    /// <summary>
    ///     Runs the current date function with no arguments.
    /// </summary>
    /// <param name="context">Test context</param>
    /// <returns>Current date with default format</returns>
    public static string CurrentDate(TestContext context)
    {
        return new CurrentDateFunction().Execute([], context);
    }

    /// <summary>
    ///     Runs current date function with date format argument.
    /// </summary>
    /// <param name="dateFormat">Date format string</param>
    /// <param name="context">Test context</param>
    /// <returns>Current date with specified format</returns>
    public static string CurrentDate(string dateFormat, TestContext context)
    {
        return new CurrentDateFunction().Execute([dateFormat], context);
    }

    /// <summary>
    ///     Runs change date function with date, offset, and format arguments.
    /// </summary>
    /// <param name="date">Date string to modify</param>
    /// <param name="dateOffset">Date offset string</param>
    /// <param name="dateFormat">Date format string</param>
    /// <param name="context">Test context</param>
    /// <returns>Modified date with a specified format</returns>
    public static string ChangeDate(string date, string dateOffset, string dateFormat, TestContext context)
    {
        return new ChangeDateFunction().Execute([date, dateOffset, dateFormat], context);
    }

    /// <summary>
    ///     Runs change date function with date and offset arguments.
    /// </summary>
    /// <param name="date">Date string to modify</param>
    /// <param name="dateOffset">Date offset string</param>
    /// <param name="context">Test context</param>
    /// <returns>Modified date with the default format</returns>
    public static string ChangeDate(string date, string dateOffset, TestContext context)
    {
        return new ChangeDateFunction().Execute([date, dateOffset], context);
    }

    /// <summary>
    ///     Runs encode base 64 function with content argument.
    /// </summary>
    /// <param name="content">Content to encode</param>
    /// <param name="context">Test context</param>
    /// <returns>Base64 encoded content</returns>
    public static string EncodeBase64(string content, TestContext context)
    {
        return new EncodeBase64Function().Execute([content], context);
    }

    /// <summary>
    ///     Runs encode base 64 function with content and charset arguments.
    /// </summary>
    /// <param name="content">Content to encode</param>
    /// <param name="charset">Character encoding</param>
    /// <param name="context">Test context</param>
    /// <returns>Base64 encoded content</returns>
    public static string EncodeBase64(string content, Encoding charset, TestContext context)
    {
        return new EncodeBase64Function().Execute([content, charset.WebName], context);
    }

    /// <summary>
    ///     Runs decode base 64 function with a content argument.
    /// </summary>
    /// <param name="content">Content to decode</param>
    /// <param name="context">Test context</param>
    /// <returns>Base64 decoded content</returns>
    public static string DecodeBase64(string content, TestContext context)
    {
        return new DecodeBase64Function().Execute([content], context);
    }

    /// <summary>
    ///     Runs decode base 64 function with content and charset arguments.
    /// </summary>
    /// <param name="content">Content to decode</param>
    /// <param name="charset">Character encoding</param>
    /// <param name="context">Test context</param>
    /// <returns>Base64 decoded content</returns>
    public static string DecodeBase64(string content, Encoding charset, TestContext context)
    {
        return new DecodeBase64Function().Execute([content, charset.WebName], context);
    }

    /// <summary>
    ///     Runs URL encode function with content argument.
    /// </summary>
    /// <param name="content">Content to encode</param>
    /// <param name="context">Test context</param>
    /// <returns>URL encoded content</returns>
    public static string UrlEncode(string content, TestContext context)
    {
        return new UrlEncodeFunction().Execute(new List<string> { content }, context);
    }

    /// <summary>
    ///     Runs URL encode function with content and charset arguments.
    /// </summary>
    /// <param name="content">Content to encode</param>
    /// <param name="charset">Character encoding</param>
    /// <param name="context">Test context</param>
    /// <returns>URL encoded content</returns>
    public static string UrlEncode(string content, Encoding charset, TestContext context)
    {
        return new UrlEncodeFunction().Execute(new List<string> { content, charset.WebName }, context);
    }

    /// <summary>
    ///     Runs URL decode function with content argument.
    /// </summary>
    /// <param name="content">Content to decode</param>
    /// <param name="context">Test context</param>
    /// <returns>URL decoded content</returns>
    public static string UrlDecode(string content, TestContext context)
    {
        return new UrlDecodeFunction().Execute(new List<string> { content }, context);
    }

    /// <summary>
    ///     Runs URL decode function with content and charset arguments.
    /// </summary>
    /// <param name="content">Content to decode</param>
    /// <param name="charset">Character encoding</param>
    /// <param name="context">Test context</param>
    /// <returns>URL decoded content</returns>
    public static string UrlDecode(string content, Encoding charset, TestContext context)
    {
        return new UrlDecodeFunction().Execute(new List<string> { content, charset.WebName }, context);
    }

    /// <summary>
    ///     Generates a digest authentication header based on the provided parameters.
    /// </summary>
    /// <param name="parameters">
    ///     An object containing the parameters required to generate the digest auth header, such as
    ///     username, password, realm, nonce key, method, URI, opaque value, and algorithm.
    /// </param>
    /// <param name="context">The test context used for the execution of the function.</param>
    /// <returns>A string representing the generated digest authentication header.</returns>
    public static string DigestAuthHeader(DigestAuthParameters parameters, TestContext context)
    {
        return new DigestAuthHeaderFunction().Execute(
        [
            parameters.Username, parameters.Password, parameters.Realm, parameters.NonceKey,
            parameters.Method, parameters.Uri, parameters.Opaque, parameters.Algorithm
        ], context);
    }


    /// <summary>
    ///     Runs a random UUID function with no arguments.
    /// </summary>
    /// <param name="context">Test context</param>
    /// <returns>Random UUID</returns>
    public static string RandomUuid(TestContext context)
    {
        return new RandomUuidFunction().Execute(new List<string>(), context);
    }

    /// <summary>
    ///     Runs random number function with length argument.
    /// </summary>
    /// <param name="length">Number length</param>
    /// <param name="context">Test context</param>
    /// <returns>Random number</returns>
    public static string RandomNumber(long length, TestContext context)
    {
        return new RandomNumberFunction().Execute(new List<string> { length.ToString() }, context);
    }

    /// <summary>
    ///     Runs random number function with length and padding arguments.
    /// </summary>
    /// <param name="length">Number length</param>
    /// <param name="padding">Whether to use padding</param>
    /// <param name="context">Test context</param>
    /// <returns>Random number</returns>
    public static string RandomNumber(long length, bool padding, TestContext context)
    {
        return new RandomNumberFunction().Execute(new List<string> { length.ToString(), padding.ToString() }, context);
    }

    /// <summary>
    ///     Runs random string function with number of letters argument.
    /// </summary>
    /// <param name="numberOfLetters">Number of letters</param>
    /// <param name="context">Test context</param>
    /// <returns>Random string</returns>
    public static string RandomString(long numberOfLetters, TestContext context)
    {
        return new RandomStringFunction().Execute(new List<string> { numberOfLetters.ToString() }, context);
    }

    /// <summary>
    ///     Runs random string function with number of letters and use numbers arguments.
    /// </summary>
    /// <param name="numberOfLetters">Number of letters</param>
    /// <param name="useNumbers">Whether to use numbers</param>
    /// <param name="context">Test context</param>
    /// <returns>Random string</returns>
    public static string RandomString(long numberOfLetters, bool useNumbers, TestContext context)
    {
        return RandomString(numberOfLetters, RandomStringFunction.Mixed, useNumbers, context);
    }

    /// <summary>
    ///     Runs random string function with number of letters, notation method, and use numbers arguments.
    /// </summary>
    /// <param name="numberOfLetters">Number of letters</param>
    /// <param name="notationMethod">Notation method</param>
    /// <param name="useNumbers">Whether to use numbers</param>
    /// <param name="context">Test context</param>
    /// <returns>Random string</returns>
    public static string RandomString(long numberOfLetters, string notationMethod, bool useNumbers, TestContext context)
    {
        return new RandomStringFunction().Execute(
            new List<string> { numberOfLetters.ToString(), notationMethod, useNumbers.ToString() }, context);
    }

    /// <summary>
    ///     Runs random string function with number of letters and notation method arguments.
    /// </summary>
    /// <param name="numberOfLetters">Number of letters</param>
    /// <param name="notationMethod">Notation method</param>
    /// <param name="context">Test context</param>
    /// <returns>Random string</returns>
    public static string RandomString(long numberOfLetters, string notationMethod, TestContext context)
    {
        return new RandomStringFunction().Execute(new List<string> { numberOfLetters.ToString(), notationMethod },
            context);
    }

    /// <summary>
    ///     Reads the file resource and returns the complete file content.
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <param name="context">Test context</param>
    /// <returns>File content</returns>
    public static string ReadFile(string filePath, TestContext context)
    {
        return new ReadFileResourceFunction().Execute(new List<string> { filePath }, context);
    }

    /// <summary>
    ///     Runs unix timestamp function with no arguments.
    /// </summary>
    /// <param name="context">Test context</param>
    /// <returns>Unix timestamp</returns>
    public static string UnixTimestamp(TestContext context)
    {
        return new UnixTimestampFunction().Execute(new List<string>(), context);
    }


    /// <summary>
    ///     Represents the parameters required for constructing a digest authentication header.
    /// </summary>
    public class DigestAuthParameters
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Realm { get; set; }
        public string NonceKey { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Opaque { get; set; }
        public string Algorithm { get; set; }
    }
}
