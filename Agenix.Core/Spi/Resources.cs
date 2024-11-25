using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Spi;

/// <summary>
///     Provides utility methods for creating various types of resources such as classpath, filesystem, HTTP, JAR, and byte
///     array resources.
/// </summary>
public class Resources
{
    public static string ClasspathResourcePrefix = "classpath:";
    public static string FilesystemResourcePrefix = "file:";

    public static string JarResourcePrefix = "jar:";
    public static string HttpResourcePrefix = "http:";

    private Resources()
    {
        // static access only
    }

    /// <summary>
    ///     Constructs and returns a resource from the given file path.
    /// </summary>
    /// <param name="filePath">The file path to the resource, which can be a classpath, file system, HTTP, or JAR resource.</param>
    /// <returns>An IResource representing the constructed resource.</returns>
    public static IResource Create(string filePath)
    {
        if (filePath.StartsWith(ClasspathResourcePrefix)) return FromClasspath(filePath);

        if (filePath.StartsWith(FilesystemResourcePrefix))
            return FromFileSystem(filePath.Replace(FilesystemResourcePrefix, ""));

        if (filePath.StartsWith(HttpResourcePrefix) ||
            filePath.StartsWith(JarResourcePrefix))
            try
            {
                return Create(new Uri(filePath));
            }
            catch (UriFormatException e)
            {
                throw new CoreSystemException(e.Message);
            }

        var file = FromFileSystem(filePath);
        return file.Exists() ? file : FromClasspath(filePath);
    }

    /// <summary>
    ///     Constructs and returns a classpath resource from the given file path relative to the specified context type's
    ///     namespace.
    /// </summary>
    /// <param name="filePath">The file path to the resource within the classpath.</param>
    /// <param name="contextType">The type used to determine the namespace for resolving the classpath resource.</param>
    /// <returns>An IResource representing the classpath resource.</returns>
    public static IResource Create(string filePath, Type contextType)
    {
        return FromClasspath(filePath, contextType);
    }

    /// <summary>
    ///     Constructs and returns a resource backed by the given byte array.
    /// </summary>
    /// <param name="content">The byte array representing the content of the resource.</param>
    /// <returns>An IResource representing the byte array resource.</returns>
    public static IResource Create(byte[] content)
    {
        return new ByteArrayResource(content);
    }

    /// <summary>
    ///     Constructs and returns a classpath resource from the given file path and context type.
    /// </summary>
    /// <param name="filePath">The file path to the resource within the classpath.</param>
    /// <param name="contextType">The type from which the classpath resource is resolved.</param>
    /// <returns>An IResource representing the classpath resource.</returns>
    public static IResource Create(FileInfo file)
    {
        return new FileSystemResource(file);
    }

    /// <summary>
    ///     Constructs and returns a URL resource from the given URL.
    /// </summary>
    /// <param name="url">The URL pointing to the resource.</param>
    /// <returns>An IResource representing the URL resource.</returns>
    public static IResource Create(Uri url)
    {
        return new UrlResource(url);
    }

    /// <summary>
    ///     Constructs and returns a classpath resource from the given file path.
    /// </summary>
    /// <param name="filePath">The file path to the resource within the classpath.</param>
    /// <returns>An IResource representing the classpath resource.</returns>
    public static IResource FromClasspath(string filePath)
    {
        return new ClasspathResource(filePath);
    }

    /// <summary>
    ///     Constructs and returns a classpath resource from the given file path relative to the specified context type's
    ///     namespace.
    /// </summary>
    /// <param name="filePath">The file path to the resource within the classpath.</param>
    /// <param name="contextType">The type used to determine the namespace for resolving the classpath resource.</param>
    /// <returns>An IResource representing the classpath resource.</returns>
    public static IResource FromClasspath(string filePath, Type contextType)
    {
        var contextNamespace = contextType.Namespace?.Replace(".", "/") ?? string.Empty;
        return FromClasspath($"{contextNamespace}/{filePath}");
    }

    /// <summary>
    ///     Constructs and returns a file system resource from the given file path.
    /// </summary>
    /// <param name="filePath">The file path to the resource in the file system.</param>
    /// <returns>An IResource representing the file system resource.</returns>
    public static IResource FromFileSystem(string filePath)
    {
        return new FileSystemResource(filePath);
    }

    /// <summary>
    ///     Extracts and returns the raw path from the given file path string by removing known resource prefixes.
    /// </summary>
    /// <param name="filePath">The file path string potentially containing a resource prefix.</param>
    /// <returns>The raw file path string without any resource prefixes.</returns>
    private static string GetRawPath(string filePath)
    {
        if (filePath.StartsWith(ClasspathResourcePrefix, StringComparison.OrdinalIgnoreCase))
            return filePath[ClasspathResourcePrefix.Length..];

        return filePath.StartsWith(FilesystemResourcePrefix, StringComparison.OrdinalIgnoreCase)
            ? filePath[FilesystemResourcePrefix.Length..]
            : filePath;
    }

    /// <summary>
    ///     Represents a resource located in the classpath.
    /// </summary>
    public class ClasspathResource : IResource
    {
        private readonly string _location;

        public ClasspathResource(string location)
        {
            var raw = GetRawPath(location);

            _location = raw.StartsWith("/") ? raw[1..] : raw;
        }

        public string GetLocation()
        {
            return _location;
        }

        public bool Exists()
        {
            return GetUri() != null;
        }

        public Stream GetInputStream()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = _location.Replace("\\", "/");
            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream ??
                   throw new InvalidOperationException(
                       $"Failed to load classpath resource '{_location}' - does not exist");
        }

        public FileInfo GetFile()
        {
            if (!Exists())
                throw new InvalidOperationException(
                    $"Failed to load classpath resource '{GetLocation()}' - does not exist");

            return new FileInfo(GetUri().LocalPath);
        }

        public Uri GetUri()
        {
            var assembly = Assembly.GetEntryAssembly();
            var resourceName = _location.Replace("\\", "/");
            var resourceUrl = assembly.GetManifestResourceStream(resourceName) != null
                ? $"resource://{assembly.GetName().Name}/{resourceName}"
                : null;
            return resourceUrl != null ? new Uri(resourceUrl) : null;
        }
    }

    /// <summary>
    ///     Represents a resource backed by a byte array.
    /// </summary>
    public class ByteArrayResource(byte[] content) : IResource
    {
        private readonly byte[] _content = content ?? throw new ArgumentNullException(nameof(content));

        public string GetLocation()
        {
            return string.Empty;
        }

        public bool Exists()
        {
            return true;
        }

        public Stream GetInputStream()
        {
            return new MemoryStream(_content);
        }

        public FileInfo GetFile()
        {
            throw new NotSupportedException("ByteArrayResource does not provide access to a file");
        }
    }

    /// <summary>
    ///     Represents a resource located at a specified URL.
    /// </summary>
    public class UrlResource(Uri url) : IResource
    {
        //?? throw new ArgumentNullException(nameof(url));

        public string GetLocation()
        {
            return url.ToString();
        }

        public bool Exists()
        {
            if (url == null) return false;

            if ("file".Equals(url.Scheme, StringComparison.OrdinalIgnoreCase))
                try
                {
                    return new FileInfo(url.LocalPath).Exists;
                }
                catch (UriFormatException e)
                {
                    throw new InvalidOperationException($"Unable to parse absolute file URL: {url}", e);
                }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";

            try
            {
                using var response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                return false;
            }
        }

        public Stream GetInputStream()
        {
            try
            {
                return url.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase)
                    ? new FileStream(url.LocalPath, FileMode.Open, FileAccess.Read)
                    : new HttpClient().GetStreamAsync(url).Result;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Cannot open stream for URL: {url}", e);
            }
        }

        public FileInfo GetFile()
        {
            if (!"file".Equals(url.Scheme, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Failed to resolve to absolute file path because it does not reside in the file system: {url}");
            try
            {
                return new FileInfo(url.LocalPath);
            }
            catch (UriFormatException ex)
            {
                throw new InvalidOperationException($"Failed to resolve URL to file: {url}", ex);
            }
        }
    }

    /// <summary>
    ///     Represents a resource located in the file system.
    /// </summary>
    public class FileSystemResource : IResource
    {
        private readonly FileInfo _file;

        public FileSystemResource(string path)
        {
            _file = new FileInfo(GetRawPath(path));
        }

        public FileSystemResource(FileInfo file)
        {
            _file = file;
        }

        public string GetLocation()
        {
            return _file.FullName;
        }

        public bool Exists()
        {
            return _file.Exists || Directory.Exists(_file.FullName);
        }

        public Stream GetInputStream()
        {
            if (!Exists()) throw new InvalidOperationException($"{_file} does not exist");

            if (_file.Attributes.HasFlag(FileAttributes.Directory))
                throw new NotSupportedException($"{_file} is a directory");

            try
            {
                return new FileStream(_file.FullName, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException e)
            {
                throw new InvalidOperationException($"{_file} does not exist", e);
            }
        }

        public FileInfo GetFile()
        {
            return _file;
        }

        public Uri GetUri()
        {
            return new Uri(_file.FullName);
        }
    }
}