using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Variable.Dictionary;
using Agenix.Validation.Json.Validation;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Json.Variable.Dictionary.Json;

/// <summary>
/// JSON data dictionary implementation maps elements via JsonPath expressions. When an element is identified by some expression
/// in the dictionary, the value is overwritten accordingly.
/// </summary>
public class JsonPathMappingDataDictionary : AbstractJsonDataDictionary
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonPathMappingDataDictionary));

    public override void ProcessMessage(IMessage message, TestContext context)
    {
        if (message.Payload == null || string.IsNullOrWhiteSpace(message.GetPayload<string>()))
        {
            return;
        }

        var delegateProcessor = new JsonPathMessageProcessor.Builder()
            .IgnoreNotFound(true)
            .Expressions(Mappings.ToDictionary(
                entry => entry.Key,
                entry => (object)entry.Value))
            .Build();

        delegateProcessor.ProcessMessage(message, context);
    }

    public override T Translate<T>(string jsonPath, T value, TestContext context)
    {
        return value;
    }

    public override void Initialize()
    {
        if (PathMappingStrategy != null && !PathMappingStrategy.Equals(PathMappingStrategy.EXACT))
        {
            Log.LogWarning("{ClassName} ignores path mapping strategy other than {Strategy}",
                GetType().Name, PathMappingStrategy.EXACT);
        }

        base.Initialize();
    }
}
