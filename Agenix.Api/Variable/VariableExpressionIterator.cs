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

namespace Agenix.Api.Variable;

/**
 * This {@link Iterator} uses a regular expression pattern to match individual
 * segments of a variableExpression. Each segment of a variable expression
 * represents a bean, either stored as a variable in the TestContext (first
 * segment) or a property of the previous bean (all other segments). The
 * iterator provides VariableSegments, which provide the name and an optional
 * index as well as the variable/property value corresponding to the segment.
 * <p>
 *     Example:
 *     <b>var1.persons[2].firstnames[0]</b>
 * </p>
 * The iterator will provide the following VariableSegments for this expression
 * <ol>
 *     <li>the variable with name <b>var1</b> from the TestContext</li>
 *     <li>the third element of the <b>persons</b> property of the variable retrieved in the previous step</li>
 *     <li>the first element of the <b>firstnames</b> property of the property retrieved in the previous step</li>
 * </ol>
 */
public class VariableExpressionIterator : IEnumerator<VariableExpressionIterator.VariableSegment>
{
    /// Defines the mechanism for matching and segmenting variable expressions.
    /// /
    private readonly VariableExpressionSegmentMatcher _matcher;

    /// Represents a collection of segment value extractors used to determine and extract
    /// the values of specific segments within a variable expression.
    private readonly List<ISegmentVariableExtractor> _segmentValueExtractors;

    /// Represents the context information used during the variable expression iteration.
    /// Provides necessary execution context and supporting operations for extracting and resolving variables.
    private readonly TestContext _testContext;

    /// Represents the next segment in the iteration process, used to look ahead
    /// and determine the availability of subsequent segments.
    private VariableSegment _nextSegment;

    public VariableExpressionIterator(string variableExpression, TestContext testContext,
        List<ISegmentVariableExtractor> segmentValueExtractors)
    {
        _testContext = testContext;
        _segmentValueExtractors = segmentValueExtractors;

        _matcher = new VariableExpressionSegmentMatcher(variableExpression);

        if (_matcher.NextMatch())
            _nextSegment = CreateSegmentValue(testContext.GetVariables());
        else
            throw new AgenixSystemException($"Cannot match a segment on variableExpression: {variableExpression}");
    }

    /// Advances the iterator to the next element in the collection.
    /// @return True if the iterator successfully advanced to the next element; otherwise, false if the end of the collection is reached.
    /// /
    public bool MoveNext()
    {
        return _nextSegment != null;
    }

    /// <summary>
    ///     Gets the current element in the collection during the iteration.
    /// </summary>
    /// <remarks>
    ///     This property returns the current instance of <see cref="VariableSegment" /> that the iterator
    ///     is currently pointing to. If the end of the collection has been reached or the iteration has
    ///     not started, accessing this property may throw an exception depending on the implementation
    ///     of the iterator.
    /// </remarks>
    public VariableSegment Current
    {
        get
        {
            var currentSegment = _nextSegment;

            // Look ahead next segment
            _nextSegment = _matcher.NextMatch() ? CreateSegmentValue(currentSegment.SegmentValue) : null;

            return currentSegment;
        }
    }

    object IEnumerator.Current => Current;

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
    }

    /// Creates a new segment value based on the provided parent value and the current match context.
    /// <param name="parentValue">The parent value to use as a reference for segment extraction.</param>
    /// <return>The created VariableSegment, containing the segment expression, its index, and the extracted segment value.</return>
    /// /
    private VariableSegment CreateSegmentValue(object parentValue)
    {
        var segmentValue = _segmentValueExtractors.FirstOrDefault(extractor => extractor.CanExtract(_testContext,
            parentValue, _matcher))?.ExtractValue(_testContext, parentValue, _matcher);
        return new VariableSegment(_matcher.SegmentExpression, _matcher.SegmentIndex, segmentValue);
    }

    /// Retrieves the value of the last segment from a variable expression iterator.
    /// <param name="variableExpression">The variable expression to be processed.</param>
    /// <param name="testContext">The test context used during variable evaluation.</param>
    /// <param name="extractors">A list of segment value extractors used to extract values from segments.</param>
    /// <return>Returns the value of the last evaluated segment, or null if no segments are present.</return>
    /// /
    public static object GetLastExpressionValue(string variableExpression, TestContext testContext,
        List<ISegmentVariableExtractor> extractors)
    {
        VariableSegment segment = null;
        var iterator = new VariableExpressionIterator(variableExpression, testContext, extractors);
        while (iterator.MoveNext()) segment = iterator.Current;

        return segment != null ? segment.SegmentValue : null;
    }

    /// Represents a distinct segment of a variable expression as handled within the
    /// {@code VariableExpressionIterator}. Each segment corresponds to a portion of an
    /// expression and contains its name, optional index, and associated value.
    /// Instances of this class encapsulate three elements of an expression:
    /// 1. {@code Name}: The name of the segment, which identifies the variable or property.
    /// 2. {@code Index}: An optional integer that indicates the index of the element
    /// when the segment refers to a collection or array.
    /// 3. {@code SegmentValue}: The value associated with the segment, resolved through
    /// the provided TestContext and any extractors applied by the iterator.
    /// /
    public class VariableSegment(string name, int index, object segmentValue)
    {
        /// <summary>
        ///     The name of the variable or property associated with this segment of the variable expression.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        ///     The index of the segment within the variable expression, representing its position relative to other segments.
        /// </summary>
        public int Index { get; } = index;

        /// Represents the specific value associated with a segment in the
        /// variable expression being iterated. This value can be an object
        /// derived based on the segment's context and its related extractors.
        public object SegmentValue { get; } = segmentValue;
    }
}
