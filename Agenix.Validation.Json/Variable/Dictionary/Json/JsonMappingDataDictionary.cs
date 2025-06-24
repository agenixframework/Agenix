using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Variable.Dictionary;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Variable.Dictionary.Json;

/// <summary>
/// Simple json data dictionary implementation holds a set of mappings where keys are json path expressions to match
/// json object graph. Parses message payload to json object tree. Traverses
/// through json data supporting nested json objects, arrays and values.
/// </summary>
public class JsonMappingDataDictionary : AbstractJsonDataDictionary
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

        try
        {
            var json = JToken.Parse(message.GetPayload<string>());

            switch (json)
            {
                case JObject jsonObject:
                    TraverseJsonData(jsonObject, "", context);
                    break;
                case JArray jsonArray:
                    {
                        var tempJson = new JObject { ["root"] = jsonArray };
                        TraverseJsonData(tempJson, "", context);
                        break;
                    }
                default:
                    throw new AgenixSystemException($"Unsupported json type {json.GetType()}");
            }

            message.Payload = json.ToString();
        }
        catch (JsonReaderException e)
        {
            Log.LogWarning(e, "Data dictionary unable to parse JSON object");
        }
    }

    public override T Translate<T>(string jsonPath, T value, TestContext context)
    {
        switch (PathMappingStrategy)
        {
            case PathMappingStrategy.EXACT when Mappings.ContainsKey(jsonPath):
                {
                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.LogDebug("Data dictionary setting element '{JsonPath}' with value: {Value}",
                            jsonPath, Mappings[jsonPath]);
                    }
                    return ConvertIfNecessary(Mappings[jsonPath], value, context);
                }
            case PathMappingStrategy.ENDS_WITH:
                {
                    foreach (var entry in Mappings.Where(entry => jsonPath.EndsWith(entry.Key)))
                    {
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogDebug("Data dictionary setting element '{JsonPath}' with value: {Value}",
                                jsonPath, entry.Value);
                        }
                        return ConvertIfNecessary(entry.Value, value, context);
                    }

                    break;
                }
            case PathMappingStrategy.STARTS_WITH:
                {
                    foreach (var entry in Mappings.Where(entry => jsonPath.StartsWith(entry.Key)))
                    {
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogDebug("Data dictionary setting element '{JsonPath}' with value: {Value}",
                                jsonPath, entry.Value);
                        }
                        return ConvertIfNecessary(entry.Value, value, context);
                    }

                    break;
                }
        }

        return value;
    }

    private object? ParseMappingValue(object? value)
    {
        // Convert to string first for parsing
        var stringValue = value?.ToString();

        if (string.IsNullOrEmpty(stringValue))
            return value;

        // Try to parse as integer
        if (int.TryParse(stringValue, out int intValue))
            return intValue;

        // Try to parse as double
        if (double.TryParse(stringValue, out double doubleValue))
            return doubleValue;

        // Try to parse as boolean
        if (bool.TryParse(stringValue, out bool boolValue))
            return boolValue;

        // Check for null
        return stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ? null :
            // Default to original value
            value;
    }



    /// <summary>
    /// Walks through the Json object structure and translates values based on element path if necessary.
    /// </summary>
    /// <param name="jsonData">The JSON object to traverse</param>
    /// <param name="jsonPath">The current JSON path</param>
    /// <param name="context">The test context</param>
    private void TraverseJsonData(JObject jsonData, string jsonPath, TestContext context)
    {
        foreach (var property in jsonData.Properties().ToList())
        {
            var currentPath = string.IsNullOrWhiteSpace(jsonPath)
                ? property.Name
                : $"{jsonPath}.{property.Name}";

            if (property.Value is JObject nestedObject)
            {
                TraverseJsonData(nestedObject, currentPath, context);
            }
            else if (property.Value is JArray jsonArray)
            {
                for (var i = 0; i < jsonArray.Count; i++)
                {
                    var arrayPath = $"{currentPath}[{i}]";
                    if (jsonArray[i] is JObject arrayObject)
                    {
                        TraverseJsonData(arrayObject, arrayPath, context);
                    }
                    else
                    {
                        var translatedValue = Translate(arrayPath, jsonArray[i].ToObject<object>(), context);
                        jsonArray[i] = JToken.FromObject(ParseMappingValue(translatedValue) ?? JValue.CreateNull());
                    }
                }
            }
            else
            {
                var originalValue = property.Value.ToObject<object>();
                var translatedValue = Translate(currentPath, originalValue, context);
                property.Value = JToken.FromObject(ParseMappingValue(translatedValue) ?? JValue.CreateNull());
            }
        }
    }
}
