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


using System.Text;
using System.Xml;

namespace Agenix.Validation.Xml;

/// <summary>
/// Provides functionality to create instances of <see cref="XmlWriter"/> objects configured with optional settings.
/// </summary>
/// <remarks>
/// The <see cref="XmlWriterFactory"/> class allows creating <see cref="XmlWriter"/> objects for various output targets,
/// such as <see cref="StringBuilder"/>, <see cref="Stream"/>, and <see cref="TextWriter"/>. An optional
/// <see cref="XmlConfigurer"/> can be supplied to customize the configuration of the created <see cref="XmlWriter"/> objects.
/// </remarks>
public class XmlWriterFactory(XmlConfigurer? configurer = null)
{
    public XmlWriter CreateWriter(Stream output)
    {
        return configurer?.CreateXmlWriter(output) ?? XmlWriter.Create(output);
    }

    public XmlWriter CreateWriter(TextWriter output)
    {
        return configurer?.CreateXmlWriter(output) ?? XmlWriter.Create(output);
    }

    public XmlWriter CreateWriter(StringBuilder output)
    {
        return configurer?.CreateXmlWriter(output) ?? XmlWriter.Create(output);
    }
}

