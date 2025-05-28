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

namespace Agenix.Screenplay.Annotations;

/// <summary>
///     Represents an anonymous task that can be performed by an actor.
/// </summary>
/// <remarks>
///     AnonymousTask extends the functionality of <see cref="AnonymousPerformable" />
///     by implementing the <see cref="ITask" /> interface. This allows defining tasks
///     with a title, steps, and custom field values to be used within a screenplay-style
///     testing framework.
/// </remarks>
public class AnonymousTask : AnonymousPerformable, ITask
{
    public AnonymousTask()
    {
    }

    public AnonymousTask(string title, List<IPerformable> steps)
        : base(title, steps)
    {
    }

    /// <summary>
    ///     Sets a custom field value for the current AnonymousTask instance.
    /// </summary>
    /// <param name="fieldName">The name of the field to be set.</param>
    /// <returns>
    ///     An instance of <see cref="AnonymousPerformableFieldSetter{AnonymousTask}" />
    ///     that enables chaining to specify the field value.
    /// </returns>
    public AnonymousPerformableFieldSetter<AnonymousTask> With(string fieldName)
    {
        return new AnonymousPerformableFieldSetter<AnonymousTask>(this, fieldName);
    }
}
