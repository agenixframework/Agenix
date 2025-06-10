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

/// <summary>
///     Interface for a factory that creates instances of WSDL (Web Services Description Language) readers.
///     Provides a mechanism for obtaining new WSDL reader instances and allows for centralized
///     management of how these instances are created.
/// </summary>
/// <remarks>
///     The <see cref="IWsdlFactory" /> interface defines the contract for a factory that
///     can create WSDL reader instances. Implementations of this interface, such as
///     <see cref="WsdlFactory" />, may encapsulate the logic for constructing different
///     types of WSDL readers, ensuring modularity and ease of use.
/// </remarks>
public interface IWsdlFactory
{
    IWsdlReader NewWsdlReader();

    static IWsdlFactory NewInstance()
    {
        return new WsdlFactory();
    }
}

/// <summary>
///     Factory class for creating instances of WSDL (Web Services Description Language) readers.
///     Implements the <see cref="IWsdlFactory" /> interface to provide a mechanism for obtaining
///     WSDL reader instances.
/// </summary>
/// <remarks>
///     The <see cref="WsdlFactory" /> class is used to centralize the creation of WSDL
///     readers, such as instances of <see cref="WcfWsdlReader" />. This allows for modularity
///     and ease of extension. The factory follows a static creation pattern via
///     the <see cref="NewInstance" /> method.
/// </remarks>
public class WsdlFactory : IWsdlFactory
{
    public static IWsdlFactory NewInstance()
    {
        return new WsdlFactory();
    }

    public IWsdlReader NewWsdlReader()
    {
        return new WcfWsdlReader();
    }
}
