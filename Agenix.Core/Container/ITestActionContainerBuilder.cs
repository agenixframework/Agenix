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

using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Container;

namespace Agenix.Core.Container;

/// <summary>
///     Interface for building test action containers.
/// </summary>
/// <typeparam name="T">The type of test action container.</typeparam>
/// <typeparam name="TS">The type of the builder.</typeparam>
public interface ITestActionContainerBuilder<out T, out S> : ITestActionBuilder<T>
    where T : ITestActionContainer
    where S : class
{
    /// @param actions An array of actions to be added.
    /// @return The updated instance of the action container builder.
    /// /
    public S Actions(params ITestAction[] actions);

    /// Adds test actions to the action container.
    /// @param actions An array of actions to be added.
    /// @return The updated instance of the action container builder.
    public S Actions(params ITestActionBuilder<ITestAction>[] actions);

    /// Get the list of test actions for this container.
    /// @return List of test actions.
    public List<ITestActionBuilder<ITestAction>> GetActions();
}
