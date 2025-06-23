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

using System.Text;
using Agenix.Api.Report;

namespace Agenix.Api.Exceptions;

/// <summary>
///     Basic custom runtime/ system exception for all errors in Agenix
/// </summary>
[Serializable]
public class AgenixSystemException : SystemException
{
    private List<FailureStackElement> _failureStack = [];

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public AgenixSystemException()
    {
    }

    public AgenixSystemException(string message) : base(message)
    {
    }

    public AgenixSystemException(string message, Exception cause) : base(message, cause)
    {
    }

    public override string Message => base.Message + GetFailureStackAsString();

    public string GetMessage()
    {
        return Message;
    }

    /// <summary>
    ///     Get formatted string representation of failure stack information.
    /// </summary>
    /// <returns></returns>
    public string GetFailureStackAsString()
    {
        var builder = new StringBuilder();

        foreach (var failureStackElement in GetFailureStack())
        {
            builder.Append("\n\t");
            builder.Append(failureStackElement.GetStackMessage());
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Sets the custom failure stack holding line number information inside test case.
    /// </summary>
    /// <param name="failureStack"></param>
    public void SetFailureStack(List<FailureStackElement> failureStack)
    {
        _failureStack = failureStack;
    }

    /// <summary>
    ///     Gets the custom failure stack with line number information where the testcase failed.
    /// </summary>
    /// <returns>the failureStack</returns>
    public Stack<FailureStackElement> GetFailureStack()
    {
        var stack = new Stack<FailureStackElement>();

        foreach (var failureStackElement in _failureStack)
        {
            stack.Push(failureStackElement);
        }

        return stack;
    }
}
