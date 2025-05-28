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

using Agenix.Api.Context;

namespace Agenix.Api.Validation;

/// <summary>
///     Delegate for executing validation logic on a specified payload.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
/// <param name="payload">The message payload to be validated.</param>
/// <param name="headers">A dictionary containing header information related to the payload.</param>
/// <param name="context">The context in which the test is executed, providing various utilities and information.</param>
public delegate void GenericValidationProcessor<in T>(T payload, IDictionary<string, object> headers,
    TestContext context);

/// <summary>
///     Provides a contract for processing generic validation mechanisms.
///     Implementing classes should override the Validate method to specify the validation logic.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
public interface IGenericValidationProcessor<in T>
{
    /// <summary>
    ///     Subclasses do override this method for validation purposes.
    /// </summary>
    /// <param name="payload">The message payload object.</param>
    /// <param name="headers">The message headers.</param>
    /// <param name="context">The current test context.</param>
    void Validate(T payload, IDictionary<string, object> headers, TestContext context);
}
