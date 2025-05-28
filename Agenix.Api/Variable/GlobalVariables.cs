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

namespace Agenix.Api.Variable;

/// Represents a collection of global variables accessible in each test case.
/// Allows for managing and storing variables using a builder pattern.
/// /
public class GlobalVariables
{
    /// Stores a collection of key-value pairs representing global variables.
    /// /
    private readonly Dictionary<string, object> _variables;

    public GlobalVariables() : this(new Builder())
    {
    }

    public GlobalVariables(Builder builder)
    {
        _variables = builder.Variables;
    }

    /// Get the global variables.
    /// @return the global variables as a dictionary of key-value pairs.
    /// /
    public Dictionary<string, object> GetVariables()
    {
        return _variables;
    }

    /// Provides a builder for creating and managing global variables.
    /// Supports a fluent interface for defining variables and collections of variables.
    /// Used in conjunction with the GlobalVariables class to construct instances with specified configurations.
    public class Builder
    {
        public Dictionary<string, object> Variables { get; } = [];

        public Builder WithVariable(string name, object value)
        {
            Variables[name] = value;
            return this;
        }

        public Builder WithVariables(Dictionary<string, object> variables)
        {
            foreach (var item in variables) Variables[item.Key] = item.Value;
            return this;
        }

        public GlobalVariables Build()
        {
            return new GlobalVariables(this);
        }
    }
}
