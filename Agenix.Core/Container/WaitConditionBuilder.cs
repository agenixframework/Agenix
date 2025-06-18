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

using System;
using Agenix.Api;
using Agenix.Api.Condition;

namespace Agenix.Core.Container;

/// <summary>
///     An abstract builder class responsible for creating instances of a Wait action
///     with configurable conditions and intervals.
/// </summary>
/// <typeparam name="T">The type that implements the ICondition interface.</typeparam>
/// <typeparam name="S">The specific type of the WaitConditionBuilder.</typeparam>
public abstract class WaitConditionBuilder<T, S> : ITestActionBuilder<Wait>
    where T : ICondition where S : WaitConditionBuilder<T, S>
{
    /**
     * Parent wait action builder
     */
    private readonly Wait.Builder<T> _builder;

    /**
     * Self reference
     */
    protected readonly S self;

    /**
     * Default constructor using fields.
     * @param builder
     */
    public WaitConditionBuilder(Wait.Builder<T> builder)
    {
        _builder = builder;
        self = (S)this;
    }

    public Wait Build()
    {
        return _builder.Build();
    }

    /// Sets the interval in milliseconds between each test of the condition.
    /// @param interval The interval to use in milliseconds.
    /// @return The modified instance of the WaitConditionBuilder.
    /// /
    public S Interval(long interval)
    {
        return Interval(interval.ToString());
    }

    /// Sets the interval for condition testing.
    /// <param name="interval">The interval value to be used for condition testing.</param>
    /// <return>The modified instance of the WaitConditionBuilder.</return>
    public S Interval(string interval)
    {
        _builder.Interval(interval);
        return self;
    }

    /// Sets the interval in milliseconds between each test of the condition.
    /// <param name="milliseconds">The interval in milliseconds to use.</param>
    /// <return>The modified instance of the WaitConditionBuilder.</return>
    public S Milliseconds(long milliseconds)
    {
        _builder.Milliseconds(milliseconds);
        return self;
    }

    /// Sets the interval in milliseconds between each test of the condition.
    /// <param name="milliseconds">The interval in milliseconds to use.</param>
    /// <return>The modified instance of the WaitConditionBuilder.</return>
    public S Milliseconds(string milliseconds)
    {
        _builder.Milliseconds(milliseconds);
        return self;
    }

    /// Sets the duration in seconds for the wait condition.
    /// @param seconds The time in seconds to wait.
    /// @return The modified instance of the WaitConditionBuilder.
    /// /
    public S Seconds(double seconds)
    {
        _builder.Seconds(seconds);
        return self;
    }

    /// Sets the duration for the wait condition.
    /// @param duration The timespan representing the duration to wait.
    /// @return The modified instance of the WaitConditionBuilder.
    /// /
    public S Time(TimeSpan duration)
    {
        _builder.Time(duration);
        return self;
    }

    /// Retrieves the condition associated with this WaitConditionBuilder.
    /// @return The condition of type T.
    /// /
    public T GetCondition()
    {
        return _builder.GetCondition;
    }

    /// Gets the builder associated with the wait condition.
    /// @return The current instance of the Wait.Builder.
    /// /
    public Wait.Builder<T> GetBuilder()
    {
        return _builder;
    }
}
