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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents a question in the Screenplay pattern to verify the presence of a specific memory
///     associated with an <see cref="Actor" />.
/// </summary>
public class TheMemory : IQuestion<bool>
{
    private readonly string _memoryKey;

    private TheMemory(string memoryKey)
    {
        _memoryKey = memoryKey;
    }

    public bool AnsweredBy(Actor actor)
    {
        return actor.Recall<dynamic>(_memoryKey) != null;
    }

    /// <summary>
    ///     Initializes a builder to create a memory question with the specified memory key.
    /// </summary>
    /// <param name="memoryKey">The key to identify the memory to be questioned.</param>
    /// <returns>A <see cref="TheMemoryQuestionBuilder" /> instance initialized with the specified memory key.</returns>
    public static TheMemoryQuestionBuilder WithKey(string memoryKey)
    {
        return new TheMemoryQuestionBuilder(memoryKey);
    }

    /// <summary>
    ///     Provides a builder for creating instances of <see cref="TheMemory" /> questions.
    /// </summary>
    public class TheMemoryQuestionBuilder(string memoryKey)
    {
        public TheMemory IsPresent()
        {
            return new TheMemory(memoryKey);
        }
    }
}
