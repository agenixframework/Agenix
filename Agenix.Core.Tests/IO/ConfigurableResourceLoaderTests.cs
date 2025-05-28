#region Imports

using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;

#endregion

namespace Agenix.Core.Tests.IO
{
	/// <summary>
	/// Unit tests for the ConfigurableResourceLoader class.
	/// </summary>
	[TestFixture]
	public sealed class ConfigurableResourceLoaderTests
	{
		private ConfigurableResourceLoader loader;

        [SetUp]
        public void SetUp()
        {
            loader = new ConfigurableResourceLoader();
        }

		#region ConfigurableResourceLoader.GetResource Tests

		/// <summary>
		/// Tests that loader correctly loads files specified by absolute name, regardless
		/// of the fact whether protocol name is specified or not.
		/// </summary>
		[Test]
		public void GetAbsoluteFileSystemResource()
		{
			string fileName = Path.GetTempFileName();
			try
			{
				IResource withoutProtocol = loader.GetResource(fileName);
				ClassicAssert.IsNotNull(withoutProtocol, "Resource should not be null");
				ClassicAssert.IsTrue(withoutProtocol is FileSystemResource, "Expected FileSystemResource");
				ClassicAssert.IsTrue(withoutProtocol.Exists, "Resource should exist but it does not");

				IResource withProtocol = loader.GetResource("file:///" + fileName);
				ClassicAssert.IsNotNull(withProtocol, "Resource should not be null");
				ClassicAssert.IsTrue(withProtocol is FileSystemResource, "Expected FileSystemResource");
				ClassicAssert.IsTrue(withProtocol.Exists, "Resource should exist but it does not");
			}
			finally
			{
				new FileInfo(fileName).Delete();
			}
		}

	    [Test]
	    public void GetResourceThatSupportsTheSpecialHomeCharacter()
	    {
	        string filename = "foo.txt";
            FileInfo expectedFile =
                new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
            StreamWriter writer = expectedFile.CreateText();
            FileSystemResource res = (FileSystemResource) loader.GetResource("~/" + filename);
            ClassicAssert.AreEqual(expectedFile.FullName, res.File.FullName);
            try
            {
                writer.Close();
            }
            catch (IOException)
            {
            }
            try
            {
                expectedFile.Delete();
            }
            catch (IOException)
            {
            }
        }

        [Test]
        public void GetResourceThatSupportsTheSpecialHomeCharacter_WithLeadingWhitespace()
        {
            string filename = "foo.txt";
            FileInfo expectedFile =
                new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
            StreamWriter writer = expectedFile.CreateText();
            FileSystemResource res = (FileSystemResource) loader.GetResource("   ~/" + filename);
            ClassicAssert.AreEqual(expectedFile.FullName, res.File.FullName);
            try
            {
                writer.Close();
            }
            catch (IOException)
            {
            }
            try
            {
                expectedFile.Delete();
            }
            catch (IOException)
            {
            }
        }

		/// <summary>
		/// Tests that loader correctly loads files specified by relative name, regardless
		/// of the fact whether protocol name is specified or not.
		/// </summary>
		[Test]
		public void GetRelativeFileSystemResource()
		{
			string fileName = "test.tmp";
			FileInfo fi = new FileInfo(fileName);
			FileStream fs = fi.Create();
			fs.Close();

			try
			{
				IResource withoutProtocol = loader.GetResource(fileName);
				ClassicAssert.IsNotNull(withoutProtocol, "Resource should not be null");
				ClassicAssert.IsTrue(withoutProtocol is FileSystemResource, "Expected FileSystemResource");
				ClassicAssert.IsTrue(withoutProtocol.Exists, "Resource should exist but it does not");

				IResource withProtocol = loader.GetResource("file://" + fileName);
				ClassicAssert.IsNotNull(withProtocol, "Resource should not be null");
				ClassicAssert.IsTrue(withProtocol is FileSystemResource, "Expected FileSystemResource");
				ClassicAssert.IsTrue(withProtocol.Exists, "Resource should exist but it does not");
			}
			finally
			{
				fi.Delete();
			}
		}

		/// <summary>
		/// Tests that loader can load UrlResource over HTTP protocol
		/// </summary>
		[Test]
		[Explicit]
		public void GetHttpUrlResource()
		{
			IResource res = loader.GetResource("http://www.springframework.net/license.html");
			ClassicAssert.IsNotNull(res, "Resource should not be null");
            ClassicAssert.AreEqual(typeof(UrlResource), res.GetType());
		}

		/// <summary>
		/// Tests that loader can load UrlResource over assembly pseudo protocol
		/// </summary>
		[Test]
		public void GetAssemblyResource()
		{
			IResource res = loader.GetResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
			ClassicAssert.IsNotNull(res, "Resource should not be null");
			ClassicAssert.AreEqual(typeof(AssemblyResource), res.GetType());
		}

		#endregion

		[Test]
		public void GetResourceForNonMappedProtocol()
		{
            ClassicAssert.Throws<UriFormatException>(() => new ConfigurableResourceLoader().GetResource("beep://foo.xml"));
        }
	}
}
