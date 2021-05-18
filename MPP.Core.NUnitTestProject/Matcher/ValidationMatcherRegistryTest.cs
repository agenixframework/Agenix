using Moq;
using MPP.Core.Exceptions;
using MPP.Core.Validation.Matcher;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Matcher
{
    public class ValidationMatcherRegistryTest : AbstractNUnitSetUp
    {
        private readonly Mock<IValidationMatcher> _matcher = new();
        private readonly ValidationMatcherLibrary _validationMatcherLibrary = new();

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            _validationMatcherLibrary.Name = "fooValidationMatcherLibrary";
            _validationMatcherLibrary.Prefix = "foo:";
            _validationMatcherLibrary.Members.Add("customMatcher", _matcher.Object);
        }

        [Test]
        public void TestGetValidationMatcherLibrary()
        {
            Context.ValidationMatcherRegistry.AddValidationMatcherLibrary(_validationMatcherLibrary);
            Assert.AreEqual(_validationMatcherLibrary, Context.ValidationMatcherRegistry.GetLibraryForPrefix("foo:"));
        }

        [Test]
        public void TestUnknownValidationMatcherLibrary()
        {
            try
            {
                Context.ValidationMatcherRegistry.AddValidationMatcherLibrary(_validationMatcherLibrary);
                Context.ValidationMatcherRegistry.GetLibraryForPrefix("unknown:");
            }
            catch (NoSuchValidationMatcherLibraryException e)
            {
                Assert.IsTrue(e.GetMessage().Contains("unknown:"));
            }
        }
    }
}