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
        Members.Add("EscapeXml", new EscapeXmlFunction());
        Members.Add("CurrentDate", new CurrentDateFunction());
        Members.Add("LowerCase", new LowerCaseFunction());
        Members.Add("RandomString", new RandomStringFunction());
        Members.Add("RandomNumber", new RandomNumberFunction());
        Members.Add("EncodeBase64", new EncodeBase64Function());
        Members.Add("DecodeBase64", new DecodeBase64Function());
        Members.Add("Translate", new TranslateFunction());

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
            Log.LogDebug($"Register function '{pair.Key}' as {pair.Value.GetType()}");
        }
    }
}
