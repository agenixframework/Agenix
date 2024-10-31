using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Message;

namespace Agenix.Core.Log;

/// <summary>
///     Special modifier adds message related modifications on logger output on headers and body.
/// </summary>
public abstract class LogMessageModifierBase : ILogModifier
{
    public abstract string Mask(string statement);

    /// <summary>
    ///     Mask the given message body to not print sensitive data.
    /// </summary>
    /// <param name="message">the message</param>
    /// <returns></returns>
    public string MaskBody(IMessage message)
    {
        return Mask(message.GetPayload<string>().Trim());
    }

    /// <summary>
    ///     Mask the given message header values to not print sensitive data.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Dictionary<string, object> MaskHeaders(IMessage message)
    {
        var newDict = new Dictionary<string, object>();
        foreach (var entry in message.GetHeaders())
        {
            if (entry.Value == null)
            {
                newDict.Add(entry.Key, "");
                continue;
            }

            var keyValuePair = $"{entry.Key}={entry.Value}";
            if (!keyValuePair.Equals(Mask(keyValuePair)))
            {
                newDict.Add(entry.Key, CoreSettings.GetLogMaskValue());
                continue;
            }

            newDict.Add(entry.Key, entry.Value);
        }

        return newDict;
    }


    public List<string> MaskHeaderData(IMessage message)
    {
        if (message.GetHeaderData == null || message.GetHeaderData().Count == 0) return [];

        return message.GetHeaderData().Select(Mask).ToList();
    }
}