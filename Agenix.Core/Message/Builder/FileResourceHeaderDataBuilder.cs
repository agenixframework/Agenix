using System.Collections.Generic;
using System.IO;
using System.Text;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Class responsible for building header data from a file resource.
/// </summary>
public class FileResourceHeaderDataBuilder : IMessageHeaderDataBuilder
{
    private readonly string _charsetName;
    private readonly string _resourcePath;

    /// <summary>
    ///     Constructor using file resource path and default charset.
    /// </summary>
    /// <param name="resourcePath"></param>
    public FileResourceHeaderDataBuilder(string resourcePath)
    {
        _resourcePath = resourcePath;
        _charsetName = CoreSettings.AgenixFileEncoding;
    }

    /// <summary>
    ///     Constructor using file resource path and charset.
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <param name="charsetName"></param>
    public FileResourceHeaderDataBuilder(string resourcePath, string charsetName)
    {
        _resourcePath = resourcePath;
        _charsetName = charsetName;
    }

    /// <summary>
    ///     Builds header data by reading and processing the file content based on the provided context.
    /// </summary>
    /// <param name="context">The TestContext object that contains necessary information for processing.</param>
    /// <returns>A string representing the processed header data.</returns>
    public string BuildHeaderData(TestContext context)
    {
        try
        {
            return context.ReplaceDynamicContentInString(FileUtils.ReadToString(
                FileUtils.GetFileResource(_resourcePath, context),
                Encoding.GetEncoding(context.ResolveDynamicValue(_charsetName))));
        }
        catch (IOException e)
        {
            throw new CoreSystemException("Failed to read message header data resource", e);
        }
    }

    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }
}