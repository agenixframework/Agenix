using System.IO;
using System.Text;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using Agenix.Core.Util;

namespace Agenix.Core.Message.Builder;

/// FileResourcePayloadBuilder class is responsible for constructing
/// message payloads using file resources or file paths. It implements
/// IMessagePayloadBuilder and IMessageTypeAware interfaces.
public class FileResourcePayloadBuilder : IMessagePayloadBuilder, IMessageTypeAware
{
    private readonly string _charsetName;
    private readonly IResource _resource;
    private readonly string _resourcePath;
    private string _messageType;

    /// <summary>
    ///     Class responsible for constructing message payloads using file resources or file paths.
    ///     Implements IMessagePayloadBuilder and IMessageTypeAware interfaces.
    /// </summary>
    public FileResourcePayloadBuilder(IResource resource)
        : this(resource, CoreSettings.AgenixFileEncoding)
    {
    }

    /// <summary>
    ///     Class responsible for constructing message payloads using file resources or file paths.
    ///     Implements IMessagePayloadBuilder and IMessageTypeAware interfaces.
    /// </summary>
    public FileResourcePayloadBuilder(IResource resource, string charset)
    {
        _charsetName = charset;
        _resourcePath = null;
        _resource = resource;
    }

    /// <summary>
    ///     Class responsible for constructing message payloads using file resources or file paths.
    ///     Implements IMessagePayloadBuilder and IMessageTypeAware interfaces.
    /// </summary>
    public FileResourcePayloadBuilder(string resourcePath)
        : this(resourcePath, CoreSettings.AgenixFileEncoding)
    {
    }

    /// <summary>
    ///     Class responsible for constructing message payloads using file resources or file paths.
    ///     Implements IMessagePayloadBuilder and IMessageTypeAware interfaces.
    /// </summary>
    public FileResourcePayloadBuilder(string resourcePath, string charset)
    {
        _charsetName = charset;
        _resourcePath = resourcePath;
        _resource = null;
    }

    /// <summary>
    ///     Builds the payload based on the provided context.
    /// </summary>
    /// <param name="context">The test context that provides dynamic value resolution.</param>
    /// <return>Returns the constructed payload as an object.</return>
    public object BuildPayload(TestContext context)
    {
        return _resource != null ? BuildFromResource(context) : BuildFromResourcePath(context);
    }

    /// <summary>
    ///     Sets the type of the message.
    /// </summary>
    /// <param name="messageType">The type of the message to be set.</param>
    public void SetMessageType(string messageType)
    {
        _messageType = messageType;
    }

    /// <summary>
    ///     Builds the payload from the provided resource.
    /// </summary>
    /// <param name="context">The test context that provides dynamic value resolution.</param>
    /// <return>The payload built from the resource as an object.</return>
    private object BuildFromResource(TestContext context)
    {
        if (MessageTypeExtensions.IsBinary(_messageType)) return _resource;

        return context.ReplaceDynamicContentInString(GetFileResourceContent(_resource, context));
    }

    /// <summary>
    ///     Builds the payload from the path of the file resource.
    /// </summary>
    /// <param name="context">The test context that provides dynamic value resolution.</param>
    /// <return>The payload built from the file resource as an object.</return>
    private object BuildFromResourcePath(TestContext context)
    {
        return _resourcePath == null
            ? ""
            : MessageTypeExtensions.IsBinary(_messageType)
                ? FileUtils.GetFileResource(_resourcePath, context)
                : context.ReplaceDynamicContentInString(GetFileResourceContent(_resourcePath, context));
    }

    /// <summary>
    ///     Retrieves the content of the file resource specified by the path.
    /// </summary>
    /// <param name="path">The path to the file resource.</param>
    /// <param name="context">The test context that provides dynamic value resolution.</param>
    /// <return>The content of the file resource as a string.</return>
    private string GetFileResourceContent(string path, TestContext context)
    {
        var fileResource = FileUtils.GetFileResource(path, context);
        return GetFileResourceContent(fileResource, context);
    }

    /// <summary>
    ///     Retrieves the content of the file resource as a string.
    /// </summary>
    /// <param name="fileResource">The file resource to be read.</param>
    /// <param name="context">The test context that provides dynamic value resolution.</param>
    /// <return>The content of the file resource as a string.</return>
    private string GetFileResourceContent(IResource fileResource, TestContext context)
    {
        try
        {
            var charset = Encoding.GetEncoding(context.ResolveDynamicValue(_charsetName));
            return FileUtils.ReadToString(fileResource, charset);
        }
        catch (IOException e)
        {
            throw new CoreSystemException("Failed to build message payload from file resource", e);
        }
    }
}