#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion


using System.Net;
using Agenix.Api.Message;
using Agenix.GraphQL.Actions;
using Agenix.GraphQL.Message;
using Moq;

namespace Agenix.GraphQL.Tests.Actions;

public class GraphQLClientResponseActionBuilderTest : AbstractNUnitSetUp
{
    private Mock<GraphQLMessage> _graphQLMessageMock;
    private Mock<IMessageBuilder> _messageBuilderMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _graphQLMessageMock = new Mock<GraphQLMessage>();
        _messageBuilderMock = new Mock<IMessageBuilder>();
    }

    [Test]
    public void StatusFromHttpStatus()
    {
        new GraphQLClientResponseActionBuilder(_messageBuilderMock.Object, _graphQLMessageMock.Object)
            .Message()
            .Status(HttpStatusCode.OK); // Method under test

        _graphQLMessageMock.Verify(x => x.Status(HttpStatusCode.OK), Times.Once);
    }

    [Test]
    public void StatusFromHttpStatusCode()
    {
        var httpStatusCode = (HttpStatusCode)123;

        new GraphQLClientResponseActionBuilder(_messageBuilderMock.Object, _graphQLMessageMock.Object)
            .Message()
            .Status(httpStatusCode); // Method under test

        _graphQLMessageMock.Verify(x => x.Status(httpStatusCode), Times.Once);
    }
}

