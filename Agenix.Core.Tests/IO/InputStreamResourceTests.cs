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
	/// Unit tests for the InputStreamResource class.
    /// </summary>
	[TestFixture]
    public sealed class InputStreamResourceTests
    {
        [Test]
        public void Instantiation ()
        {
            FileInfo file = null;
            Stream stream = null;
            try
            {
                file = new FileInfo ("Instantiation");
                stream = file.Create ();
                InputStreamResource res = new InputStreamResource (stream, "A temporary resource.");
                ClassicAssert.IsTrue (res.IsOpen);
                ClassicAssert.IsTrue (res.Exists);
                ClassicAssert.IsNotNull (res.InputStream);
            }
            finally
            {
                try
                {
                    if (stream != null)
                    {
                        stream.Close ();
                    }
                    if (file != null
                        && file.Exists)
                    {
                        file.Delete ();
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        [Test]
        public void InstantiationWithNull ()
        {
            Assert.Throws<ArgumentNullException>(() => new InputStreamResource (null, "A null resource."));
        }

        [Test]
        public void ReadStreamMultipleTimes ()
        {
            FileInfo file = null;
            Stream stream = null;
            try
            {
                file = new FileInfo ("ReadStreamMultipleTimes");
                stream = file.Create ();
                // attempting to read this stream twice is an error...
                InputStreamResource res = new InputStreamResource (stream, "A temporary resource.");
                Stream streamOne = res.InputStream;
                Stream streamTwo;
                Assert.Throws<InvalidOperationException>(() => streamTwo = res.InputStream); // should bail here
            }
            finally
            {
                try
                {
                    if (stream != null)
                    {
                        stream.Close ();
                    }
                    if (file != null
                        && file.Exists)
                    {
                        file.Delete ();
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
	}
}
