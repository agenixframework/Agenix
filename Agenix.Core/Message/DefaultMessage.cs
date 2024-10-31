using System;
using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;

namespace Agenix.Core.Message;

/// <summary>
///     Default message implementation holds message payload and message headers. Also provides access methods for special
///     header elements such as unique message id and creation timestamp.
/// </summary>
[Serializable]
public class DefaultMessage : IMessage
{
    /// <summary>
    ///     Optional list of header data
    /// </summary>
    private readonly List<string> _headerData = new();

    /// <summary>
    ///     Message headers
    /// </summary>
    private readonly Dictionary<string, object> _headers = new();

    /// <summary>
    ///     The message name for internal use
    /// </summary>
    private string _name;

    /// <summary>
    ///     Message payload object
    /// </summary>
    private object _payload;

    /// <summary>
    ///     Type of the message indicates the content type
    /// </summary>
    private string _type;

    public DefaultMessage() : this("")
    {
    }

    /// <summary>
    ///     Constructs copy of given message.
    /// </summary>
    /// <param name="message">the message obj</param>
    public DefaultMessage(IMessage message) : this(message, message.GetHeaders())
    {
        Name = message.Name;
        SetType(message.GetType());
        _headerData.AddRange(message.GetHeaderData());
    }

    /// <summary>
    ///     Default constructor using just message payload.
    /// </summary>
    /// <param name="payload">the payload obj</param>
    public DefaultMessage(object payload) : this(payload, new Dictionary<string, object>())
    {
    }

    /// <summary>
    ///     Default constructor using payload and headers.
    /// </summary>
    /// <param name="payload">the payload obj</param>
    /// <param name="headers">the map of headers</param>
    /// <param name="forceAgenixHeaderUpdate">if false the headers are not updated, otherwise updated</param>
    public DefaultMessage(object payload, Dictionary<string, object> headers, bool forceAgenixHeaderUpdate = false)
    {
        _payload = payload;
        _headers = _headers.Merge(headers);

        if (forceAgenixHeaderUpdate)
        {
            _headers.Add(MessageHeaders.Id, Guid.NewGuid().ToString());
            _headers.Add(MessageHeaders.Timestamp, Environment.TickCount);
        }
        else
        {
            if (!_headers.ContainsKey(MessageHeaders.Id)) _headers.Add(MessageHeaders.Id, Guid.NewGuid().ToString());

            if (!_headers.ContainsKey(MessageHeaders.Timestamp))
                _headers.Add(MessageHeaders.Timestamp, Environment.TickCount);
        }
    }

    public string Id => GetHeader(MessageHeaders.Id).ToString();

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public object Payload
    {
        get => _payload;
        set => _payload = value;
    }

    public object GetHeader(string headerName)
    {
        return _headers[headerName];
    }

    public IMessage SetHeader(string headerName, object headerValue)
    {
        if (headerName.Equals(MessageHeaders.Id))
            throw new CoreSystemException("Not allowed to set reserved message header from message: " +
                                          MessageHeaders.Id);
        _headers.Add(headerName, headerValue);
        return this;
    }

    public void RemoveHeader(string headerName)
    {
        if (headerName.Equals(MessageHeaders.Id))
            throw new CoreSystemException("Not allowed to remove reserved message header from message: " +
                                          MessageHeaders.Id);

        _headers.Remove(headerName);
    }

    public IMessage AddHeaderData(string headerData)
    {
        _headerData.Add(headerData);
        return this;
    }

    public List<string> GetHeaderData()
    {
        return _headerData;
    }

    public Dictionary<string, object> GetHeaders()
    {
        return _headers;
    }

    public TX GetPayload<TX>()
    {
        return TypeConversionUtils.ConvertIfNecessary<TX>(Payload, typeof(TX));
    }

    public new string GetType()
    {
        if (_type != null) return _type;
        if (MessageUtils.HasJsonPayload(this))
            _type = MessageType.JSON.ToString();
        else if (MessageUtils.HasXmlPayload(this))
            _type = MessageType.XML.ToString();
        else if (Payload is string)
            _type = MessageType.PLAINTEXT.ToString();
        else
            _type = MessageType.UNSPECIFIED.ToString();

        return _type;
    }


    public IMessage SetType(string type)
    {
        if (type != null) _headers.Add(MessageHeaders.MessageType, type);

        _type = type;
        return this;
    }

    public DefaultMessage SetType(MessageType type)
    {
        _type = type.ToString();
        return this;
    }

    public long GetTimestamp()
    {
        return (long)GetHeader(MessageHeaders.Timestamp);
    }

    public override string ToString()
    {
        if (_headerData == null || _headerData.Count == 0)
            return TypeDescriptor.GetClassName(typeof(DefaultMessage))?.ToUpper() + " [id: " + Id + ", payload: " +
                   GetPayload<string>() + "][headers: " + string.Join(",", _headers) + "]";
        return TypeDescriptor.GetClassName(typeof(DefaultMessage))?.ToUpper() + " [id: " + Id + ", payload: " +
               GetPayload<string>() + "][headers: " + string.Join(",", _headers) + "][header-data: " +
               string.Join(",", _headerData);
    }
}