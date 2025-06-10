using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Variable.Dictionary;

/// <summary>
/// Represents a simple key-value mapping dictionary derived from AbstractDataDictionary.
/// This class is used to process messages and update payloads based on key-value mappings.
/// </summary>
public class SimpleMappingDictionary(System.Collections.Generic.Dictionary<string, string> mappings)
    : AbstractDataDictionary<string>
{
    public SimpleMappingDictionary() : this(new System.Collections.Generic.Dictionary<string, string>())
    {
    }

    public override void ProcessMessage(IMessage message, TestContext context)
    {
        var payload = message.GetPayload<string>();

        payload = mappings.Aggregate(payload, (current, mapping) => current.Replace(mapping.Key, mapping.Value));

        message.Payload = payload;
    }

    public override R Translate<R>(string key, R value, TestContext context)
    {
        return value;
    }

    public override bool SupportsMessageType(string messageType)
    {
        return true;
    }
}
