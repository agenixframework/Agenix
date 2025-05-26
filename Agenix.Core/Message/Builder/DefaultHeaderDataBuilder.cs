using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Util;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Provides functionality to build and manage message header data.
/// </summary>
public class DefaultHeaderDataBuilder : IMessageHeaderDataBuilder
{
    private readonly object _headerData;

    /// Default constructor for initializing the header data.
    /// @param headerData The data representing the header fragment.
    /// /
    public DefaultHeaderDataBuilder(object headerData)
    {
        _headerData = headerData;
    }

    /// Builds header data by replacing dynamic content in the header data string.
    /// @param context The context used to replace dynamic content in the header data string.
    /// @return A string with dynamic content replaced, or an empty string if header data is null.
    /// /
    public virtual string BuildHeaderData(TestContext context)
    {
        return _headerData == null
            ? ""
            : context.ReplaceDynamicContentInString(_headerData is string
                ? _headerData.ToString()
                : TypeConversionUtils.ConvertIfNecessary<string>(_headerData, typeof(string)));
    }

    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }

    /// Retrieves the header data.
    /// @return The header data object.
    /// /
    public object GetHeaderData()
    {
        return _headerData;
    }
}