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

namespace Agenix.Api.Spi;

/// <summary>
///     Abstract base class for building test actions with reference resolver capabilities.
///     This class is intended to be inherited by other test action builders that require
///     access to a reference resolver to resolve dependencies or references during their construction or execution.
/// </summary>
/// <typeparam name="T">The type of the test action that this builder creates.</typeparam>
/// <remarks>
///     This class implements both <see cref="ITestActionBuilder{T}.IDelegatingTestActionBuilder{T}" /> and
///     <see cref="IReferenceResolverAware" />.
///     It facilitates delegation to another implementation of <see cref="ITestActionBuilder{T}" /> and seamlessly
///     integrates with a reference resolver
///     through the <see cref="SetReferenceResolver" /> method.
/// </remarks>
/// <example>
///     This class should be extended by concrete implementations to provide the logic for constructing specific types of
///     test actions.
/// </example>
public abstract class
    AbstractReferenceResolverAwareTestActionBuilder<T> : ITestActionBuilder<T>.IDelegatingTestActionBuilder<T>,
    IReferenceResolverAware where T : ITestAction
{
    protected ITestActionBuilder<T> _delegate;
    protected IReferenceResolver referenceResolver;

    public void SetReferenceResolver(IReferenceResolver newReferenceResolver)
    {
        if (referenceResolver != null)
        {
            referenceResolver = newReferenceResolver;

            if (Delegate is IReferenceResolverAware referenceResolverAware)
                referenceResolverAware.SetReferenceResolver(referenceResolver);
        }
    }

    public ITestActionBuilder<T> Delegate => _delegate;

    public abstract T Build();
}
