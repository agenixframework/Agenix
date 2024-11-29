using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// Builds a wait condition specifically designed for HTTP checks, allowing for customization
/// of HTTP-related parameters such as URL, timeout, status, and method for defining conditions
/// that involve HTTP requests.
public class WaitHttpConditionBuilder : WaitConditionBuilder<HttpCondition, WaitHttpConditionBuilder>
{
    /// Constructs a WaitHttpConditionBuilder with a specified builder.
    /// @param builder the builder used to initialize the WaitHttpConditionBuilder
    /// /
    public WaitHttpConditionBuilder(Wait.Builder<HttpCondition> builder) : base(builder)
    {
    }

    /// Specifies the URL to be used in the HTTP condition.
    /// @param requestUrl The URL to set for the HTTP condition.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Url(string requestUrl)
    {
        GetCondition().Url = requestUrl;
        return this;
    }

    /// Sets the HTTP connection timeout.
    /// @param timeout The timeout duration to be set for the HTTP connection as a string.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Timeout(string timeout)
    {
        GetCondition().Timeout = timeout;
        return this;
    }

    /// Sets the HTTP connection timeout using a long value.
    /// @param timeout The timeout value in milliseconds to be set for the HTTP connection.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Timeout(long timeout)
    {
        GetCondition().Timeout = timeout.ToString();
        return this;
    }

    /// Sets the HTTP status code to check.
    /// @param status The HTTP status code to be set in the condition.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Status(int status)
    {
        GetCondition().HttpResponseCode = status.ToString();
        return this;
    }

    /// Sets the URL for the HTTP request condition.
    /// @param requestUrl The URL to be set in the HTTP condition.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Method(string method)
    {
        GetCondition().Method = method.ToUpper();
        return this;
    }
}