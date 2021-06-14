using System.Collections.Generic;

namespace FleetPay.Core.Message
{
    public interface IMessage
    {
        /// <summary>
        ///     Gets the unique message id;
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Gets/Sets the message name for internal use.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Gets/Sets the message payload.
        /// </summary>
        object Payload { get; set; }

        /// <summary>
        ///     Gets the message header value by its header name.
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        object GetHeader(string headerName);

        /// <summary>
        ///     Sets new header entry in message header list.
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        IMessage SetHeader(string headerName, object headerValue);

        /// <summary>
        ///     Removes the message header if it not a reserved message header such as unique message id.
        /// </summary>
        /// <param name="headerName"></param>
        void RemoveHeader(string headerName);

        /// <summary>
        ///     Adds new header data.
        /// </summary>
        /// <param name="headerData"></param>
        /// <returns></returns>
        IMessage AddHeaderData(string headerData);

        /// <summary>
        ///     Gets the list of header data in this message.
        /// </summary>
        /// <returns></returns>
        List<string> GetHeaderData();

        /// <summary>
        ///     Gets message headers.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> GetHeaders();

        /// <summary>
        ///     Gets message payload with required type conversion.
        /// </summary>
        /// <typeparam name="TX"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
         TX GetPayload<TX>();

        /// <summary>
        ///     Indicates the type of the message content (e.g. Xml, Json, binary)
        /// </summary>
        /// <returns></returns>
        string GetType();

        /// <summary>
        ///     Sets the message type indicating the content type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IMessage SetType(string type);
    }
}