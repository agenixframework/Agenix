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

using Agenix.Api;

namespace Agenix.Core;

/// Default implementation of AgenixContext provider. It facilitates the generation of
/// AgenixContext instances according to a specified creation strategy. The provider
/// can generate a new context instance upon each request or return a consistent,
/// singleton instance, as defined by the AgenixInstanceStrategy.
/// /
public class DefaultAgenixContextProvider(AgenixInstanceStrategy strategy) : IAgenixContextProvider
{
    public static string Spring = "spring";

    private static AgenixContext _context;

    public DefaultAgenixContextProvider() : this(AgenixInstanceStrategy.SINGLETON)
    {
    }

    /// <summary>
    ///     Creates an instance of the <see cref="AgenixContext" /> based on the current strategy.
    ///     If the strategy is set to NEW, or the context has not been initialized, a new instance
    ///     of <see cref="AgenixContext" /> is created. Otherwise, the existing context is returned.
    /// </summary>
    /// <returns>
    ///     An instance of the <see cref="AgenixContext" />. Depending on the strategy,
    ///     it may either be a new instance or a reused existing instance.
    /// </returns>
    public AgenixContext Create()
    {
        if (strategy.Equals(AgenixInstanceStrategy.NEW) || _context == null)
        {
            _context = AgenixContext.Create();
        }

        return _context;
    }
}
