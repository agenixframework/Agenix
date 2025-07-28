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

using Agenix.Playwright.Actions;
using Microsoft.Playwright;
using Moq;

namespace Agenix.Playwright.Tests.Actions;

public class CheckInputActionTest : AbstractPlaywrightActionTestBase
{
    protected override void CustomizeSetup()
    {
        // Setup default checkbox interaction
        SetupLocatorForCheckboxInteraction();
    }

    [Test]
    public void TestExecuteCheck()
    {
        // Arrange: Element is unchecked
        SetupLocatorForCheckboxInteraction();

        var action = new CheckInputAction.Builder()
            .Check()
            .WithTagName("input[name='checkbox']")
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.CheckAsync(It.IsAny<LocatorCheckOptions>()), Times.Once);
        Locator.Verify(x => x.UncheckAsync(It.IsAny<LocatorUncheckOptions>()), Times.Never);
    }

    [Test]
    public void TestExecuteUncheck()
    {
        // Arrange: Element is checked
        SetupLocatorForCheckboxInteraction(true);

        var action = new CheckInputAction.Builder()
            .Uncheck()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("input[name='checkbox']")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.UncheckAsync(It.IsAny<LocatorUncheckOptions>()), Times.Once);
        Locator.Verify(x => x.CheckAsync(It.IsAny<LocatorCheckOptions>()), Times.Never);
    }

    [Test]
    public void TestExecuteAlreadyChecked()
    {
        // Arrange: Element is already checked
        SetupLocatorForCheckboxInteraction(true);

        var action = new CheckInputAction.Builder()
            .Check()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("input[name='checkbox']")
            .Build();

        // Act
        action.Execute(Context);

        // Assert: Should call CheckAsync since the action always performs the operation
        Locator.Verify(x => x.CheckAsync(It.IsAny<LocatorCheckOptions>()), Times.Once);
        Locator.Verify(x => x.UncheckAsync(It.IsAny<LocatorUncheckOptions>()), Times.Never);
    }

    [Test]
    public void TestExecuteWithTimeout()
    {
        SetupLocatorForCheckboxInteraction();

        var action = new CheckInputAction.Builder()
            .Check()
            .WithTimeout(5000)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("input[name='checkbox']")
            .Build();

        action.Execute(Context);

        Locator.Verify(x => x.CheckAsync(It.Is<LocatorCheckOptions>(opts =>
            opts.Timeout >= 5000 && opts.Timeout <= 5000)), Times.Once);
    }

    [Test]
    public void TestExecuteWithForce()
    {
        SetupLocatorForCheckboxInteraction();

        var action = new CheckInputAction.Builder()
            .Check()
            .WithForce()
            .WithTagName("input[name='checkbox']")
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        action.Execute(Context);

        Locator.Verify(x => x.CheckAsync(It.Is<LocatorCheckOptions>(opts => opts.Force == true)), Times.Once);
    }
}
