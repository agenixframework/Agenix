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


using Agenix.Validation.Xml.Schema;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Xml;

public class XsdSchemaRepositoryTest
{
    [Test]
    public void TestUnknownLocation()
    {
        var schemaRepository = new XsdSchemaRepository();

        schemaRepository.Locations.Add("file:org/agenix/unknown/unknown.xsd");

        schemaRepository.Initialize();
        Assert.That(schemaRepository.GetSchemas().Count, Is.EqualTo(0));
    }

    [Test]
    public void TestWsdlResourceLocation()
    {
        var schemaRepository = new XsdSchemaRepository();

        schemaRepository.Locations.Add(
            "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Validation/TestService.wsdl");

        //schemaRepository.Initialize();

        //Assert.That(schemaRepository.GetSchemas().Count, Is.EqualTo(1));
        //Assert.That(schemaRepository.GetSchemas()[0], Is.InstanceOf<WsdlXsdSchema>());
    }

    [Test]
    public void TestXsdResourceLocation()
    {
        var schemaRepository = new XsdSchemaRepository();

        schemaRepository.Locations.Add(
            "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Validation/test.xsd");

        schemaRepository.Initialize();

        Assert.That(schemaRepository.GetSchemas().Count, Is.EqualTo(1));
        Assert.That(schemaRepository.GetSchemas()[0], Is.InstanceOf<SimpleXsdSchema>());
    }
}
