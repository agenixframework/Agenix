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
///     Represents a composite of multiple tasks or actions that can be performed by an actor.
/// </summary>
/// <remarks>
///     A composite performable enables the combination of multiple actions or tasks into a single,
///     cohesive unit that can be executed sequentially by an actor.
/// </remarks>
public class CompositePerformable : IPerformable
{
    private readonly List<IPerformable> _todoList;

    private CompositePerformable(List<IPerformable> todoList)
    {
        _todoList = todoList;
    }

    /// <summary>
    ///     Performs all actions or tasks in the composite list using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the actions.</typeparam>
    /// <param name="actor">The actor that will execute each performable in the composite list.</param>
    public void PerformAs<T>(T actor) where T : Actor
    {
        foreach (var todo in _todoList) actor.AttemptsTo(todo);
    }

    /// <summary>
    ///     Combines two performables into a composite performable.
    /// </summary>
    /// <param name="firstPerformable">The first performable to be included in the composite.</param>
    /// <param name="nextPerformable">The next performable to be included in the composite.</param>
    /// <returns>
    ///     A new composite performable containing the combined actions of the specified performables.
    /// </returns>
    public static IPerformable From(IPerformable firstPerformable, IPerformable nextPerformable)
    {
        var todoList = new List<IPerformable>();
        todoList.AddRange(Flattened(firstPerformable));
        todoList.AddRange(Flattened(nextPerformable));
        return new CompositePerformable(todoList);
    }

    /// <summary>
    ///     Flattens the given performable into a list of individual performables,
    ///     expanding composite performables into their constituent actions.
    /// </summary>
    /// <param name="performable">
    ///     The performable to be flattened. If it is a composite performable, its actions will be
    ///     extracted.
    /// </param>
    /// <returns>
    ///     A list of individual performables. If the input is not a composite performable, the list will contain the
    ///     input performable itself.
    /// </returns>
    private static List<IPerformable> Flattened(IPerformable performable)
    {
        if (performable is CompositePerformable composite) return composite._todoList;

        return [performable];
    }
}
