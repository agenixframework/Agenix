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

    /// Wait for a given file path.
    /// <param name="filePath">The path of the file to wait for</param>
    /// <return>This instance of WaitFileConditionBuilder for method chaining</return>
    /// /
    public WaitFileConditionBuilder Path(string filePath)
    {
        GetCondition().SetFilePath(filePath);
        return self;
    }

    /// Wait for a given file resource.
    /// @param file The file resource to wait on
    /// @return This instance of WaitFileConditionBuilder for method chaining
    /// /
    public WaitFileConditionBuilder Resource(FileInfo file)
    {
        GetCondition().SetFile(file);
        return self;
    }
}
