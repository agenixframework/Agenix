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
///     Represents an interface for entities capable of performing tasks and asking questions.
/// </summary>
public interface IPerformsTasks
{
    /// <summary>
    ///     Allows an actor to perform one or more tasks or actions represented by the <see cref="IPerformable" /> interface.
    /// </summary>
    /// <typeparam name="T">The type of tasks or actions to be performed, which must implement <see cref="IPerformable" />.</typeparam>
    /// <param name="todos">A collection of tasks or actions that the actor will attempt to perform.</param>
    void AttemptsTo<T>(params T[] todos) where T : IPerformable;

    /// <summary>
    ///     Allows an actor to request an answer to a specified question represented by the <see cref="IQuestion{TAnswer}" />
    ///     interface.
    /// </summary>
    /// <typeparam name="ANSWER">The type of the answer expected from the question.</typeparam>
    /// <param name="question">The question from which the actor seeks an answer.</param>
    /// <returns>The result of the question, which is of type <typeparamref name="ANSWER" />.</returns>
    ANSWER AsksFor<ANSWER>(IQuestion<ANSWER> question);
}
