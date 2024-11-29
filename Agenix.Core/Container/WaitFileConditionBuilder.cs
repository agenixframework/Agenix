using System.IO;
using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// A builder class for constructing wait conditions related to file operations.
/// This class provides methods to configure the conditions for waiting on file operations.
/// It extends the functionality of WaitConditionBuilder for handling instances of FileCondition.
public class WaitFileConditionBuilder : WaitConditionBuilder<FileCondition, WaitFileConditionBuilder>
{
    /// A builder class for constructing wait conditions related to file operations.
    /// Extends the {@link WaitConditionBuilder} for handling {@link FileCondition} instances.
    /// /
    public WaitFileConditionBuilder(Wait.Builder<FileCondition> builder) : base(builder)
    {
    }

    /// Wait for given file path.
    /// <param name="filePath">The path of the file to wait for</param>
    /// <return>This instance of WaitFileConditionBuilder for method chaining</return>
    /// /
    public WaitFileConditionBuilder Path(string filePath)
    {
        GetCondition().SetFilePath(filePath);
        return self;
    }

    /// Wait for given file resource.
    /// @param file The file resource to wait on
    /// @return This instance of WaitFileConditionBuilder for method chaining
    /// /
    public WaitFileConditionBuilder Resource(FileInfo file)
    {
        GetCondition().SetFile(file);
        return self;
    }
}