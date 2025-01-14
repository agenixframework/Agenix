using Agenix.Core.Message;

namespace Agenix.Http.Message;

/// Utility class for handling operations related to HTTP messages.
/// Provides methods to copy settings from one message to another,
/// ensuring compatibility and preserving headers and properties.
/// /
public static class HttpMessageUtils
{
    /// Copies settings from a source message to a target HTTP message, converting if necessary.
    /// @param from The source message, which can be either an IMessage or HttpMessage.
    /// @param to The target HTTP message to which settings will be applied.
    /// /
    public static void Copy(IMessage from, HttpMessage to)
    {
        HttpMessage source;
        if (from is HttpMessage httpMessage)
            source = httpMessage;
        else
            source = new HttpMessage(from);

        Copy(source, to);
    }

    /// Copies the properties and headers from one HttpMessage instance to another.
    /// @param from the source HttpMessage from which properties are to be copied
    /// @param to the target HttpMessage to which properties are to be copied
    /// /
    public static void Copy(HttpMessage from, HttpMessage to)
    {
        to.Name = from.Name;
        to.SetType(from.GetType());
        to.Payload = from.Payload;

        foreach (var entry in from.GetHeaders().Where(entry =>
                     !entry.Key.Equals(MessageHeaders.Id) && !entry.Key.Equals(MessageHeaders.Timestamp)))
            to.Header(entry.Key, entry.Value);

        foreach (var headerData in from.GetHeaderData()) to.AddHeaderData(headerData);

        foreach (var cookie in from.GetCookies()) to.Cookie(cookie);
    }
}