#region Imports

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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

#endregion

namespace Agenix.Core.Util;

/// <summary>
///     Utility class for performing common file operations such as reading from and writing to files,
///     obtaining file extensions, and converting file content to byte arrays.
/// </summary>
public class FileUtils
{
    public static readonly string FILE_EXTENSION_XML = ".xml";
    public static readonly string FILE_EXTENSION_YAML = ".yaml";

    private static readonly ILogger Log = LogManager.GetLogger(typeof(FileUtils));

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
        if (!resource.Exists)
        {
            throw new Exception($"Failed to read resource {resource.Description} - does not exist");
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Reading file resource: '{resource.Description}' (encoding is '{encoding.WebName}')");
        }

        return ReadToString(resource.InputStream, encoding);
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
        if (inputStream == null)
        {
            throw new InvalidOperationException("Failed to read resource - input stream is empty");
        }

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
    /// <exception cref="AgenixSystemException">Thrown when there is an error writing to the file.</exception>
    public static void WriteToFile(string content, string file, Encoding charset)
    {
        Log.LogDebug($"Writing file resource: {file} (encoding is {charset.EncodingName})");

        if (!File.Exists(file))
        {
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        try
        {
            File.WriteAllText(file, content, charset);
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to write file", e);
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
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Writing file resource: '{fileInfo.Name}'");
        }

        if (fileInfo.Directory is { Exists: false })
        {
            fileInfo.Directory.Create();
        }

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
    /// <exception cref="AgenixSystemException">Thrown when there is an error writing to the file.</exception>
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

            files.AddRange(Directory.GetFiles(dir)
                .Where(file => fileNamePatterns.Any(pattern => Regex.IsMatch(file, pattern))));

            foreach (var subDir in Directory.GetDirectories(dir))
            {
                dirs.Push(subDir);
            }
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
        return Encoding.GetEncoding(AgenixSettings.AgenixFileEncoding());
    }

    /// <summary>
    ///     Retrieves a file resource based on the provided file path after replacing any dynamic content within the path.
    /// </summary>
    /// <param name="resourceName">The file path of the resource, which may contain placeholders for dynamic content.</param>
    /// <param name="context">The TestContext instance used to resolve dynamic content in the provided file path.</param>
    /// <returns>An instance of IResource that represents the resolved file resource.</returns>
    /// <exception cref="Exception">Thrown if the resource cannot be found, resolved, or accessed.</exception>
    public static IResource GetFileResource(string resourceName, TestContext context)
    {
        return GetFileResource(context.ReplaceDynamicContentInString(resourceName));
    }

    /// <summary>
    ///     Retrieves a file resource based on the provided resource name and context.
    /// </summary>
    /// <param name="resourceName">The name of the resource to be retrieved.</param>
    /// <param name="context">The context used to resolve and replace dynamic content in the resource name.</param>
    /// <returns>An instance of <see cref="IResource" /> representing the requested resource.</returns>
    public static IResource GetFileResource(string resourceName)
    {
        return new ConfigurableResourceLoader().GetResource(resourceName);
    }

    /// <summary>
    ///     Retrieves the file extension from the provided file path.
    /// </summary>
    /// <param name="path">The full path or name of the file.</param>
    /// <returns>
    ///     The file extension, including the leading period, or an empty string if the file path does not contain an
    ///     extension.
    /// </returns>
    public static string GetFileExtension(string path)
    {
        return Path.GetExtension(path);
    }

    /// <summary>
    ///     Retrieves the file name and extension from the specified path.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    /// <returns>The file name and extension as a string.</returns>
    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    ///     Retrieves the base name (file name without extension) from the given file name.
    /// </summary>
    /// <param name="fileName">The full file name, including the extension.</param>
    /// <returns>The base name of the file without its extension.</returns>
    public static string GetBaseName(string fileName)
    {
        return Path.GetFileNameWithoutExtension(fileName);
    }

    /// <summary>
    ///     Retrieves the base path of a specified file by extracting its directory path.
    /// </summary>
    /// <param name="filePath">The full path to the file whose base path is to be determined.</param>
    /// <returns>The base path of the specified file.</returns>
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
        if (file == null)
        {
            return Array.Empty<byte>();
        }

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
        if (fileStream == null)
        {
            throw new InvalidOperationException("Input stream is null");
        }

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
            using var inStream = resource.InputStream;
            if (inStream == null)
            {
                throw new InvalidOperationException(
                    $"Unable to access input stream of resource {resource.Description}");
            }

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
            throw new AgenixSystemException("Failed to read input stream", e);
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
        {
            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    properties[element.Name] = element.InnerText;
                }
            }
        }
    }

    /// <summary>
    ///     Loads settings from a configuration file located at the specified resource path and populates the provided
    ///     NameValueCollection.
    /// </summary>
    /// <param name="settings">The NameValueCollection to populate with settings from the configuration file.</param>
    /// <param name="resourcePath">The file path to the configuration file to be loaded.</param>
    /// <exception cref="ArgumentException">Thrown when the resource path is null or empty.</exception>
    /// <exception cref="ConfigurationErrorsException">Thrown if there are errors parsing the configuration file.</exception>
    /// <exception cref="IOException">Thrown if an I/O error occurs while accessing the file.</exception>
    private static void LoadFromConfigFile(NameValueCollection settings, string resourcePath)
    {
        var configMap = new ExeConfigurationFileMap { ExeConfigFilename = resourcePath };
        var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

        foreach (var key in config.AppSettings.Settings.AllKeys)
        {
            settings[key] = config.AppSettings.Settings[key].Value;
        }
    }

    /// <summary>
    ///     Loads configuration settings from the provided resource into a NameValueCollection.
    /// </summary>
    /// <param name="resource">The resource to load configuration settings from.</param>
    /// <returns>A NameValueCollection containing the configuration settings.</returns>
    /// <exception cref="ArgumentException">Thrown when the resource path is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an error occurs while loading the settings from the resource.</exception>
    public static NameValueCollection LoadAsSettings(IResource resource)
    {
        var settings = new NameValueCollection();
        // Create a temporary file to hold the stream content
        var tempFilePath = Path.GetTempFileName();

        try
        {
            // Write the stream content to the temporary file
            using (var fileStream = File.Create(tempFilePath))
            {
                resource.InputStream.CopyTo(fileStream);
            }

            if (string.IsNullOrEmpty(tempFilePath))
            {
                throw new ArgumentException("Config path must not be null or empty", nameof(tempFilePath));
            }

            if (tempFilePath.EndsWith(FILE_EXTENSION_XML, StringComparison.OrdinalIgnoreCase))
            {
                LoadFromXml(settings, tempFilePath);
            }
            else
            {
                LoadFromConfigFile(settings, tempFilePath);
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to load configuration settings from an external .config", e);
        }
        finally
        {
            // Clean up the temporary file
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (IOException ex)
                {
                    // Log but don't throw error during cleanup
                    // Assuming Log is available in this class, based on the code snippet
                    Log.LogWarning($"Failed to delete temporary configuration file: {tempFilePath}", ex);
                }
            }
        }


        return settings;
    }
}
