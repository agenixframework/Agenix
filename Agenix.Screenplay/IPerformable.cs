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

/**
 * A task or action that can be performed by an actor.
 *
 *
 * It is common to use builder methods to create instances of the Performable class, in order to make the tests read
 * more fluently. For example:
 *
 * [source,c#]
 * --
 * purchase().anApple().thatCosts(0).dollars()
 * --
 */
public interface IPerformable
{
    /// <summary>
    ///     Performs the specified action or task using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the action.</typeparam>
    /// <param name="actor">The actor that will perform the action or task.</param>
    void PerformAs<T>(T actor) where T : Actor;

    /// <summary>
    ///     Chains the current performable to another performable, creating a composite performable that will execute both in
    ///     sequence.
    /// </summary>
    /// <param name="nextPerformable">The next performable to be executed.</param>
    /// <returns>A composite performable combining the current and the next performable.</returns>
    IPerformable Then(IPerformable nextPerformable)
    {
        return CompositePerformable.From(this, nextPerformable);
    }
}
