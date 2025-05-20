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

        if (_matchingHeaders.Count == 0) throw new AgenixSystemException("Invalid empty message selector");

        _factories = [];

        if (context.ReferenceResolver != null)
            _factories.AddRange(context.ReferenceResolver.ResolveAll<IMessageSelector.IMessageSelectorFactory>()
                .Values);

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