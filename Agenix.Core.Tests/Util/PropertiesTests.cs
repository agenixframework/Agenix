#region Imports

using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Util;

#endregion

namespace Agenix.Core.Tests.Util
{
	/// <summary>
	/// Unit tests for Spring.Util.Properties.
	/// </summary>
	[TestFixture]
	public class PropertiesTests
	{
        [Test]
        public void Instantiation ()
        {
            Properties root = new Properties();
            root.Add ("foo", "this");
            root.Add ("bar", "is");
            Properties props = new Properties(root);
			props.SetProperty("myPropertyKey", "myPropertyValue");
            ClassicAssert.AreEqual (3, props.Count);
            ClassicAssert.AreEqual ("this", props.GetProperty ("foo"));
            ClassicAssert.AreEqual ("is", props.GetProperty ("bar"));
            ClassicAssert.AreEqual ("myPropertyValue", props.GetProperty ("myPropertyKey"));
        }

        [Test]
        public void GetPropertyWithDefault ()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            ClassicAssert.AreEqual ("this", props.GetProperty ("foo"));
            ClassicAssert.AreEqual ("is", props.GetProperty ("bar"));
            ClassicAssert.AreEqual ("it", props.GetProperty ("baz", "it"));
        }

        [Test]
        public void Remove ()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            ClassicAssert.AreEqual (2, props.Count);
            props.Remove ("foo");
            ClassicAssert.AreEqual (1, props.Count);
            ClassicAssert.IsFalse (props.ContainsKey ("foo"));
        }

        [Test]
        public void Store ()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            props.Add ("baz", "it");
            FileInfo file = new FileInfo ("properties.test");
            try
            {
                // write 'em out with the specified header...
                using (Stream cout = file.OpenWrite ())
                {
                    props.Store (cout, "My Properties");
                }
            }
            finally
            {
                try
                {
                    file.Delete ();
                }
                catch (IOException)
                {
                }
            }
        }

        [Test]
        public void ListAndLoad()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            props.Add ("baz", "it");
            FileInfo file = new FileInfo ("properties.test");
            try
            {
                // write 'em out...
                using (Stream cout = file.OpenWrite ())
                {
                    props.List (cout);
                }
                // read 'em back in...
                using (Stream cin = file.OpenRead ())
                {
                    props = new Properties ();
                    props.Load (cin);
                    ClassicAssert.AreEqual (3, props.Count);
                    ClassicAssert.AreEqual ("this", props.GetProperty ("foo"));
                    ClassicAssert.AreEqual ("is", props.GetProperty ("bar"));
                    ClassicAssert.AreEqual ("it", props.GetProperty ("baz", "it"));
                }
            }
            finally
            {
                try
                {
                    file.Delete ();
                }
                catch (IOException)
                {
                }
            }
        }

		[Test]
		public void SimpleProperties()
		{
			string input = "key1=value1\r\nkey2:value2\r\n\r\n# a comment line\r\n   leadingspace : true";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			ClassicAssert.IsTrue("value1".Equals(props["key1"]));
			ClassicAssert.IsTrue("value2".Equals(props["key2"]));
			ClassicAssert.IsTrue("true".Equals(props["leadingspace"]));
		}

        [Test]
        public void WhitespaceProperties()
        {
            string input = "key1 =\t\nkey2:\nkey3";

            Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
            Properties props = new Properties();
            props.Load(s);

            ClassicAssert.AreEqual(string.Empty, props["key1"], "key1 should have empty value");
            ClassicAssert.AreEqual(string.Empty, props["key2"], "key2 should have empty value");
            ClassicAssert.IsTrue(props.ContainsKey("key3"));
            ClassicAssert.IsNull(props["key3"]);
        }

		[Test]
		public void Continuation()
		{
			string input = "continued = this is a long value element \\\r\nthat uses continuation \\\r\n    xxx";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			ClassicAssert.IsTrue("this is a long value element that uses continuation xxx".Equals(props["continued"]));
		}

		[Test]
		public void SeperatorEscapedWithinKey()
		{
			string input = "\\" + ":key:newvalue";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			ClassicAssert.IsTrue("newvalue".Equals(props[":key"]));
		}

		[Test]
		public void EscapedCharactersInValue()
		{
			string input = "escaped=test\\ttest";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			ClassicAssert.IsTrue("test\ttest".Equals(props["escaped"]));
		}

	}
}
