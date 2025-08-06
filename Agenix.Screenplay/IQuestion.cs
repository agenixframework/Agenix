#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections;
using System.Globalization;
using Agenix.Screenplay.Questions.Converters;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a question within the screenplay pattern that can define how an actor retrieves information.
/// </summary>
/// <typeparam name="TAnswer">The type of answer that the question provides.</typeparam>
public interface IQuestion<TAnswer>
{
    /// <summary>
    ///     Represents the subject of a question, providing a descriptive label or identifier.
    /// </summary>
    /// <remarks>
    ///     This property is useful for describing the content or purpose of a question,
    ///     allowing it to be identified or referenced in a meaningful way.
    /// </remarks>
    string Subject => string.Empty;

    /// <summary>
    ///     Creates a question using the specified function to define how an actor retrieves information.
    /// </summary>
    /// <param name="function">A function that takes an actor and returns a value of the specified type.</param>
    /// <typeparam name="T">The type of answer that the created question provides.</typeparam>
    /// <returns>An instance of a question based on the provided function.</returns>
    static IQuestion<T> Create<T>(Func<Actor, T> function)
    {
        return new LambdaQuestion<T>(function);
    }

    /// <summary>
    ///     Provides the answer to the question for the given actor.
    /// </summary>
    /// <param name="actor">The actor who will respond to the question.</param>
    /// <returns>The answer to the question as determined by the actor.</returns>
    Task<TAnswer> AnsweredBy(Actor actor);

    /// <summary>
    ///     Creates a QuestionBuilder instance with the specified subject.
    /// </summary>
    /// <param name="subject">The subject for the question being created.</param>
    /// <returns>A QuestionBuilder instance to construct questions related to the specified subject.</returns>
    static QuestionBuilder About(string subject)
    {
        return new QuestionBuilder(subject);
    }

    /// <summary>
    ///     Converts the result of the current question into a boolean value.
    /// </summary>
    /// <returns>A question that evaluates and returns the boolean representation of the original answer.</returns>
    Task<IQuestion<bool>> AsBoolean()
    {
        var booleanQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var original = await AnsweredBy(actor).ConfigureAwait(false);

            switch (original)
            {
                case bool b:
                    return b;
                case string s when bool.TryParse(s, out var parsed):
                    return parsed;
                default:
                    {
                        var text = original?.ToString();
                        return bool.TryParse(text, out var parsed2) && parsed2;
                    }
            }
        });

        return booleanQuestion;
    }

    /// <summary>
    ///     Negates the result of a boolean question.
    /// </summary>
    /// <param name="question">The boolean question whose result will be negated.</param>
    /// <returns>An IQuestion that represents the negation of the original question's answer.</returns>
    static Task<IQuestion<bool>> Not(IQuestion<bool> question)
    {
        var negated = About(question.Subject).AnsweredBy(async actor =>
        {
            var value = await question.AnsweredBy(actor).ConfigureAwait(false);
            return !value;
        });

        return negated;
    }


    /// <summary>
    ///     Converts the answer of the current question into a string value.
    /// </summary>
    /// <returns>An IQuestion where the answer is represented as a string.</returns>
    Task<IQuestion<string>> AsString()
    {
        return About(Subject).AnsweredBy<string>(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);
            return value?.ToString() ?? string.Empty;
        });
    }

    /// <summary>
    ///     Converts the answer of the current question into an integer value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as an integer.</returns>
    Task<IQuestion<int>> AsInteger()
    {
        var intQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);
            if (value is int i)
            {
                return i;
            }

            var text = value?.ToString();
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : throw new ArgumentException($"Could not parse {text} as an integer.");
        });

        return intQuestion;
    }


    /// <summary>
    ///     Converts the answer of the current question into a double value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a double.</returns>
    Task<IQuestion<double>> AsDouble()
    {
        var doubleQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            switch (value)
            {
                case double d:
                    return d;
                // Prefer culture-invariant parsing for numeric values
                case IFormattable formattable:
                    {
                        var s = formattable.ToString(null, CultureInfo.InvariantCulture);
                        return double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out var parsedFromFormattable)
                            ? parsedFromFormattable
                            : throw new ArgumentException($"Could not parse {s} as a double.");
                    }
                default:
                    {
                        var text = value?.ToString();
                        return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out var parsed)
                            ? parsed
                            : throw new ArgumentException($"Could not parse {text} as a double.");
                    }
            }
        });

        return doubleQuestion;
    }


    /// <summary>
    ///     Converts the answer of the current question into a float value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a float.</returns>
    Task<IQuestion<float>> AsFloat()
    {
        var floatQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            switch (value)
            {
                case float f:
                    return f;
                case IFormattable formattable:
                    {
                        var s = formattable.ToString(null, CultureInfo.InvariantCulture);
                        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out var parsedFromFormattable)
                            ? parsedFromFormattable
                            : throw new ArgumentException($"Could not parse {s} as a float.");
                    }
                default:
                    {
                        var text = value?.ToString();
                        return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out var parsed)
                            ? parsed
                            : throw new ArgumentException($"Could not parse {text} as a float.");
                    }
            }
        });

        return floatQuestion;
    }

    /// <summary>
    ///     Converts the answer of the current question into a long value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a long.</returns>
    Task<IQuestion<long>> AsLong()
    {
        var longQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            switch (value)
            {
                case long l:
                    return l;
                case IFormattable formattable:
                    {
                        var s = formattable.ToString(null, CultureInfo.InvariantCulture);
                        return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out var parsedFromFormattable)
                            ? parsedFromFormattable
                            : throw new ArgumentException($"Could not parse {s} as a long.");
                    }
                default:
                    {
                        var text = value?.ToString();
                        return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                            ? parsed
                            : throw new ArgumentException($"Could not parse {text} as a long.");
                    }
            }
        });

        return longQuestion;
    }

    /// <summary>
    ///     Converts the answer of the current question into a decimal value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a decimal.</returns>
    Task<IQuestion<decimal>> AsDecimal()
    {
        return About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            if (value is decimal d)
            {
                return d;
            }

            if (value is IFormattable formattable)
            {
                var s = formattable.ToString(null, CultureInfo.InvariantCulture);
                return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : throw new ArgumentException($"Could not parse {s} as a decimal.");
            }

            var text = value?.ToString();
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed2)
                ? parsed2
                : throw new ArgumentException($"Could not parse {text} as a decimal.");
        });
    }


    /// <summary>
    ///     Converts the answer of the current question into a DateTime value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a DateTime.</returns>
    Task<IQuestion<DateTime>> AsDate()
    {
        var dateQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            switch (value)
            {
                case DateTime dt:
                    return dt;
                case IFormattable formattable:
                    {
                        var s = formattable.ToString(null, CultureInfo.InvariantCulture);
                        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                            out var parsedFromFormattable)
                            ? parsedFromFormattable
                            : throw new FormatException($"Unable to parse DateTime from '{s}'.");
                    }
            }

            var text = value?.ToString();
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : throw new FormatException($"Unable to parse DateTime from '{text ?? "<null>"}'.");
        });

        return dateQuestion;
    }


    /// <summary>
    ///     Converts the answer to a DateTime object based on the specified date format.
    /// </summary>
    /// <param name="format">The date format to use for parsing the answer.</param>
    /// <returns>A question that provides the parsed DateTime answer.</returns>
    Task<IQuestion<DateTime>> AsDate(string format)
    {
        return About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            if (value is DateTime dt)
            {
                return dt;
            }

            if (value is string s)
            {
                return DateTime.ParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }

            if (value is IFormattable formattable)
            {
                var text = formattable.ToString(null, CultureInfo.InvariantCulture);
                return DateTime.ParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }

            var fallback = value?.ToString() ?? string.Empty;
            return DateTime.ParseExact(fallback, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
        });
    }


    /// <summary>
    ///     Converts the answer of the current question into an enumerated value of the specified type.
    /// </summary>
    /// <typeparam name="T">The enum type to which the answer will be converted.</typeparam>
    /// <returns>An IQuestion where the answer is parsed as the specified enum type.</returns>
    Task<IQuestion<T>> AsEnum<T>() where T : struct, Enum
    {
        var enumQuestion = About(Subject).AnsweredBy(async actor =>
        {
            var value = await AnsweredBy(actor).ConfigureAwait(false);

            switch (value)
            {
                // Already the target enum
                case T alreadyEnum:
                    return alreadyEnum;
                // Parse from string name (case-insensitive)
                case string s when Enum.TryParse<T>(s, true, out var parsedByName):
                    return parsedByName;
            }

            // Parse from numeric value using underlying type
            try
            {
                var underlyingType = Enum.GetUnderlyingType(typeof(T));
                var numeric = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                return (T)Enum.ToObject(typeof(T), numeric!);
            }
            catch
            {
                // ignore and fallback
            }

            // Final fallback: use ToString() and try parsing
            var text = value?.ToString();
            if (!string.IsNullOrEmpty(text) && Enum.TryParse<T>(text, true, out var parsed))
            {
                return parsed;
            }

            throw new ArgumentException($"Unable to convert value to enum {typeof(T).Name}.");
        });

        return enumQuestion;
    }


    /// <summary>
    ///     Convert the answer to a question into another form using an arbitrary function.
    /// </summary>
    /// <typeparam name="T">The target type of the transformation</typeparam>
    /// <param name="transformer">The function to transform the answer</param>
    /// <returns>A new question with the transformed answer type</returns>
    Task<IQuestion<T>> Map<T>(Func<TAnswer, T> transformer)
    {
        var mapped = About(Subject).AnsweredBy<T>(async actor =>
        {
            var source = await AnsweredBy(actor).ConfigureAwait(false);
            return transformer(source);
        });

        return mapped;
    }


    /// <summary>
    ///     Convert all the matching answers to a question into another form using an arbitrary function.
    /// </summary>
    /// <typeparam name="T">The target type for each transformed element</typeparam>
    /// <param name="transformer">The function to transform each string value</param>
    /// <returns>An async function returning a collection of transformed values</returns>
    Func<Actor, Task<ICollection<T>>> MapEach<T>(Func<string, T> transformer)
    {
        return async actor =>
        {
            var source = await AnsweredBy(actor).ConfigureAwait(false) as IList<string>;
            return source?.Select(transformer).ToList() ?? new List<T>();
        };
    }

    /// <summary>
    ///     Returns a new question with the specified text as a subject.
    /// </summary>
    /// <param name="description">The description to use as the subject</param>
    /// <returns>A new question with the same behavior but different description</returns>
    Task<IQuestion<TAnswer>> DescribedAs(string description)
    {
        return About(description).AnsweredBy(AnsweredBy);
    }

    /// <summary>
    ///     Converts the answer to a specified type using a default converter.
    /// </summary>
    /// <returns>A new question that converts its answer to the specified type</returns>
    IQuestion<T> As<T>()
    {
        return QuestionExtensions.As<TAnswer, T>(this);
    }

    /// <summary>
    ///     Converts the answer of the current question into a list of the specified type.
    /// </summary>
    /// <param name="type">The target type of the elements in the resulting list.</param>
    /// <typeparam name="T">The type of the elements in the resulting list.</typeparam>
    /// <returns>An IQuestion that resolves to a list of elements of the specified type.</returns>
    IQuestion<List<T>> AsListOf<T>(Type type)
    {
        return new ListConvertingQuestion<TAnswer, T>(this, type);
    }

    /// <summary>
    ///     Converts the answer of the current question into a collection of the specified target type.
    /// </summary>
    /// <typeparam name="T">The target type of the elements in the resulting collection.</typeparam>
    /// <param name="type">The .NET type representing the target collection's item type.</param>
    /// <returns>A question that produces a collection of the specified target type when answered by an actor.</returns>
    IQuestion<ICollection<T>> AsCollectionOf<T>(Type type)
    {
        return new CollectionConvertingQuestion<TAnswer, T>(this, type);
    }

    private sealed class LambdaQuestion<T>(Func<Actor, T> function) : IQuestion<T>
    {
        private readonly Func<Actor, T> _function = function ?? throw new ArgumentNullException(nameof(function));

        public Task<T> AnsweredBy(Actor actor)
        {
            return Task.FromResult(_function(actor));
        }
    }

    /// <summary>
    ///     Provides extension methods for the <see cref="IQuestion{T}" /> interface.
    /// </summary>
    static class QuestionExtensions
    {
        public static IQuestion<TTarget> As<TSource, TTarget>(IQuestion<TSource> question)
        {
            return new TypeConvertingQuestion<TSource, TTarget>(question);
        }

        private sealed class TypeConvertingQuestion<TSource, TTarget>(IQuestion<TSource> sourceQuestion)
            : IQuestion<TTarget>
        {
            public async Task<TTarget> AnsweredBy(Actor actor)
            {
                var sourceValue = await sourceQuestion.AnsweredBy(actor);
                return (TTarget)DefaultConverters.ConverterFor<object>(typeof(TTarget)).Convert(sourceValue);
            }
        }
    }

    /// <summary>
    ///     Represents a question that converts the result of a source question into a list of a specified target type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source question's answer.</typeparam>
    /// <typeparam name="TTarget">The desired type of the elements in the converted list.</typeparam>
    class ListConvertingQuestion<TSource, TTarget> : IQuestion<List<TTarget>>
    {
        private readonly IQuestion<TSource> _sourceQuestion;
        private readonly Type _targetType;

        public ListConvertingQuestion(IQuestion<TSource> sourceQuestion, Type targetType)
        {
            _sourceQuestion = sourceQuestion;
            _targetType = targetType;
        }

        /// <summary>
        ///     Retrieves the answer to the question by executing it within the context of the specified actor.
        /// </summary>
        /// <param name="actor">The actor responsible for answering the question.</param>
        /// <returns>
        ///     A task representing the operation, with the result containing the answer of type
        ///     <typeparamref name="TAnswer" />.
        /// </returns>
        public async Task<List<TTarget>> AnsweredBy(Actor actor)
        {
            var sourceValue = await _sourceQuestion.AnsweredBy(actor);

            // Treat null as an empty list
            if (sourceValue is null)
            {
                return [];
            }

            var converter = DefaultConverters.ConverterFor<object>(_targetType);

            // If the answer is an enumerable (but not string), convert each element
            if (sourceValue is IEnumerable enumerable && sourceValue is not string)
            {
                return (from object? item in enumerable select (TTarget)converter.Convert(item)).ToList();
            }

            // Otherwise, convert a single value into a one-element list
            return [(TTarget)converter.Convert(sourceValue)];
        }
    }

    /// <summary>
    ///     Represents a question that converts a source question's answer into a collection of a specified target type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source question's answer.</typeparam>
    /// <typeparam name="TTarget">The target type into which each element in the source collection is converted.</typeparam>
    private sealed class CollectionConvertingQuestion<TSource, TTarget>(
        IQuestion<TSource> sourceQuestion,
        Type targetType)
        : IQuestion<ICollection<TTarget>>
    {
        public async Task<ICollection<TTarget>> AnsweredBy(Actor actor)
        {
            var sourceValue = await sourceQuestion.AnsweredBy(actor).ConfigureAwait(false);

            if (sourceValue is null)
            {
                return Array.Empty<TTarget>();
            }

            var converter = DefaultConverters.ConverterFor<object>(targetType);

            // If the answer is a collection (but not a string), convert each item
            if (sourceValue is IEnumerable enumerable && sourceValue is not string)
            {
                return (from object? item in enumerable select (TTarget)converter.Convert(item)).ToList();
            }

            // Otherwise, convert a single value into a one-element collection
            return new List<TTarget> { (TTarget)converter.Convert(sourceValue) };
        }
    }
}
