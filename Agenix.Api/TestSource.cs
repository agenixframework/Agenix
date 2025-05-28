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

namespace Agenix.Api;

public class TestSource
{
    /**
     * Test type, name and optional sourceFile
     */
    private readonly string _type;

    public TestSource(string type, string name)
        : this(type, name, null)
    {
    }

    public TestSource(string type, string name, string filePath)
    {
        _type = type;
        Name = name;
        FilePath = filePath;
    }

    public TestSource(Type testClass)
        : this("cs", testClass.Name)
    {
    }

    /**
     * The test source type. Usually one of java, xml, groovy, yaml.
     * @return
     */
    public string Type => _type;

    /**
     * Gets the name.
     * @return
     */
    public string Name { get; }

    /**
     * Optional source file path.
     * @return
     */
    public string FilePath { get; }
}
