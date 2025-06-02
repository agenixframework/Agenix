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

using System.Diagnostics;
using Agenix.Screenplay.Utils;

namespace Agenix.Screenplay;

/// <summary>
///     Provides functionality for generating a human-readable task name based on the currently executing method.
/// </summary>
public class HumanReadableTaskName
{
    /// Generates a human-readable task name for the currently executing method.
    /// <returns>
    ///     A string representing the human-readable name for the current method,
    ///     formatted by combining the class name and method name, and processed through the NameConverter.Humanize method.
    ///     The result excludes any methods defined within the "Agenix" namespace hierarchy.
    /// </returns>
    public static string ForCurrentMethod()
    {
        var stackTrace = new StackTrace();
        var businessIndex = 1;

        for (var methodIndex = 1; methodIndex < stackTrace.FrameCount; methodIndex++)
        {
            var frame = stackTrace.GetFrame(methodIndex);
            var method = frame?.GetMethod();
            if (method?.DeclaringType?.FullName?.StartsWith("Agenix") == true)
            {
                continue;
            }

            businessIndex = methodIndex;
            break;
        }

        var newFrame = stackTrace.GetFrame(businessIndex);
        var declaringType = newFrame?.GetMethod()?.DeclaringType;
        var className = declaringType?.FullName ?? "";
        var methodName = newFrame?.GetMethod()?.Name ?? "";

        var simpleClassName = className.Contains(".")
            ? className.Substring(className.LastIndexOf('.') + 1)
            : className;

        return NameConverter.Humanize($"{simpleClassName}_{methodName}");
    }
}
