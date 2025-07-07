using System.Net;
using System.Text;

namespace Agenix.Azure.Security.Tests.Handler;

/// <summary>
/// Test HTTP message handler for testing HTTP clients and handlers
/// </summary>
public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode StatusCode, string Content)> _responses = new();
    private HttpStatusCode _defaultStatusCode = HttpStatusCode.OK;
    private string _defaultContent = string.Empty;
    private Exception? _exception;

    public int RequestCount { get; private set; }
    public List<HttpRequestMessage> Requests { get; } = new();
    public HttpRequestMessage? LastRequest => Requests.LastOrDefault();

    /// <summary>
    /// Set a single response for all requests
    /// </summary>
    public void SetResponse(HttpStatusCode statusCode, string content)
    {
        _defaultStatusCode = statusCode;
        _defaultContent = content;
        _exception = null;
        _responses.Clear();
    }

    /// <summary>
    /// Set multiple responses in sequence
    /// </summary>
    public void SetResponses(params (HttpStatusCode StatusCode, string Content)[] responses)
    {
        _responses.Clear();
        foreach (var response in responses)
        {
            _responses.Enqueue(response);
        }
        _exception = null;
    }

    /// <summary>
    /// Set an exception to be thrown on the next request
    /// </summary>
    public void SetException(Exception exception)
    {
        _exception = exception;
    }

    /// <summary>
    /// Reset the handler state
    /// </summary>
    public void Reset()
    {
        RequestCount = 0;
        Requests.Clear();
        _responses.Clear();
        _exception = null;
        _defaultStatusCode = HttpStatusCode.OK;
        _defaultContent = string.Empty;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        Requests.Add(request);

        if (_exception != null)
        {
            var ex = _exception;
            _exception = null; // Reset after throwing once
            throw ex;
        }

        await Task.Delay(1, cancellationToken); // Simulate network delay

        // Use queued responses if available, otherwise use default
        var (statusCode, content) = _responses.Count > 0
            ? _responses.Dequeue()
            : (_defaultStatusCode, _defaultContent);

        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Requests.Clear();
            _responses.Clear();
        }
        base.Dispose(disposing);
    }
}
