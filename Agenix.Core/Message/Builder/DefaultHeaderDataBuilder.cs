using System.Collections.Generic;
using Agenix.Core.Util;

namespace Agenix.Core.Message.Builder;

public class DefaultHeaderDataBuilder : IMessageHeaderDataBuilder
{
    private readonly object _headerData;

    /**
     * Default constructor using header fragment data.
     * @param headerData
     */
    public DefaultHeaderDataBuilder(object headerData)
    {
        _headerData = headerData;
    }

    /// Builds header data by replacing dynamic content in the header data string.
    /// @param context The context used to replace dynamic content in the header data string.
    /// @return A string with dynamic content replaced, or an empty string if header data is null.
    /// /
    public string BuildHeaderData(TestContext context)
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