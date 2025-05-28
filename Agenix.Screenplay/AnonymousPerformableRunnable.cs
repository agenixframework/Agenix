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
///     Represents an anonymous, executable task or action that can be performed by an actor within the screenplay pattern.
/// </summary>
/// <remarks>
///     The AnonymousPerformableRunnable class provides a way to define performable tasks or actions
///     inline using a delegate. It encapsulates an action that can be executed by any actor.
///     This supports concise and flexible task definition to enhance the readability of actor behaviors.
/// </remarks>
/// <example>
///     This class is typically used when you want to define a performable action inline for simplicity.
/// </example>
public class AnonymousPerformableRunnable(Action actions) : IPerformable
{
    public void PerformAs<T>(T actor) where T : Actor
    {
        actions();
    }
}
