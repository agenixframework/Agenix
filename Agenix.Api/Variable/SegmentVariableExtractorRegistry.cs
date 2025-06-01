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

using System.Collections;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Variable;

/**
* Simple registry holding all available segment variable extractor implementations. Test context can ask this registry for
* the extractors managed by this registry to access variable content from the TestContext expressed by variable expressions.
* Registry provides all known {@link SegmentVariableExtractor}s.
*/
public class SegmentVariableExtractorRegistry
{
    /**
     * Segment variable extractor resource lookup path
     */
    private const string ResourcePath = "Extension/agenix/variable/extractor/segment";

    /// <summary>
    ///     Provides a logger instance for the SegmentVariableExtractorRegistry class.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SegmentVariableExtractorRegistry));

    /// <summary>
    ///     Type resolver used to locate and instantiate custom segment variable extractors
    ///     by performing resource path lookups within the application's classpath.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Holds the collection of segment variable extractors used to extract values from segment-based variable
    ///     representations.
    ///     Managed by the SegmentVariableExtractorRegistry class, this collection provides access to various
    ///     implementations of ISegmentVariableExtractor for processing segment variable expressions in context.
    /// </summary>
    private readonly List<ISegmentVariableExtractor> _segmentValueExtractors =
    [
        MapVariableExtractor.Instance,
        ObjectFieldValueExtractor.Instance
    ];

    public SegmentVariableExtractorRegistry()
    {
        _segmentValueExtractors.AddRange(Lookup());
    }

    /// <summary>
    ///     Provides access to the collection of initialized and registered segment variable extractors
    ///     within the SegmentVariableExtractorRegistry.
    /// </summary>
    public List<ISegmentVariableExtractor> SegmentValueExtractors => _segmentValueExtractors;

    /// Resolves all registered segment variable extractors from the resource path lookup. Scans the application context
    /// for implementations of the ISegmentVariableExtractor interface and retrieves their instances. If no extractor
    /// implementations are found, an empty collection is returned instead of throwing an exception.
    /// @return A collection of all segment variable extractor instances or an empty collection if none are found.
    /// /
    private static ICollection<ISegmentVariableExtractor> Lookup()
    {
        try
        {
            var extractors = TypeResolver.ResolveAll<ISegmentVariableExtractor>();
            return extractors.Values;
        }
        catch (AgenixSystemException e)
        {
            Log.LogWarning($"Failed to resolve segment variable extractor from resource '{ResourcePath}'");
        }

        return new List<ISegmentVariableExtractor>();
    }

    /// Base class for segment variable extractors providing a foundation for custom variable extraction implementations.
    /// This class ensures that an exception is thrown if no match is found during the extraction process.
    /// Subclasses must implement the logic for determining whether a specific segment can be extracted
    /// and for performing the actual extraction of values.
    /// /
    public abstract class AbstractSegmentVariableExtractor : ISegmentVariableExtractor
    {
        public abstract bool CanExtract(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher);

        public object ExtractValue(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher)
        {
            var matchedValue = DoExtractValue(testContext, obj, matcher);

            if (matchedValue == null)
            {
                HandleMatchFailure(matcher);
            }

            return matchedValue;
        }

        /// Handles a match failure by throwing an AgenixSystemException with an appropriate message
        /// specific to the failure encountered during segment variable extraction.
        /// <param name="matcher">
        ///     The VariableExpressionSegmentMatcher instance that contains details about the
        ///     segment variable expression and current match state.
        /// </param>
        private void HandleMatchFailure(VariableExpressionSegmentMatcher matcher)
        {
            string exceptionMessage;
            if (matcher.TotalSegmentCount == 1)
            {
                exceptionMessage = $"Unknown variable '{matcher.VariableExpression}'";
            }
            else
            {
                if (matcher.SegmentIndex == 1)
                {
                    exceptionMessage = $"Unknown variable for first segment '{matcher.SegmentExpression}' " +
                                       $"of variable expression '{matcher.VariableExpression}'";
                }
                else
                {
                    exceptionMessage = $"Unknown segment-value for segment '{matcher.SegmentExpression}' " +
                                       $"of variable expression '{matcher.VariableExpression}'";
                }
            }

            throw new AgenixSystemException(exceptionMessage);
        }

        protected abstract object DoExtractValue(TestContext testContext, object obj,
            VariableExpressionSegmentMatcher matcher);
    }

    /// Abstract base class for segment variable extractors that work with indexed values.
    /// Provides the core functionality for handling variable extraction where indexing may be applied.
    /// Implementations of this class are required to define how the indexed values are specifically extracted.
    public abstract class IndexedSegmentVariableExtractor : AbstractSegmentVariableExtractor
    {
        protected override object DoExtractValue(TestContext testContext, object obj,
            VariableExpressionSegmentMatcher matcher)
        {
            var extractedValue = DoExtractIndexedValue(testContext, obj, matcher);

            if (matcher.SegmentIndex != -1)
            {
                extractedValue = GetIndexedElement(matcher, extractedValue);
            }

            return extractedValue;
        }

        /// Retrieves an indexed element from the provided indexed value based on the segment information
        /// contained within the specified matcher. If the provided value is not an array, an exception is thrown.
        /// <param name="matcher">The matcher containing segment-specific details such as the index to retrieve.</param>
        /// <param name="indexedValue">The indexed value (expected to be an array) from which an element will be extracted.</param>
        /// <returns>The element from the indexed value corresponding to the specified index in the matcher.</returns>
        /// <exception cref="Agenix.Api.Exceptions.AgenixSystemException">
        ///     Thrown when the provided indexed value is not of array type.
        /// </exception>
        private object GetIndexedElement(VariableExpressionSegmentMatcher matcher, object indexedValue)
        {
            if (indexedValue.GetType().IsArray)
            {
                return ((Array)indexedValue).GetValue(matcher.SegmentIndex);
            }

            throw new AgenixSystemException(
                $"Expected an instance of Array type. Cannot retrieve indexed property {matcher.SegmentExpression} from {indexedValue.GetType().Name}");
        }

        /**
     * Extract the indexed value from the object
     *
     * @param object
     * @param matcher
     * @return
     */
        protected abstract object DoExtractIndexedValue(TestContext testContext, object obj,
            VariableExpressionSegmentMatcher matcher);
    }

    /// A segment variable extractor implementation that retrieves a field value from a parent object.
    /// This extractor accesses the value based on the specific field name defined in the variable segment expression.
    /// Responsible for resolving object fields during the variable extraction process within a test context.
    /// Extends the capabilities of IndexedSegmentVariableExtractor and ensures compatibility
    /// with complex object structures that include fields as their data sources.
    public class ObjectFieldValueExtractor : IndexedSegmentVariableExtractor
    {
        public static readonly ObjectFieldValueExtractor Instance = new();

        private ObjectFieldValueExtractor()
        {
            // singleton
        }

        protected override object DoExtractIndexedValue(TestContext testContext, object parentObject,
            VariableExpressionSegmentMatcher matcher)
        {
            var field = ReflectionHelper.FindField(parentObject.GetType(), matcher.SegmentExpression);
            if (field == null)
            {
                throw new AgenixSystemException(
                    $"Failed to get variable - unknown field '{matcher.SegmentExpression}' on type {parentObject.GetType().Name}");
            }

            return ReflectionHelper.GetField(field, parentObject);
        }

        public override bool CanExtract(TestContext testContext, object? obj, VariableExpressionSegmentMatcher matcher)
        {
            return obj != null && obj is not string;
        }
    }

    /// SegmentVariableExtractor implementation that retrieves a segment value from a given IDictionary object.
    /// This class uses the segment expression as the key to access the corresponding value within the map.
    /// Designed to work with variable expressions within a testing context.
    public class MapVariableExtractor : IndexedSegmentVariableExtractor
    {
        public static readonly MapVariableExtractor Instance = new();

        private MapVariableExtractor()
        {
            // singleton
        }

        protected override object? DoExtractIndexedValue(TestContext testContext, object parentObject,
            VariableExpressionSegmentMatcher matcher)
        {
            object? matchedValue = null;
            if (parentObject is IDictionary map)
            {
                matchedValue = map[matcher.SegmentExpression];
            }

            return matchedValue;
        }

        public override bool CanExtract(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher)
        {
            return obj is IDictionary;
        }
    }
}
