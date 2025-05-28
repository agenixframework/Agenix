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

using Agenix.Api.Condition;
using Agenix.Api.Context;

namespace Agenix.Core.Condition;

/// <summary>
///     Represents an abstract base class for defining specific conditions.
///     This class provides an implementation of the ICondition interface,
///     managing the name of the condition and requiring derived classes to
///     implement the core condition functionalities.
/// </summary>
public abstract class AbstractCondition : ICondition
{
    /// The condition name
    private readonly string _name;

    /// Represents an abstract condition that serves as a base class for specific condition implementations.
    /// Implements the ICondition interface and provides functionality for condition name management.
    /// /
    public AbstractCondition()
    {
        _name = GetType().Name;
    }

    /// Represents an abstract condition that implements the ICondition interface.
    /// Provides basic functionality for retrieving the name of the condition.
    /// /
    public AbstractCondition(string name)
    {
        _name = name;
    }

    public string GetName()
    {
        return _name;
    }

    public abstract bool IsSatisfied(TestContext context);
    public abstract string GetSuccessMessage(TestContext context);
    public abstract string GetErrorMessage(TestContext context);
}
