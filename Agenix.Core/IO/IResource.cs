#region Imports

using System;
using System.IO;
using System.Text;

#endregion

namespace Agenix.Core.IO
{
	/// <summary>
    /// The central abstraction for Agenix access to resources such as
    /// <see cref="System.IO.Stream"/>s.
	/// </summary>
	/// <p>
	/// This interface encapsulates a resource descriptor that abstracts away
	/// from the underlying type of resource; possible resource types include
	/// files, memory streams, and databases (this list is not exhaustive).
	/// </p>
	/// <p>
	/// A <see cref="System.IO.Stream"/> can definitely be opened and accessed
	/// for every such resource; if the resource exists in a physical form (for
	/// example, the resource is not an in-memory stream or one that has been
	/// extracted from an assembly or ZIP file), a <see cref="System.Uri"/> or
	/// <see cref="System.IO.FileInfo"/> can also be accessed. The actual
	/// behavior is implementation-specific.
	/// </p>
	/// <p>
	/// This interface, when used in tandem with the
	/// <see cref="IResourceLoader"/> interface, forms the backbone of
	/// resource handling. 
	/// </p>
	public interface IResource : IInputStreamSource
	{
		/// <summary>
		/// Does this resource represent a handle with an open stream?
		/// </summary>
		/// <remarks>
		/// <p>
		/// If <see langword="true"/>, the <see cref="System.IO.Stream"/>
		/// cannot be read multiple times, and must be read and then closed to
		/// avoid resource leaks.
		/// </p>
		/// <p>
		/// Will be <see langword="false"/> for all usual resource descriptors.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this resource represents a handle with an
		/// open stream.
		/// </value>
		/// <seealso cref="IInputStreamSource.InputStream"/>
		bool IsOpen { get; }

		/// <summary>
		/// Returns the <see cref="System.Uri"/> handle for this resource.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For safety, always check the value of the
		/// <see cref="Exists"/> property prior to
		/// accessing this property; resources that cannot be exposed as 
		/// a <see cref="System.Uri"/> will typically return
		/// <see langword="false"/> from a call to the
		/// <see cref="Exists"/> property.
		/// </p>
		/// </remarks>
		/// <value>
		/// The <see cref="System.Uri"/> handle for this resource.
		/// </value>
		/// <exception cref="System.IO.IOException">
		/// If the resource is not available or cannot be exposed as a
		/// <see cref="System.Uri"/>.
		/// </exception>
		/// <seealso cref="IResource"/>
		/// <seealso cref="Exists"/>
		Uri Uri { get; }

		/// <summary>
		/// Returns a <see cref="System.IO.FileInfo"/> handle for this resource.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For safety, always check the value of the
		/// <see cref="Exists"/> property prior to
		/// accessing this property; resources that cannot be exposed as 
		/// a <see cref="System.IO.FileInfo"/> will typically return
		/// <see langword="false"/> from a call to the
		/// <see cref="Exists"/> property.
		/// </p>
		/// </remarks>
		/// <value>
		/// The <see cref="System.IO.FileInfo"/> handle for this resource.
		/// </value>
		/// <exception cref="System.IO.IOException">
		/// If the resource is not available on a filesystem, or cannot be
		/// exposed as a <see cref="System.IO.FileInfo"/> handle.
		/// </exception>
		/// <seealso cref="IResource"/>
		/// <seealso cref="Exists"/>
		FileInfo File { get; }

		/// <summary>
		/// Returns a description for this resource.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The description is typically used for diagnostics and other such
		/// logging when working with the resource.
		/// </p>
		/// <p>
		/// Implementations are also encouraged to return this value from their
		/// <see cref="System.Object.ToString()"/> method.
		/// </p>
		/// </remarks>
		/// <value>
		/// A description for this resource.
		/// </value>
		string Description { get; }

		/// <summary>
		/// Does this resource actually exist in physical form?
		/// </summary>
		/// <remarks>
		/// <p>
		/// An example of a resource that physically exists would be a
		/// file on a local filesystem. An example of a resource that does not
		/// physically exist would be an in-memory stream.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this resource actually exists in physical
		/// form (for example on a filesystem).
		/// </value>
		/// <seealso cref="File"/>
		/// <seealso cref="Uri"/>
		bool Exists { get; }

		/// <summary>
		/// Creates a resource relative to this resource.
		/// </summary>
		/// <param name="relativePath">
		/// The path (always resolved as relative to this resource).
		/// </param>
		/// <returns>
		/// The relative resource.
		/// </returns>
		/// <exception cref="System.IO.IOException">
		/// If the relative resource could not be created from the supplied
		/// path.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// If the resource does not support the notion of a relative path.
		/// </exception>
		IResource CreateRelative(string relativePath);
		/*
		 * Returns a Reader that reads from the underlying resource using UTF-8 as charset.
		 */
		/// <summary>
		/// Returns a <see cref="System.IO.TextReader"/> that reads from the underlying resource using UTF-8 encoding.
		/// </summary>
		/// <returns>
		/// A <see cref="System.IO.TextReader"/> instance for reading the resource's content using UTF-8 encoding.
		/// </returns>
		/// <exception cref="System.IO.IOException">
		/// If an error occurs while accessing the resource.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// If the resource cannot provide a reader.
		/// </exception>
		TextReader GetReader()
		{
			return GetReader(Encoding.GetEncoding(CoreSettings.AgenixFileEncoding()));
		}

		/*
		 * Returns a Reader that reads from the underlying resource using the given Charset.
		 */
		/// <summary>
		/// Returns a <see cref="System.IO.TextReader"/> to read from the underlying resource using the specified character encoding.
		/// </summary>
		/// <param name="charset">
		/// The character encoding to use when creating the reader.
		/// </param>
		/// <returns>
		/// A text reader for the resource.
		/// </returns>
		/// <exception cref="System.IO.IOException">
		/// If an I/O error occurs while creating the reader.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the provided encoding is null.
		/// </exception>
		TextReader GetReader(Encoding charset)
		{
			return new StreamReader(InputStream, charset);
		}
	}
}
