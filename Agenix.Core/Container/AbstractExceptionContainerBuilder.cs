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

using Agenix.Api;

namespace Agenix.Core.Container;

/// <summary>
///     Represents an abstract builder for exception containers that handle specific action configurations
///     prior to their execution within the framework. This class provides methods to add actions or action builders
///     to the container before execution.
/// </summary>
/// <typeparam name="T">
///     The type of the action container associated with this builder. It must inherit from
///     <see cref="AbstractActionContainer" />.
/// </typeparam>
/// <typeparam name="S">
///     The type of the builder that implements this abstract class. It must inherit from
///     <see cref="AbstractExceptionContainerBuilder{T, S}" />.
/// </typeparam>
public abstract class AbstractExceptionContainerBuilder<T, S> : AbstractTestContainerBuilder<T, S>
    where T : AbstractActionContainer
    where S : AbstractExceptionContainerBuilder<T, S>
{
    /// Adds specified actions to the container before its execution.
    /// This method accepts action instances.
    /// @param actions An array of action instances to be added to the container.
    /// @return An instance of the container with the specified actions included.
    /// /
    public S When(params ITestAction[] actions)
    {
        return Actions(actions);
    }

    /// Adds specified actions to the container before its execution. This version accepts action builders.
    /// @param actions An array of action builders that create actions to be added to the container.
    /// @return An instance of the container with the specified actions added.
    /// /
    public S When(params ITestActionBuilder<ITestAction>[] actions)
    {
        return Actions(actions);
    }
}
