using System.Net;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Http.Actions;
using Agenix.Http.Message;
using NUnit.Framework;
using static Moq.Mock;
using static System.Net.HttpStatusCode;

namespace Agenix.Core.NUnitTestProject.Http.Actions;

public class HttpClientResponseActionBuilderTest
{
    private HttpMessage _httpMessageMock;
    private IMessageBuilder _messageBuilderMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _httpMessageMock = Of<HttpMessage>();
        _messageBuilderMock = Of<IMessageBuilder>();
    }

    [Test]
    public void StatusFromHttpStatus()
    {
        new HttpClientResponseActionBuilder(_messageBuilderMock, _httpMessageMock)
            .Message()
            .Status(OK); // Method under test

        Get(_httpMessageMock).Verify(m => m.Status(OK));
    }

    [Test]
    public void StatusFromHttpStatusCode()
    {
        var httpStatusCode = (HttpStatusCode)123;

        new HttpClientResponseActionBuilder(_messageBuilderMock, _httpMessageMock)
            .Message()
            .Status(httpStatusCode); // Method under test

        Get(_httpMessageMock).Verify(m => m.Status(httpStatusCode));
    }
}