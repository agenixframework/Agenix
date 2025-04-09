#region Imports

using System;
using System.IO;
using System.Text;
using System.Xml;
using Agenix.Core.Util;

#endregion

namespace Agenix.Core.IO
{
    /// <summary>
    /// Used when retrieving information from the standard .NET configuration
    /// files (App.config / Web.config).
    /// </summary>
    /// <remarks>
    /// <p>
    /// If created with the name of a configuration section, then all methods
    /// aside from the description return <see langword="null"/>,
    /// <see langword="false"/>, or throw an exception. If created with an
    /// <see cref="System.Xml.XmlElement"/>, then the
    /// <see cref="InputStream"/> property
    /// will return a corresponding <see cref="System.IO.Stream"/> to parse.
    /// </p>
    /// </remarks>
    public class ConfigSectionResource : AbstractResource
    {
        private readonly XmlElement _configElement;
        private readonly string _sectionName;

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates new instance of the
        /// <see cref="ConfigSectionResource"/> class.
        /// </summary>
        /// <param name="configSection">
        /// The actual XML configuration section.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="configSection"/> is <see langword="null"/>.
        /// </exception>
        public ConfigSectionResource(XmlElement configSection)
        {
            AssertUtils.ArgumentNotNull(configSection, "configSection");
            _sectionName = configSection.Name;
            _configElement = configSection;
        }

        /// <summary>
        /// Creates new instance of the
        /// <see cref="ConfigSectionResource"/> class.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the configuration section.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="resourceName"/> is
        /// <see langword="null"/> or contains only whitespace character(s).
        /// </exception>
        public ConfigSectionResource(string resourceName) : base(resourceName)
        {
            AssertUtils.ArgumentHasText(resourceName, "resourceName");
            _sectionName = GetResourceNameWithoutProtocol(resourceName);
            _configElement = (XmlElement) ConfigurationUtils.GetSection(_sectionName);
        }

        #endregion

        #region IResource Members

        /// <summary>
        /// Returns the <see cref="System.Uri"/> handle for this resource.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation always returns <see langword="null"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="null"/>.
        /// </value>
        /// <seealso cref="System.Uri"/>
        public override Uri Uri => null;

        /// <summary>
        /// Returns a <see cref="System.IO.FileInfo"/> handle for this resource.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation always returns <see langword="null"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="null"/>.
        /// </value>
        /// <seealso cref="Agenix.Core.IO.IResource.File"/>
        public override FileInfo File => null;

        /// <summary>
        /// Returns a description for this resource (the name of the
        /// configuration section in this case).
        /// </summary>
        /// <value>
        /// A description for this resource.
        /// </value>
        /// <seealso cref="NHamcrest.Core.Description"/>
        public override string Description => $"config [{ConfigurationUtils.GetFileName(_configElement)}#{_sectionName}]";

        /// <summary>
        /// Does this resource actually exist in physical form?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation always returns <see langword="false"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="false"/>
        /// </value>
        /// <seealso cref="Agenix.Core.IO.IResource.Exists"/>
        /// <seealso cref="Agenix.Core.IO.IResource.File"/>
        public override bool Exists => false;

        #endregion

        #region IInputStreamSource Members

        /// <summary>
        /// Return an <see cref="System.IO.Stream"/> for this resource.
        /// </summary>
        /// <value>
        /// An <see cref="System.IO.Stream"/>.
        /// </value>
        /// <exception cref="System.IO.IOException">
        /// If the stream could not be opened.
        /// </exception>
        /// <seealso cref="IInputStreamSource"/>
        public override Stream InputStream
        {
            get
            {
                if (_configElement == null)
                {
                    throw new FileNotFoundException($"Configuration Section '{_sectionName}' does not exist", _sectionName);
                }
                return new MemoryStream(Encoding.UTF8.GetBytes(_configElement.OuterXml));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Exposes the actual <see cref="System.Xml.XmlElement"/> for the
        /// configuration section.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Introduced to accomodate line info tracking during parsing.
        /// </p>
        /// </remarks>
        internal XmlElement ConfigElement => _configElement;

        #endregion
    }
}
