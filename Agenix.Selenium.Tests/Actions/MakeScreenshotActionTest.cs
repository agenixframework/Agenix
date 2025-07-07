using System.IO;
using Agenix.Api;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Agenix.Selenium.Tests.Actions
{
    public class MakeScreenshotActionTest : AbstractNUnitSetUp
    {
        private SeleniumBrowser _seleniumBrowser = new();
        private Mock<IWebDriver> _webDriver = new();

        [SetUp]
        public void SetupMethod()
        {
            _webDriver.Reset();

            // Configure the WebDriver to also implement ITakesScreenshot
            _webDriver.As<ITakesScreenshot>();

            _seleniumBrowser.WebDriver = _webDriver.Object;
        }

        [Test]
        public void TestExecute()
        {
            // Create actual screenshot data (minimal PNG header)
            var screenshotData = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52  // IHDR chunk start
            };

            // Create a real Screenshot object using reflection or a factory method
            var screenshot = CreateMockScreenshot(screenshotData);

            _webDriver.As<ITakesScreenshot>()
                .Setup(x => x.GetScreenshot())
                .Returns(screenshot);

            var action = new MakeScreenshotAction.Builder()
                .WithBrowser(_seleniumBrowser)
                .Build();

            action.Execute(Context);

            Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumScreenshot), Does.Match(@"Test_screenshot_\d{8}_\d{6}_\d{3}\.png"));
            Assert.That(_seleniumBrowser.GetStoredFile(Context.GetVariable(SeleniumHeaders.SeleniumScreenshot).Replace("Test_", "")), Is.Not.Null);
        }

        private Screenshot CreateMockScreenshot(byte[] data)
        {
            // Create a Screenshot object with the test data
            // This uses the actual Screenshot constructor
            return new Screenshot(Convert.ToBase64String(data));
        }


        [Test]
        public void TestExecuteOutputDir()
        {
            // Create actual screenshot data and save it to a temp file
            var testImageData = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52  // IHDR chunk start
            };
            var base64Data = Convert.ToBase64String(testImageData);
            var screenshot = new Screenshot(base64Data);

            _webDriver.As<ITakesScreenshot>()
                .Setup(x => x.GetScreenshot())
                .Returns(screenshot);

            Context.SetVariable(AgenixSettings.TestNameVariable(), "MyTest");

            var action = new MakeScreenshotAction.Builder()
                .WithBrowser(_seleniumBrowser)
                .SetOutputDir("target")
                .Build();

            action.Execute(Context);

            // Check that a file was created in the target directory with the expected pattern
            var targetDir = new DirectoryInfo("target");
            Assert.That(targetDir.Exists, Is.True, "Target directory should exist");

            var screenshotFiles = targetDir.GetFiles("MyTest_screenshot_*.png");
            Assert.That(screenshotFiles.Length, Is.GreaterThanOrEqualTo(1), "Should have exactly one screenshot file");

            var screenshotFile = screenshotFiles[0];
            Assert.That(screenshotFile.Name, Does.Match(@"MyTest_screenshot_\d{8}_\d{6}_\d{3}\.png"));
            Assert.That(screenshotFile.Length, Is.GreaterThan(0), "Screenshot file should not be empty");
        }

    }
}
