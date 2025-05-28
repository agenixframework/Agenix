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

using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Validation;

/// <summary>
///     Unit tests for the <see cref="MessageValidator" /> class.
/// </summary>
public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        ClassicAssert.AreEqual(3, validators.Count);
        ClassicAssert.IsNotNull(validators["header"]);
        ClassicAssert.AreEqual(validators["header"].GetType(), typeof(DefaultMessageHeaderValidator));
        ClassicAssert.IsNotNull(validators["json"]);
        ClassicAssert.AreEqual(validators["json"].GetType(),
            typeof(JsonTextMessageValidator));
        ClassicAssert.IsNotNull(validators["json-path"]);
        ClassicAssert.AreEqual(validators["json-path"].GetType(),
            typeof(JsonPathMessageValidator));
    }

    [Test]
    public void TestTestLookup()
    {
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("header").IsPresent);
    }
}
