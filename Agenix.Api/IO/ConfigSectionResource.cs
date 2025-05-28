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

#region Imports

using System.Text;
using System.Xml;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     Used when retrieving information from the standard .NET configuration
///     files (App.config / Web.config).
/// </summary>
/// <remarks>
///     <p>
///         If created with the name of a configuration section, then all methods
///         aside from the description return <see langword="null" />,
///         <see langword="false" />, or throw an exception. If created with an
///         <see cref="System.Xml.XmlElement" />, then the
///         <see cref="InputStream" /> property
///         will return a corresponding <see cref="System.IO.Stream" /> to parse.
///     </p>
/// </remarks>
public class ConfigSectionResource : AbstractResource
{
    private readonly string _sectionName;

    #region IInputStreamSource Members

    /// <summary>
    ///     Return an <see cref="System.IO.Stream" /> for this resource.
    /// </summary>
    /// <value>
    ///     An <see cref="System.IO.Stream" />.
    /// </value>
    /// <exception cref="System.IO.IOException">
    ///     If the stream could not be opened.
    /// </exception>
    /// <seealso cref="IInputStreamSource" />
    public override Stream InputStream
    {
        get
        {
            if (ConfigElement == null)
                throw new FileNotFoundException($"Configuration Section '{_sectionName}' does not exist", _sectionName);
            return new MemoryStream(Encoding.UTF8.GetBytes(ConfigElement.OuterXml));
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Exposes the actual <see cref="System.Xml.XmlElement" /> for the
    ///     configuration section.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         Introduced to accomodate line info tracking during parsing.
    ///     </p>
    /// </remarks>
    internal XmlElement ConfigElement { get; }

    #endregion

    #region Constructor (s) / Destructor

    /// <summary>
    ///     Creates new instance of the
    ///     <see cref="ConfigSectionResource" /> class.
    /// </summary>
    /// <param name="configSection">
    ///     The actual XML configuration section.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="configSection" /> is <see langword="null" />.
    /// </exception>
    public ConfigSectionResource(XmlElement configSection)
    {
        AssertUtils.ArgumentNotNull(configSection, "configSection");
        _sectionName = configSection.Name;
        ConfigElement = configSection;
    }

    /// <summary>
    ///     Creates new instance of the
    ///     <see cref="ConfigSectionResource" /> class.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the configuration section.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="resourceName" /> is
    ///     <see langword="null" /> or contains only whitespace character(s).
    /// </exception>
    public ConfigSectionResource(string resourceName) : base(resourceName)
    {
        AssertUtils.ArgumentHasText(resourceName, "resourceName");
        _sectionName = GetResourceNameWithoutProtocol(resourceName);
        ConfigElement = (XmlElement)ConfigurationUtils.GetSection(_sectionName);
    }

    #endregion

    #region IResource Members

    /// <summary>
    ///     Returns the <see cref="System.Uri" /> handle for this resource.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         This implementation always returns <see langword="null" />.
    ///     </p>
    /// </remarks>
    /// <value>
    ///     <see langword="null" />.
    /// </value>
    /// <seealso cref="System.Uri" />
    public override Uri Uri => null;

    /// <summary>
    ///     Returns a <see cref="System.IO.FileInfo" /> handle for this resource.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         This implementation always returns <see langword="null" />.
    ///     </p>
    /// </remarks>
    /// <value>
    ///     <see langword="null" />.
    /// </value>
    /// <seealso cref="IResource.File" />
    public override FileInfo File => null;

    /// <summary>
    ///     Returns a description for this resource (the name of the
    ///     configuration section in this case).
    /// </summary>
    /// <value>
    ///     A description for this resource.
    /// </value>
    public override string Description => $"config [{ConfigurationUtils.GetFileName(ConfigElement)}#{_sectionName}]";

    /// <summary>
    ///     Does this resource actually exist in physical form?
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         This implementation always returns <see langword="false" />.
    ///     </p>
    /// </remarks>
    /// <value>
    ///     <see langword="false" />
    /// </value>
    /// <seealso cref="IResource.Exists" />
    /// <seealso cref="IResource.File" />
    public override bool Exists => false;

    #endregion
}
