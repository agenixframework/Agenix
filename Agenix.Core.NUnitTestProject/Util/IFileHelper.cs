using System.IO;
using Agenix.Core.Exceptions;

namespace Agenix.Core.NUnitTestProject.Util;

public interface IFileHelper
{
    /// <summary>
    ///     Creates a temporary file with a unique file name using the system's temporary folder,
    ///     and modifies the extension to ".agenix.tmp". Deletes the file immediately after creation.
    /// </summary>
    /// <returns>A FileInfo object representing the created temporary file.</returns>
    /// <exception cref="CoreSystemException">Thrown if there is an I/O error during temporary file creation.</exception>
    public static FileInfo CreateTmpFile()
    {
        FileInfo tempFile;
        try
        {
            // Create the temporary file with .tmp extension
            var tempFilePath = Path.GetTempFileName();

            // Create the new path with .agenix.txt extension
            var newFilePath = Path.Combine(Path.GetDirectoryName(tempFilePath) ?? string.Empty,
                Path.GetFileNameWithoutExtension(tempFilePath) + "agenix.tmp");

            // Rename the file by moving it to the new path
            File.Move(tempFilePath, newFilePath);

            // Return the FileInfo of the new file
            tempFile = new FileInfo(newFilePath);
        }
        catch (IOException e)
        {
            throw new CoreSystemException(e.Message, e);
        }

        return tempFile;
    }
}