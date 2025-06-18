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

using System.Collections.Generic;
using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;

namespace Agenix.Core.Message.Selector;

/// <summary>
///     Message selector delegates incoming messages to several other selector implementations according to selector names.
/// </summary>
public class DelegatingMessageSelector : IMessageSelector
{
    /// Instance of TestContext used within the DelegatingMessageSelector.
    /// /
    private readonly TestContext _context;

    /// List of factories for creating message selectors based on key-value pairs
    /// /
    private readonly List<IMessageSelector.IMessageSelectorFactory> _factories;

    /// Dictionary containing headers to match against messages, where the key is the header name and the value is the expected header value
    /// /
    private readonly IDictionary<string, string> _matchingHeaders;

    public DelegatingMessageSelector(string selector, TestContext context)
    {
        _context = context;
        _matchingHeaders = MessageSelectorBuilder.WithString(selector).ToKeyValueDictionary();

        if (_matchingHeaders.Count == 0)
        {
            throw new AgenixSystemException("Invalid empty message selector");
        }

        _factories = [];

        if (context.ReferenceResolver != null)
        {
            _factories.AddRange(context.ReferenceResolver.ResolveAll<IMessageSelector.IMessageSelectorFactory>()
                .Values);
        }

        _factories.AddRange(IMessageSelector.Lookup().Values);
    }

    /// <summary>
    ///     Determines if the given message matches the selection criteria.
    /// </summary>
    /// <param name="message">The message to evaluate against the selection criteria.</param>
    /// <return>True if the message matches the criteria; otherwise, false.</return>
    public bool Accept(IMessage message)
    {
        return (from entry in _matchingHeaders
            let factory =
                _factories.FirstOrDefault(f => f.Supports(entry.Key)) ?? new HeaderMatchingMessageSelector.Factory()
            select factory.Create(entry.Key, entry.Value, _context)).All(selector => selector.Accept(message));
    }

    /// <summary>
    ///     Add message selector factory to list of delegates.
    /// </summary>
    /// <param name="factory"></param>
    public void AddMessageSelectorFactory(IMessageSelector.IMessageSelectorFactory factory)
    {
        _factories.Add(factory);
    }
}
