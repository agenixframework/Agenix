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

namespace Agenix.Validation.Xml.Schema.Locator;

/// <summary>
///     Interface for locating WSDL resources and their imports
/// </summary>
public interface IWsdlLocator : IDisposable
{
    /// <summary>
    ///     Gets the base input source for the WSDL
    /// </summary>
    /// <returns>Stream containing the base WSDL content</returns>
    Stream GetBaseInputSource();

    /// <summary>
    ///     Gets the import input source for imported WSDL or XSD files
    /// </summary>
    /// <param name="parentLocation">The location of the parent WSDL</param>
    /// <param name="importLocation">The location of the import to resolve</param>
    /// <returns>Stream containing the imported content</returns>
    Stream GetImportInputSource(string parentLocation, string importLocation);

    /// <summary>
    ///     Gets the base URI of the WSDL
    /// </summary>
    /// <returns>The base URI as a string</returns>
    string GetBaseUri();

    /// <summary>
    ///     Gets the URI of the latest import that was resolved
    /// </summary>
    /// <returns>The latest import URI as a string, or null if no imports have been resolved</returns>
    string? GetLatestImportUri();

    /// <summary>
    ///     Closes any resources held by this locator
    /// </summary>
    void Close();
}
