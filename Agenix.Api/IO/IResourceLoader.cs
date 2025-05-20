namespace Agenix.Api.IO
{
    /// <summary>
    /// Describes an object that can load
    /// <see cref="IResource"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The <see cref="ConfigurableResourceLoader"/> class is a
    /// standalone implementation that is usable outside an
    /// <see cref="Agenix.Context.IApplicationContext"/>; the aforementioned
    /// class is also used by the
    /// <see cref="Agenix.Core.IO.ResourceConverter"/> class.
    /// </p>
    /// </remarks>
    public interface IResourceLoader
    {
    	/// <summary>
    	/// Return an <see cref="IResource"/> handle for the
    	/// specified resource.
    	/// </summary>
    	/// <remarks>
    	/// <p>
    	/// The handle should always be a reusable resource descriptor; this
    	/// allows one to make repeated calls to the underlying
    	/// <see cref="Agenix.Core.IO.IInputStreamSource.InputStream"/>.
    	/// </p>
    	/// <p>
    	/// <ul>
    	/// <li>
    	/// <b>Must</b> support fully qualified URLs, e.g. "file:C:/test.dat".
    	/// </li>
    	/// <li>
    	/// Should support relative file paths, e.g. "test.dat" (this will be
    	/// implementation-specific, typically provided by an
    	/// </li>
    	/// </ul>
    	/// </p>
    	/// <note>
    	/// An <see cref="IResource"/> handle does not imply an
    	/// existing resource; you need to check the value of an
    	/// <see cref="IResource"/>'s
    	/// <see cref="IResource.Exists"/> property to determine
    	/// conclusively whether or not the resource actually exists.
    	/// </note>
    	/// </remarks>
    	/// <param name="location">The resource location.</param>
    	/// <returns>
    	/// An appropriate <see cref="IResource"/> handle.
    	/// </returns>
    	IResource GetResource(string location);
    }
}
