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


using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Api.Xml;
using Agenix.Core.Xml;
using Agenix.Validation.Xml.Message.Builder;
using Moq;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Message.Builder;

public class MarshallingHeaderDataBuilderTest : AbstractNUnitSetUp
{
    private readonly Xml2Marshaller _marshaller = new(typeof(TestRequest));
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly TestRequest _request = new("Hello Agenix!");

    [SetUp]
    public void SetUp()
    {
        _marshaller.SetProperty("omitxmldeclaration", "true");
        _marshaller.SetProperty("stripnamespaces", "true");
        _marshaller.SetProperty("removeemptylines", "true");
    }

    [Test]
    public void ShouldBuildHeaderData()
    {
        // Arrange
        var marshallerMap = new Dictionary<string, IMarshaller> { ["marshaller"] = _marshaller };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        _referenceResolver.Setup(r => r.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildHeaderDataWithMapper()
    {
        // Arrange
        var builder = new MarshallingHeaderDataBuilder(_request, _marshaller);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildHeaderDataWithMapperName()
    {
        // Arrange
        _referenceResolver.Setup(r => r.IsResolvable("marshaller")).Returns(true);
        _referenceResolver.Setup(r => r.Resolve<IMarshaller>("marshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request, "marshaller");

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldBuildHeaderDataWithVariableSupport()
    {
        // Arrange
        Context.SetVariable("message", "Hello Agenix!");
        var builder = new MarshallingHeaderDataBuilder(new TestRequest("${message}"), _marshaller);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void ShouldThrowExceptionWhenMarshallerNotFound()
    {
        // Arrange
        _referenceResolver.Setup(r => r.IsResolvable("nonExistentMarshaller")).Returns(false);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request, "nonExistentMarshaller");

        // Act & Assert
        Assert.That(() => builder.BuildHeaderData(Context),
            Throws.TypeOf<AgenixSystemException>()
                .With.Message.EqualTo("Unable to find proper object marshaller for name 'nonExistentMarshaller'"));
    }

    [Test]
    public void ShouldThrowExceptionWhenMultipleMarshallersFound()
    {
        // Arrange
        var marshallerMap = new Dictionary<string, IMarshaller>
        {
            ["marshaller1"] = _marshaller, ["marshaller2"] = new Xml2Marshaller(typeof(TestRequest))
        };

        _referenceResolver.Setup(r => r.ResolveAll<IMarshaller>()).Returns(marshallerMap);
        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new MarshallingHeaderDataBuilder(_request);

        // Act & Assert
        Assert.That(() => builder.BuildHeaderData(Context),
            Throws.TypeOf<AgenixSystemException>()
                .With.Message.Contains("Unable to auto detect object marshaller")
                .And.Message.Contains("found 2 matching marshaller instances"));
    }

    [Test]
    public void ShouldReturnSuperResultWhenPayloadIsNull()
    {
        // Arrange
        var builder = new MarshallingHeaderDataBuilder(null);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldReturnSuperResultWhenPayloadIsString()
    {
        // Arrange
        var builder = new MarshallingHeaderDataBuilder("simple string");

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        Assert.That(result, Is.EqualTo("simple string"));
    }
}
