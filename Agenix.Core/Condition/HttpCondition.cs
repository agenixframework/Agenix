using System;
using System.Net;
using Agenix.Core.Exceptions;
using log4net;

namespace Agenix.Core.Condition;

/// <summary>
///     Tests if a HTTP Endpoint is reachable. The test is successful if the endpoint responds with the expected
///     responsecode. By default a HTTP 200 response code is expected.
/// </summary>
public class HttpCondition() : AbstractCondition("http-check")
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(HttpCondition));

    /// <summary>
    ///     Represents the expected HTTP response code used to validate
    ///     if an HTTP request returns the correct status.
    /// </summary>
    public string HttpResponseCode { get; set; } = "200";

    /// <summary>
    ///     The HTTP method used to make the web request, e.g., GET, POST, HEAD, etc.
    /// </summary>
    public string Method { get; set; } = "HEAD";

    /// <summary>
    ///     URL to be checked as part of the HTTP condition.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Specifies the timeout duration, in milliseconds, for the HTTP request.
    /// </summary>
    public string Timeout { get; set; } = "1000";

    /// Evaluates whether the HTTP condition is satisfied based on the test context.
    /// Compares the HTTP response code with the expected value obtained by invoking a URL within the given test context.
    /// <param name="context">The test context containing necessary parameters for the HTTP condition evaluation.</param>
    /// <return>
    ///     True if the HTTP response code matches the expected value, otherwise false.
    /// </return>
    /// /
    public override bool IsSatisfied(TestContext context)
    {
        return GetHttpResponseCode(context) == InvokeUrl(context);
    }

    /// Constructs a success message for the HTTP condition, indicating that the request
    /// to a specific URL returned the expected status code.
    /// <param name="context">The test context containing execution details to retrieve URL and response code information.</param>
    /// <return>A formatted string conveying the success message for the HTTP condition.</return>
    /// /
    public override string GetSuccessMessage(TestContext context)
    {
        return string.Format("Http condition success - request url '{0}' did return expected status '{1}'",
            GetUrl(context), GetHttpResponseCode(context));
    }

    /// Generates an error message if the HTTP condition check fails.
    /// <param name="context">The test context containing details of the HTTP request.</param>
    /// <return>A string describing the error encountered during the HTTP condition check.</return>
    /// /
    public override string GetErrorMessage(TestContext context)
    {
        return string.Format("Failed to check Http condition - request url '{0}' did not return expected status '{1}'",
            GetUrl(context), GetHttpResponseCode(context));
    }

    /// Invokes an HTTP request using the specified context and returns the HTTP response code.
    /// <param name="context">The context that provides configuration and settings for the HTTP request</param>
    /// <return>The HTTP response code indicating the result of the request</return>
    /// /
    private int InvokeUrl(TestContext context)
    {
        var contextUrl = GetUrl(context);
        if (Log.IsDebugEnabled) Log.Debug($"Probing Http request url '{contextUrl.ToString()}'");

        var responseCode = -1;

        try
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(contextUrl);
            httpWebRequest.Timeout = GetTimeout(context);
            httpWebRequest.Method = context.ResolveDynamicValue(Method);

            using var response = (HttpWebResponse)httpWebRequest.GetResponse();
            responseCode = (int)response.StatusCode;
        }
        catch (WebException e)
        {
            if (e.Response != null) responseCode = (int)((HttpWebResponse)e.Response).StatusCode;
            Log.Warn($"Could not access Http url '{contextUrl.ToString()}' - {e.Message}");
        }

        return responseCode;
    }

    /// Gets the request URL with test variable support.
    /// <param name="context">The test context to get the URL from.</param>
    /// <return>The extracted URL.</return>
    /// /
    private Uri GetUrl(TestContext context)
    {
        try
        {
            return new Uri(context.ReplaceDynamicContentInString(Url));
        }
        catch (UriFormatException e)
        {
            throw new CoreSystemException("Invalid request url", e);
        }
    }

    /// Retrieves the timeout value in milliseconds from the provided test context.
    /// <param name="context">The test context from which to extract the timeout.</param>
    /// <return>The extracted timeout as an integer.</return>
    /// /
    private int GetTimeout(TestContext context)
    {
        return int.Parse(context.ResolveDynamicValue(Timeout));
    }

    /// Gets the expected Http response code.
    /// <param name="context">The test context to get the response code from</param>
    /// <return>The extracted response code</return>
    /// /
    private int GetHttpResponseCode(TestContext context)
    {
        return int.Parse(context.ResolveDynamicValue(HttpResponseCode));
    }


    public override string ToString()
    {
        return "HttpCondition{" +
               "url='" + Url + '\'' +
               ", timeout='" + Timeout + '\'' +
               ", httpResponseCode='" + HttpResponseCode + '\'' +
               ", method='" + Method + '\'' +
               ", name='" + GetName() + '\'' +
               '}';
    }
}