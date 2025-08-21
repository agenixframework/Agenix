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

using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class CheckInputActionTest : AbstractNUnitSetUp
{
    private readonly Mock<IWebElement> _element = new();
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _element.Reset();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _element.Setup(x => x.Displayed).Returns(true);
        _element.Setup(x => x.Enabled).Returns(true);
        _element.Setup(x => x.TagName).Returns("input");
    }

    [Test]
    public async Task TestExecuteCheck()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Selected).Returns(false);

        var action = new CheckInputAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "checkbox")
            .SetChecked(true)
            .Build();

        await action.ExecuteAsync(Context);

        _element.Verify(x => x.Click(), Times.Once);
    }

    [Test]
    public async Task TestExecuteUncheck()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Selected).Returns(true);

        var action = new CheckInputAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "checkbox")
            .SetChecked(false)
            .Build();

        await action.ExecuteAsync(Context);

        _element.Verify(x => x.Click(), Times.Once);
    }

    [Test]
    public async Task TestExecuteAlreadyChecked()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.Selected).Returns(true);

        var action = new CheckInputAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "checkbox")
            .SetChecked(true)
            .Build();

        await action.ExecuteAsync(Context);

        _element.Verify(x => x.Click(), Times.Never);
    }
}
