using System;
using System.IO;
using System.Reflection;
using Agenix.Core.Spi;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Spi;

public class ResourcesTest
{
    private static Uri baseUri;
    private static Uri baseFolderUri;
    private static Uri fileWithContentUri;
    private static Uri fileWithoutContentUri;
    private static Uri nonExistingFileUri;

    [OneTimeSetUp]
    public static void BeforeClass()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var resource = Resources.Create(testDirectory + "/ResourcesTest/");
        var directoryName = resource.GetFile().DirectoryName;
        if (directoryName != null) baseUri = new Uri(directoryName);
        baseFolderUri = new Uri(resource.GetFile().FullName);

        fileWithContentUri = new Uri(baseFolderUri, "FileWithContent.json");
        fileWithoutContentUri = new Uri(baseFolderUri, "FileWithoutContent.txt");
        nonExistingFileUri = new Uri(baseFolderUri, "NonExistingFile.txt");
    }

    [Test]
    public void UrlNullDoesNotExistTest()
    {
        Assert.That(new Resources.UrlResource(null).Exists(), Is.False);
    }

    [Test]
    public void ByteArrayResourceTest()
    {
        IResource byteArrayResource = new Resources.ByteArrayResource(new byte[100]);

        Assert.That(byteArrayResource.Exists(), Is.True);
        Assert.That(byteArrayResource.GetLocation(), Is.EqualTo(""));
        Assert.That(byteArrayResource.GetInputStream() is MemoryStream, Is.True);
        Assert.Throws<NotSupportedException>(() => byteArrayResource.GetFile());
    }

    [Test]
    public void DefaultFileSystemResourceTest()
    {
        var withContentResource = Resources.Create(fileWithContentUri.AbsolutePath);
        Assert.That(withContentResource is Resources.FileSystemResource, Is.True);
        Assert.That(withContentResource.Exists(), Is.True);
        Assert.That(withContentResource.GetFile().Exists, Is.True);
        Assert.That(withContentResource.GetInputStream() is FileStream, Is.True);

        var withoutContentResource = Resources.Create(fileWithoutContentUri.AbsolutePath);
        Assert.That(withoutContentResource is Resources.FileSystemResource, Is.True);
        Assert.That(withoutContentResource.Exists(), Is.True);
        Assert.That(withoutContentResource.GetFile().Exists, Is.True);
        Assert.That(withoutContentResource.GetInputStream() is FileStream, Is.True);

        var nonExistingResource = Resources.Create(nonExistingFileUri.AbsolutePath);
        Assert.That(nonExistingResource is Resources.ClasspathResource, Is.True);
        Assert.That(nonExistingResource.Exists(), Is.False);
    }
}