using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using log4net;

namespace Agenix.Core.Util;

/// <summary>
///     Utility class for performing common file operations such as reading from and writing to files,
///     obtaining file extensions, and converting file content to byte arrays.
/// </summary>
public class FileUtils
{
    public static readonly string FILE_EXTENSION_JAVA = ".java";
    public static readonly string FILE_EXTENSION_XML = ".xml";
    public static readonly string FILE_EXTENSION_GROOVY = ".groovy";
    public static readonly string FILE_EXTENSION_YAML = ".yaml";

    private static readonly ILog _log = LogManager.GetLogger(typeof(FileUtils));

    /// <summary>
    ///     Reads the content of the provided resource and converts it to a string using the default charset.
    /// </summary>
    /// <param name="resource">The resource from which to read the content.</param>
    /// <returns>A string representation of the content read from the resource.</returns>
    /// <exception cref="Exception">Thrown when the resource does not exist.</exception>
    public static string ReadToString(IResource resource)
    {
        return ReadToString(resource, GetDefaultCharset());
    }

    /// <summary>
    ///     Reads the content of the provided input stream and converts it to a string using the default charset.
    /// </summary>
    /// <param name="inputStream">The stream to be read.</param>
    /// <returns>A string representation of the content read from the input stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input stream is null.</exception>
    public static string ReadToString(Stream inputStream)
    {
        return ReadToString(inputStream, GetDefaultCharset());
    }

    /// <summary>
    ///     Reads the content of the provided file and converts it to a string using the default charset.
    /// </summary>
    /// <param name="fileInfo">The file from which to read the content.</param>
    /// <returns>A string representation of the content read from the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the fileInfo parameter is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file cannot be found.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs while opening the file.</exception>
    public static string ReadToString(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        using var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
        return ReadToString(fileStream, GetDefaultCharset());
    }

    /// <summary>
    ///     Reads the content of the provided resource and converts it to a string using the specified encoding.
    /// </summary>
    /// <param name="resource">The resource from which to read the content.</param>
    /// <param name="encoding">The encoding to use for converting the resource's byte content to a string.</param>
    /// <returns>A string representation of the content read from the resource.</returns>
    /// <exception cref="Exception">Thrown when the resource does not exist.</exception>
    public static string ReadToString(IResource resource, Encoding encoding)
    {
        if (!resource.Exists())
            throw new Exception($"Failed to read resource {resource.GetLocation()} - does not exist");

        if (_log.IsDebugEnabled)
            _log.Debug($"Reading file resource: '{resource.GetLocation()}' (encoding is '{encoding.WebName}')");

        return ReadToString(resource.GetInputStream(), encoding);
    }

    /// <summary>
    ///     Reads the content of the provided input stream and converts it to a string using the specified encoding.
    /// </summary>
    /// <param name="inputStream">The stream to be read.</param>
    /// <param name="encoding">The encoding to be used for converting the stream's byte content to a string.</param>
    /// <returns>A string representation of the content read from the input stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input stream is null.</exception>
    public static string ReadToString(Stream inputStream, Encoding encoding)
    {
        if (inputStream == null) throw new InvalidOperationException("Failed to read resource - input stream is empty");

        using var memoryStream = new MemoryStream();
        inputStream.CopyTo(memoryStream);
        return encoding.GetString(memoryStream.ToArray());
    }

    /// <summary>
    ///     Writes the specified content to a file at the given path using the specified encoding.
    /// </summary>
    /// <param name="content">The content to be written to the file.</param>
    /// <param name="file">The path of the file where the content will be written.</param>
    /// <param name="charset">The encoding to be used when writing the content to the file.</param>
    /// <exception cref="CoreSystemException">Thrown when there is an error writing to the file.</exception>
    public static void WriteToFile(string content, string file, Encoding charset)
    {
        _log.Debug($"Writing file resource: {file} (encoding is {charset.EncodingName})");

        if (!File.Exists(file))
        {
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        try
        {
            File.WriteAllText(file, content, charset);
        }
        catch (IOException e)
        {
            throw new CoreSystemException("Failed to write file", e);
        }
    }

    /// <summary>
    ///     Writes the provided content to a file using the specified encoding.
    /// </summary>
    /// <param name="content">The content to be written to the file.</param>
    /// <param name="file">The path of the file where the content will be written.</param>
    /// <param name="charset">The encoding to be used for writing the content to the file.</param>
    /// <exception cref="IOException">Thrown when there is an error during the file writing process.</exception>
    public static void WriteToFile(Stream inputStream, FileInfo fileInfo)
    {
        if (_log.IsDebugEnabled) _log.Debug($"Writing file resource: '{fileInfo.Name}'");

        if (fileInfo.Directory is { Exists: false }) fileInfo.Directory.Create();

        try
        {
            using var outputFileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write);
            inputStream.CopyTo(outputFileStream);
        }
        catch (IOException e)
        {
            throw new Exception("Failed to write file", e);
        }
    }

    /// <summary>
    ///     Writes the specified content to a file at the given path using the specified encoding.
    /// </summary>
    /// <param name="content">The content to be written to the file.</param>
    /// <param name="file">The path of the file where the content will be written.</param>
    /// <param name="charset">The encoding to be used when writing the content to the file.</param>
    /// <exception cref="CoreSystemException">Thrown when there is an error writing to the file.</exception>
    public static void WriteToFile(string content, string file)
    {
        WriteToFile(content, file, GetDefaultCharset());
    }

    /// <summary>
    ///     Searches for files in the specified starting directory and its subdirectories that match any of the provided
    ///     filename patterns.
    /// </summary>
    /// <param name="startDir">The starting directory for the search.</param>
    /// <param name="fileNamePatterns">A set of filename patterns to match against.</param>
    /// <returns>A list of files that match any of the provided filename patterns.</returns>
    public static List<string> findFiles(string startDir, HashSet<string> fileNamePatterns)
    {
        var files = new List<string>();

        var dirs = new Stack<string>();
        dirs.Push(startDir);

        while (dirs.Count > 0)
        {
            var dir = dirs.Pop();

            foreach (var file in Directory.GetFiles(dir))
                if (fileNamePatterns.Any(pattern => Regex.IsMatch(file, pattern)))
                    files.Add(file);

            foreach (var subDir in Directory.GetDirectories(dir)) dirs.Push(subDir);
        }

        return files;
    }

    /// <summary>
    ///     Gets the default charset. If set by Agenix system property (agenix.file.encoding) use
    ///     this one otherwise use system default.
    /// </summary>
    /// <returns>The default encoding to be used based on the Agenix system property or the system default.</returns>
    public static Encoding GetDefaultCharset()
    {
        return Encoding.GetEncoding(CoreSettings.AgenixFileEncoding);
    }

    public static IResource GetFileResource(string filePath, TestContext context)
    {
        return GetFileResource(context.ReplaceDynamicContentInString(filePath));
    }

    public static IResource GetFileResource(string filePath)
    {
        return Resources.Create(filePath);
    }

    public static string GetFileExtension(string path)
    {
        return Path.GetExtension(path);
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public static string GetBaseName(string fileName)
    {
        return Path.GetFileNameWithoutExtension(fileName);
    }

    public static string GetBasePath(string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }

    /// <summary>
    ///     Copies the content of the specified file to a byte array.
    /// </summary>
    /// <param name="file">The file from which to copy the content.</param>
    /// <returns>A byte array representation of the content of the file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file content cannot be read.</exception>
    public static byte[] CopyToByteArray(FileInfo file)
    {
        if (file == null) return Array.Empty<byte>();

        try
        {
            using var inStream = file.OpenRead();
            return ReadAllBytes(inStream);
        }
        catch (IOException e)
        {
            throw new InvalidOperationException("Failed to read file content", e);
        }
    }

    /// <summary>
    ///     Reads all bytes from the provided file stream.
    /// </summary>
    /// <param name="fileStream">The file stream from which to read bytes.</param>
    /// <returns>A byte array containing the bytes read from the file stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file stream is null.</exception>
    private static byte[] ReadAllBytes(FileStream fileStream)
    {
        if (fileStream == null) throw new InvalidOperationException("Input stream is null");

        using var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Copies the content of the provided resource to a byte array.
    /// </summary>
    /// <param name="resource">The resource from which to copy the content.</param>
    /// <returns>A byte array containing the content copied from the resource.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if unable to access the input stream of the resource or if reading
    ///     the resource fails.
    /// </exception>
    public static byte[] CopyToByteArray(IResource resource)
    {
        try
        {
            using var inStream = resource.GetInputStream();
            if (inStream == null)
                throw new InvalidOperationException(
                    $"Unable to access input stream of resource {resource.GetLocation()}");

            using var memoryStream = new MemoryStream();
            inStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        catch (IOException e)
        {
            throw new InvalidOperationException("Failed to read resource", e);
        }
    }

    /// <summary>
    ///     Reads the content of the provided input stream and converts it to a byte array.
    /// </summary>
    /// <param name="inputStream">The input stream from which to read the content.</param>
    /// <returns>A byte array containing the data read from the input stream.</returns>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the input stream cannot be read.</exception>
    public static byte[] CopyToByteArray(Stream inputStream)
    {
        try
        {
            using (inputStream) // Ensure the stream gets disposed
            {
                using (var memoryStream = new MemoryStream())
                {
                    inputStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        catch (IOException e)
        {
            throw new CoreSystemException("Failed to read input stream", e);
        }
    }

    public static TestSource GetTestSource(string sourceFile)
    {
        var ext = GetFileExtension(sourceFile);
        var name = GetFileName(sourceFile);
        return new TestSource(ext, name, sourceFile);
    }

    /// <summary>
    ///     Loads properties from an XML file into the provided NameValueCollection.
    /// </summary>
    /// <param name="properties">The collection to populate with properties from the XML file.</param>
    /// <param name="resourcePath">The path to the XML file from which to load properties.</param>
    private static void LoadFromXml(NameValueCollection properties, string resourcePath)
    {
        var xmlDoc = new XmlDocument();

        using (var stream = new FileStream(resourcePath, FileMode.Open, FileAccess.Read))
        {
            xmlDoc.Load(stream);
        }

        if (xmlDoc.DocumentElement != null)
            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                if (node is XmlElement element)
                    properties[element.Name] = element.InnerText;
    }

    /// <summary>
    ///     Loads settings from a .config file located at the specified config path
    ///     and adds them to the provided NameValueCollection.
    /// </summary>
    /// <param name="settings">The NameValueCollection to which the settings will be added.</param>
    /// <param name="resourcePath">The path of the settings file to load.</param>
    private static void LoadFromConfigFile(NameValueCollection settings, string resourcePath)
    {
        var configMap = new ExeConfigurationFileMap { ExeConfigFilename = resourcePath };
        var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

        foreach (var key in config.AppSettings.Settings.AllKeys) settings[key] = config.AppSettings.Settings[key].Value;
    }

    /// <summary>
    ///     Loads the settings from the specified config path into a NameValueCollection.
    ///     Supports both XML and .config file formats.
    /// </summary>
    /// <param name="configPath">The path to the config file to load configuration settings from.</param>
    /// <returns>A NameValueCollection containing the settings loaded from the config.</returns>
    /// <exception cref="ArgumentException">Thrown when the resource path is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the settings cannot be loaded from the config.</exception>
    public static NameValueCollection LoadAsSettings(IResource resource)
    {
        var settings = new NameValueCollection();
        var configPath = resource.GetLocation();

        try
        {
            if (string.IsNullOrEmpty(configPath))
                throw new ArgumentException("Config path must not be null or empty", nameof(configPath));

            if (configPath.EndsWith(FILE_EXTENSION_XML, StringComparison.OrdinalIgnoreCase))
                LoadFromXml(settings, configPath);
            else
                LoadFromConfigFile(settings, configPath);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to load configuration settings from an external .config", e);
        }

        return settings;
    }
}