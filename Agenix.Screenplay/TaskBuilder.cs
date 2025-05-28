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

using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay;

/// <summary>
///     A builder class for creating tasks in a screenplay-style testing framework.
/// </summary>
/// <remarks>
///     The <see cref="TaskBuilder" /> class allows the creation of an <see cref="AnonymousTask" />
///     with a specific title and a series of steps for an actor to perform. This builder
///     simplifies the process of defining tasks by providing a fluent API.
/// </remarks>
public class TaskBuilder(string title)
{
    /// <summary>
    ///     Defines an anonymous task where the actor attempts to perform the specified steps.
    /// </summary>
    /// <typeparam name="T">The type of steps to be performed, implementing <see cref="IPerformable" />.</typeparam>
    /// <param name="steps">An array of steps that the actor will attempt to perform in the task.</param>
    /// <returns>An <see cref="AnonymousTask" /> representing the task that consists of the provided steps.</returns>
    public AnonymousTask WhereTheActorAttemptsTo<T>(params T[] steps) where T : IPerformable
    {
        return ITask.Where(title, steps);
    }
}
