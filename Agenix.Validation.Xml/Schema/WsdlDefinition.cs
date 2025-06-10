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


namespace Agenix.Validation.Xml.Schema;

public class WsdlDefinition
{
    public string TargetNamespace { get; set; }
    public WsdlTypes Types { get; set; }
    public IDictionary<string, string> Namespaces { get; set; }
    public string DocumentBaseUri { get; set; }
    public List<WsdlImport> Imports { get; set; }
}


public class WsdlTypes
{
    public List<object> ExtensibilityElements { get; set; } = new();
}

public class WsdlImport
{
    public string LocationUri { get; set; }
    public string Namespace { get; set; }
}


public class WsdlType
{
    public List<WsdlSchema> Schemas { get; set; } = [];
}

public class WsdlSchema
{
    public string TargetNamespace { get; set; }
    public string ElementFormDefault { get; set; }
    public string AttributeFormDefault { get; set; }
    public List<WsdlSchemaImport> Imports { get; set; } = [];
    public List<WsdlSchemaInclude> Includes { get; set; } = [];
}

public class WsdlSchemaImport
{
    public string Namespace { get; set; }
    public string SchemaLocation { get; set; }
}

public class WsdlSchemaInclude
{
    public string SchemaLocation { get; set; }
}



