using System.IO;
using Agenix.Api.Condition;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Container;

public class WaitBuilderTest
{
    private Wait.Builder<ICondition> _waitBuilder;

    [SetUp]
    public void Setup()
    {
        _waitBuilder = new Wait.Builder<ICondition>();
    }

    [Test]
    public void TestConditionIsSet()
    {
        // GIVEN
        var conditionMock = new Mock<ICondition>();

        // WHEN
        _waitBuilder.Condition(conditionMock.Object);

        // THEN
        ClassicAssert.AreEqual(_waitBuilder.Build().Condition, conditionMock.Object);
    }

    [Test]
    public void TestWaitForHttpUrlIsSet()
    {
        // GIVEN
        var url = "google.com";

        // WHEN
        var builder = _waitBuilder.Http().Url(url);

        // THEN
        ClassicAssert.AreEqual(builder.GetCondition().Url, url);
    }

    [Test]
    public void TestWaitForMessageStringIsSet()
    {
        // GIVEN
        var messageName = "myMessage";

        // WHEN
        var builder = _waitBuilder.Message().Name(messageName);

        // THEN
        ClassicAssert.AreEqual(builder.GetCondition().GetMessageName(), messageName);
    }

    [Test]
    public void TestWaitForExecutionConditionIsCreated()
    {
        // GIVEN
        var echo = new EchoAction.Builder().Build();

        // WHEN
        var builder = _waitBuilder.Execution().Action(echo);

        // THEN
        ClassicAssert.AreEqual(builder.GetCondition().GetAction(), echo);
    }

    [Test]
    public void TestWaitForFilePathIsSet()
    {
        // GIVEN
        var path = "/path/to/file";

        // WHEN
        var builder = _waitBuilder.File().Path(path);

        // THEN
        ClassicAssert.AreEqual(builder.GetCondition().GetFilePath(), path);
    }

    [Test]
    public void TestWaitForFileReferenceIsSet()
    {
        // GIVEN
        var fileMock = new Mock<IFileInfoWrapper>();
        fileMock.Setup(f => f.Exists).Returns(true);
        fileMock.Setup(f => f.Name).Returns("example.txt");

        // WHEN
        var builder = _waitBuilder.File().Resource(new FileInfo(fileMock.Object.Name));

        // THEN
        ClassicAssert.AreEqual(builder.GetCondition().GetFile().Name, fileMock.Object.Name);
    }

    [Test]
    public void TestSecondsToWaitAreSet()
    {
        // GIVEN
        var seconds = 42.0;

        // WHEN
        var builder = _waitBuilder.Seconds(seconds);

        // THEN
        ClassicAssert.AreEqual("42000", builder.Build().Time);
    }

    [Test]
    public void TestMillisecondsToWaitAreSet()
    {
        // GIVEN
        var milliseconds = 400L;

        // WHEN
        var builder = _waitBuilder.Milliseconds(milliseconds);

        // THEN
        ClassicAssert.AreEqual("400", builder.Build().Time);
    }

    [Test]
    public void TestIntervalToWaitAreSet()
    {
        // GIVEN
        var interval = 100L;

        // WHEN
        var builder = _waitBuilder.Interval(interval);

        // THEN
        ClassicAssert.AreEqual("100", builder.Build().Interval);
    }

    public interface IFileInfoWrapper
    {
        string Name { get; }

        bool Exists { get; }
        // Add other properties or methods you need here
    }

    public class FileInfoWrapper(string filePath) : IFileInfoWrapper
    {
        private readonly FileInfo _fileInfo = new(filePath);

        public string Name => _fileInfo.Name;

        public bool Exists => _fileInfo.Exists;
        // Implement other properties or methods here
    }
}
