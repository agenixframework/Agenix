using FleetPay.Core.Exceptions;
using FleetPay.Core.Validation.Matcher;
using Moq;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Matcher
{
    public class ValidationMatcherLibraryTest
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
        public void TestGetValidationMatcher()
        {
            Assert.IsNotNull(_validationMatcherLibrary.GetValidationMatcher("customMatcher"));
        }

        [Test]
        public void TestKnowsValidationMatcher()
        {
            Assert.IsTrue(_validationMatcherLibrary.KnowsValidationMatcher("foo:customMatcher()"));
            Assert.IsFalse(_validationMatcherLibrary.KnowsValidationMatcher("foo:unknownMatcher()"));
        }

        [Test]
        public void TestUnknownValidationMatcher()
        {
            try
            {
                _validationMatcherLibrary.GetValidationMatcher("unknownMatcher");
            }
            catch (NoSuchValidationMatcherException e)
            {
                Assert.IsTrue(e.GetMessage().Contains("unknownMatcher"));
                Assert.IsTrue(e.GetMessage().Contains(_validationMatcherLibrary.Name));
                Assert.IsTrue(e.GetMessage().Contains(_validationMatcherLibrary.Prefix));
            }
        }
    }
}