using System;
using System.IO;
using System.Text;

namespace Agenix.Core.Spi;

/*
 * Describe a resource, such as a file or class path resource.
 */
public interface IResource
{
    /*
     * The location of the resource.
     */
    string GetLocation();

    /*
     * Whether this resource exists.
     */
    bool Exists();

    /*
     * The URI of the resource.
     */
    Uri GetURI()
    {
        return new Uri(GetLocation());
    }

    /*
     * The URL for the resource or null if the URL can not be computed.
     */
    Uri GetURL()
    {
        var uri = GetURI();
        return uri != null ? uri : null;
    }

    /*
     * Returns an InputStream that reads from the underlying resource.
     */
    Stream GetInputStream();

    /*
     * Return the file associated with this resource.
     */
    FileInfo GetFile();

    /*
     * Returns a Reader that reads from the underlying resource using UTF-8 as charset.
     */
    TextReader GetReader()
    {
        return GetReader(Encoding.GetEncoding(CoreSettings.AgenixFileEncoding()));
    }

    /*
     * Returns a Reader that reads from the underlying resource using the given Charset.
     */
    TextReader GetReader(Encoding charset)
    {
        return new StreamReader(GetInputStream(), charset);
    }
}