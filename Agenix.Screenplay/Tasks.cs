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
///     Provides utility methods to create and manage task instances that implement the IPerformable interface.
/// </summary>
public class Tasks
{
    /// <summary>
    ///     Creates an instance of a specified step class, ensuring that the instance implements the IPerformable interface.
    /// </summary>
    /// <typeparam name="T">The type that implements the IPerformable interface.</typeparam>
    /// <param name="stepClass">The type of the step class to instantiate.</param>
    /// <param name="parameters">An array of parameters required to initialize the step class.</param>
    /// <returns>An instance of the specified step class cast to the type T.</returns>
    public static T Instrumented<T>(params object[] parameters) where T : IPerformable
    {
        return DynamicConstructorBuilder.CreateInstance<T>(parameters);
    }
}
