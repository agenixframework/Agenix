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

using System;
using System.Collections.Generic;
using System.Linq;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation.Xml;
using Agenix.Api.Variable;
using Agenix.Core.Validation.Json;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation;

/// <summary>
///     A class that delegates the extraction of payload variables based on specified path expressions and namespaces.
/// </summary>
/// <remarks>
///     This class implements the <see cref="IVariableExtractor" /> interface and processes messages to extract
///     variables using JSON or XPath expressions defined by the user.
/// </remarks>
public class DelegatingPayloadVariableExtractor : IVariableExtractor
{
    /// <summary>
    ///     A logger instance used for logging within the DelegatingPayloadVariableExtractor class.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DelegatingPayloadVariableExtractor));

    public DelegatingPayloadVariableExtractor() : this(new Builder())
    {
    }

    public DelegatingPayloadVariableExtractor(Builder builder)
    {
        PathExpressions = builder._expressions;
        Namespaces = builder._namespaces;
    }

    public Dictionary<string, object> PathExpressions { get; set; }
    public Dictionary<string, string> Namespaces { get; set; }

    /// <summary>
    ///     Processes the given message within the specified test context.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">
    ///     The test context providing the necessary path expressions and dynamic content replacement
    ///     mechanisms.
    /// </param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }

    /// <summary>
    ///     Extracts variables from the specified message based on path expressions defined in the context.
    /// </summary>
    /// <param name="message">The message from which variables should be extracted.</param>
    /// <param name="context">The test context that provides path expressions and dynamic content replacement mechanisms.</param>
    public virtual void ExtractVariables(IMessage message, TestContext context)
    {
        if (PathExpressions.Count == 0)
        {
            return;
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Reading path elements.");
        }

        var jsonPathExpressions = new Dictionary<string, object>();
        var xpathExpressions = new Dictionary<string, object>();

        foreach (var pathExpression in PathExpressions)
        {
            var path = context.ReplaceDynamicContentInString(pathExpression.Key);
            var variable = pathExpression.Value;

            if (JsonPathMessageValidationContext.IsJsonPathExpression(path))
            {
                jsonPathExpressions[path] = variable;
            }
            else
            {
                xpathExpressions[path] = variable;
            }
        }

        if (jsonPathExpressions.Count != 0)
        {
            var jsonPathBuilder = LookupVariableExtractor("jsonPath", context);
            if (jsonPathBuilder != null)
            {
                var extractor = InvokeBuilderMethods(jsonPathBuilder, jsonPathExpressions, null);
                extractor?.ExtractVariables(message, context);
            }
        }

        if (xpathExpressions.Count != 0)
        {
            var xpathExtractor = LookupVariableExtractor("xpath", context);

            if (Namespaces.Count != 0 && xpathExtractor is IXmlNamespaceAware xmlNamespaceAware)
            {
                xmlNamespaceAware.SetNamespaces(Namespaces);
            }

            if (xpathExtractor != null)
            {
                var extractor = InvokeBuilderMethods(xpathExtractor, xpathExpressions, Namespaces);
                extractor?.ExtractVariables(message, context);
            }
        }
    }

    /// <summary>
    ///     Invokes builder methods using reflection to avoid dynamic casting issues.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    /// <param name="expressions">The expressions to set</param>
    /// <param name="namespaces">The namespaces to set (optional)</param>
    /// <returns>The built IVariableExtractor instance</returns>
    private static IVariableExtractor InvokeBuilderMethods(object builder, Dictionary<string, object> expressions,
        Dictionary<string, string> namespaces)
    {
        try
        {
            var builderType = builder.GetType();

            // Try to set namespaces first if provided
            if (namespaces is { Count: > 0 })
            {
                var namespacesMethod = builderType.GetMethod("Namespaces", [typeof(Dictionary<string, string>)]);
                if (namespacesMethod != null)
                {
                    builder = namespacesMethod.Invoke(builder, [namespaces]);
                }
            }

            // Set expressions
            var expressionsMethod = builderType.GetMethod("Expressions", [typeof(IDictionary<string, object>)])
                                    ?? builderType.GetMethod("Expressions", [typeof(Dictionary<string, object>)]);

            if (expressionsMethod != null)
            {
                builder = expressionsMethod.Invoke(builder, [expressions]);
            }
            else
            {
                // Try an individual expression method if Expressions method doesn't exist
                var expressionMethod = builderType.GetMethod("Expression", [typeof(string), typeof(object)]);
                if (expressionMethod != null)
                {
                    builder = expressions.Aggregate(builder,
                        (current, expr) => expressionMethod.Invoke(current, [expr.Key, expr.Value]));
                }
            }

            // Build the extractor
            var buildMethod = builderType.GetMethod("Build", Type.EmptyTypes);
            if (buildMethod != null)
            {
                var extractor = buildMethod.Invoke(builder, null) as IVariableExtractor;

                // Handle namespaces on the extractor if not handled on the builder
                if (namespaces is { Count: > 0 } && extractor is IXmlNamespaceAware xmlNamespaceAware)
                {
                    xmlNamespaceAware.SetNamespaces(namespaces);
                }

                return extractor;
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning("Failed to invoke builder methods via reflection: {ExMessage}", ex.Message);
        }

        return null;
    }


    /// <summary>
    ///     Looks up the appropriate IVariableExtractor.IBuilder implementation based on the specified type and context.
    /// </summary>
    /// <param name="type">The type name of the variable extractor builder to look up.</param>
    /// <param name="context">The test context which provides the reference resolver to resolve the builder instance.</param>
    /// <typeparam name="T">The IVariableExtractor implementation type being sought.</typeparam>
    /// <typeparam name="TB">The builder type for the IVariableExtractor implementation.</typeparam>
    /// <returns>
    ///     An instance of IVariableExtractor.IBuilder corresponding to the specified type, if resolvable.
    /// </returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown if no appropriate implementation of the specified type can be found.
    /// </exception>
    private object LookupVariableExtractor(string type, TestContext context)
    {
        try
        {
            // Try the IVariableExtractor.Lookup method first
            var lookupResult = IVariableExtractor.Lookup<IBuilder>(type);
            if (lookupResult.IsPresent)
            {
                return lookupResult;
            }
        }
        catch (InvalidCastException ex)
        {
            Log.LogError("Direct lookup failed for type '{Type}': {ExMessage}, trying alternative resolution", type,
                ex.Message);
        }

        // Fallback to reference resolver
        if (context.ReferenceResolver.IsResolvable(type))
        {
            return context.ReferenceResolver.Resolve(type);
        }

        if (context.ReferenceResolver.IsResolvable(type + "VariableExtractorBuilder"))
        {
            return context.ReferenceResolver.Resolve(type + "VariableExtractorBuilder");
        }

        if (context.ReferenceResolver.IsResolvable(type + "PayloadVariableExtractorBuilder"))
        {
            return context.ReferenceResolver.Resolve(type + "PayloadVariableExtractorBuilder");
        }

        throw new AgenixSystemException(
            $"Missing proper variable extractor implementation of type '{type}' - consider adding proper validation module to the project");
    }

    /// <summary>
    ///     Builder class for constructing instances of <see cref="DelegatingPayloadVariableExtractor" />.
    /// </summary>
    /// <remarks>
    ///     This class provides a fluent interface for specifying namespaces and expressions,
    ///     and for building <see cref="DelegatingPayloadVariableExtractor" /> objects.
    /// </remarks>
    public class Builder : IVariableExtractor.IBuilder<DelegatingPayloadVariableExtractor, Builder>
    {
        internal readonly Dictionary<string, object> _expressions = new();
        internal readonly Dictionary<string, string> _namespaces = new();

        /// <summary>
        ///     Adds multiple expressions and their associated variable names to the collection.
        /// </summary>
        /// <param name="expressions">A dictionary containing expressions and their associated variable names.</param>
        /// <returns>
        ///     An instance of the <see cref="Builder" /> class.
        /// </returns>
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var expression in expressions)
            {
                _expressions[expression.Key] = expression.Value;
            }

            return this;
        }

        /// <summary>
        ///     Adds an expression and its associated variable name to the collection of expressions.
        /// </summary>
        /// <param name="path">The path of the expression.</param>
        /// <param name="variableName">The variable name associated with the expression.</param>
        /// <returns>
        ///     An instance of the <see cref="Builder" /> class.
        /// </returns>
        public Builder Expression(string path, object variableName)
        {
            _expressions[path] = variableName;
            return this;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="DelegatingPayloadVariableExtractor" /> class.
        /// </summary>
        /// <returns>
        ///     A new instance of <see cref="DelegatingPayloadVariableExtractor" />.
        /// </returns>
        public virtual DelegatingPayloadVariableExtractor Build()
        {
            return new DelegatingPayloadVariableExtractor(this);
        }

        // Static entry method for builder.
        public static Builder FromBody()
        {
            return new Builder();
        }

        /// <summary>
        ///     Sets the namespaces for the builder using the provided dictionary of namespaces.
        /// </summary>
        /// <param name="namespaces">A dictionary of namespaces where the key is the prefix and the value is the namespace URI.</param>
        /// <returns>
        ///     An instance of the <see cref="Builder" /> class.
        /// </returns>
        public Builder Namespaces(Dictionary<string, string> namespaces)
        {
            foreach (var ns in namespaces)
            {
                _namespaces[ns.Key] = ns.Value;
            }

            return this;
        }

        /// <summary>
        ///     Adds a namespace with the specified prefix to the collection of namespaces.
        /// </summary>
        /// <param name="prefix">The prefix associated with the namespace.</param>
        /// <param name="nm">The namespace to be added.</param>
        /// <returns>
        ///     An instance of the <see cref="Builder" /> class.
        /// </returns>
        public Builder Namespace(string prefix, string nm)
        {
            _namespaces[prefix] = nm;
            return this;
        }
    }
}
