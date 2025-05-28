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

namespace Agenix.Screenplay;

/// <summary>
///     Represents an actionable behavior or operation that can be performed within a screenplay context.
/// </summary>
public interface Interaction : IPerformable
{
    /// <summary>
    ///     Creates an instance of <see cref="AnonymousInteraction" /> that encapsulates a performable operation with a
    ///     specified title and a list of steps.
    /// </summary>
    /// <param name="title">The descriptive title of the interaction, representing its purpose or intent.</param>
    /// <param name="steps">An array of <see cref="IPerformable" /> tasks composing the interaction.</param>
    /// <returns>An instance of <see cref="AnonymousInteraction" /> configured with the specified title and steps.</returns>
    public static AnonymousInteraction Where(string title, params IPerformable[] steps)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, steps.ToList());
    }

    /// <summary>
    ///     Creates an instance of <see cref="AnonymousInteraction" /> that encapsulates a performable operation with a
    ///     specified title and operation.
    /// </summary>
    /// <param name="title">The title of the interaction, representing its purpose or description.</param>
    /// <param name="performableOperation">The performable operation defined as an action involving an <see cref="Actor" />.</param>
    /// <returns>
    ///     An instance of <see cref="AnonymousInteraction" /> configured with the specified title and performable
    ///     operation.
    /// </returns>
    public static AnonymousInteraction Where(string title, Action<Actor> performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, performableOperation);
    }

    /// <summary>
    ///     Creates an instance of <see cref="AnonymousInteraction" /> that encapsulates a performable operation with a
    ///     specified title.
    /// </summary>
    /// <param name="title">The title of the interaction, representing its purpose or description.</param>
    /// <param name="performableOperation">The performable operation to be executed as part of the interaction.</param>
    /// <returns>
    ///     An instance of <see cref="AnonymousInteraction" /> configured with the specified title and performable
    ///     operation.
    /// </returns>
    public static AnonymousInteraction ThatPerforms(string title, Action performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, performableOperation);
    }
}
