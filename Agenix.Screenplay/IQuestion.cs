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

using System.Globalization;
using Agenix.Screenplay.Questions.Converters;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a question within the screenplay pattern that can define how an actor retrieves information.
/// </summary>
/// <typeparam name="ANSWER">The type of answer that the question provides.</typeparam>
public interface IQuestion<ANSWER>
{
    string Subject => string.Empty;

    /// <summary>
    ///     Provides the answer to the question for the given actor.
    /// </summary>
    /// <param name="actor">The actor who will respond to the question.</param>
    /// <returns>The answer to the question as determined by the actor.</returns>
    ANSWER AnsweredBy(Actor actor);

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
    ///     Converts the answer of the current question into a boolean value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a boolean.</returns>
    IQuestion<bool> AsBoolean()
    {
        return About(Subject).AnsweredBy(actor =>
            bool.Parse(AnsweredBy(actor).ToString()));
    }


    /// <summary>
    ///     Negates the result of a boolean question.
    /// </summary>
    /// <param name="question">The boolean question whose result will be negated.</param>
    /// <returns>An IQuestion that represents the negation of the original question's answer.</returns>
    static IQuestion<bool> Not(IQuestion<bool> question)
    {
        return About(question.Subject).AnsweredBy(actor => !question.AnsweredBy(actor));
    }

    /// <summary>
    ///     Converts the answer of the current question into a string value.
    /// </summary>
    /// <returns>An IQuestion where the answer is represented as a string.</returns>
    IQuestion<string> AsString()
    {
        return About(Subject).AnsweredBy(actor => AnsweredBy(actor).ToString());
    }

    /// <summary>
    ///     Converts the answer of the current question into an integer value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as an integer.</returns>
    IQuestion<int> AsInteger()
    {
        return About(Subject).AnsweredBy(actor =>
            int.Parse(AnsweredBy(actor).ToString()));
    }

    /// <summary>
    ///     Converts the answer of the current question into a double value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a double.</returns>
    IQuestion<double> AsDouble()
    {
        return About(Subject).AnsweredBy(actor =>
            double.Parse(AnsweredBy(actor).ToString()));
    }

    /// <summary>
    ///     Converts the answer of the current question into a float value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a float.</returns>
    IQuestion<float> AsFloat()
    {
        return About(Subject).AnsweredBy(actor =>
            float.Parse(AnsweredBy(actor).ToString()));
    }

    /// <summary>
    ///     Converts the answer of the current question into a long value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a long.</returns>
    IQuestion<long> AsLong()
    {
        return About(Subject).AnsweredBy(actor =>
            long.Parse(AnsweredBy(actor).ToString()));
    }

    /// <summary>
    ///     Converts the answer of the current question into a decimal value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a decimal.</returns>
    IQuestion<decimal> AsDecimal()
    {
        return About(Subject).AnsweredBy(actor =>
            decimal.Parse(AnsweredBy(actor).ToString()));
    }

    /// <summary>
    ///     Converts the answer of the current question into a DateTime value.
    /// </summary>
    /// <returns>An IQuestion where the answer is parsed as a DateTime.</returns>
    IQuestion<DateTime> AsDate()
    {
        return About(Subject).AnsweredBy(actor =>
            DateTime.Parse(AnsweredBy(actor).ToString()));
    }

    /// <summary>
    ///     Converts the answer to a DateTime object based on the specified date format.
    /// </summary>
    /// <param name="format">The date format to use for parsing the answer.</param>
    /// <returns>A question that provides the parsed DateTime answer.</returns>
    IQuestion<DateTime> AsDate(string format)
    {
        return About(Subject).AnsweredBy(actor =>
            DateTime.ParseExact(
                AnsweredBy(actor).ToString(),
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None));
    }

    /// <summary>
    ///     Converts the answer of the current question into an enumerated value of the specified type.
    /// </summary>
    /// <typeparam name="T">The enum type to which the answer will be converted.</typeparam>
    /// <returns>An IQuestion where the answer is parsed as the specified enum type.</returns>
    IQuestion<T> AsEnum<T>() where T : struct, Enum
    {
        return About(Subject).AnsweredBy(actor =>
            Enum.Parse<T>(AnsweredBy(actor).ToString(), true));
    }

    /// <summary>
    ///     Convert the answer to a question into another form using an arbitrary function.
    /// </summary>
    /// <typeparam name="T">The target type of the transformation</typeparam>
    /// <param name="transformer">The function to transform the answer</param>
    /// <returns>A new question with the transformed answer type</returns>
    IQuestion<T> Map<T>(Func<ANSWER, T> transformer)
    {
        return About(Subject).AnsweredBy(actor => transformer(AnsweredBy(actor)));
    }

    /// <summary>
    ///     Convert all the matching answers to a question into another form using an arbitrary function.
    /// </summary>
    /// <typeparam name="T">The target type for each transformed element</typeparam>
    /// <param name="transformer">The function to transform each string value</param>
    /// <returns>A new question returning a collection of transformed values</returns>
    Func<Actor, ICollection<T>> MapEach<T>(Func<string, T> transformer)
    {
        return actor => ((IList<string>)AnsweredBy(actor))
            .Select(transformer)
            .ToList();
    }

    /// <summary>
    ///     Returns a new question with the specified text as a subject.
    /// </summary>
    /// <param name="description">The description to use as the subject</param>
    /// <returns>A new question with the same behavior but different description</returns>
    IQuestion<ANSWER> DescribedAs(string description)
    {
        return About(description).AnsweredBy(AnsweredBy);
    }

    /// <summary>
    ///     Converts the answer to a specified type using a default converter.
    /// </summary>
    /// <returns>A new question that converts its answer to the specified type</returns>
    IQuestion<T> As<T>()
    {
        return QuestionExtensions.As<ANSWER, T>(this);
    }

    /// <summary>
    ///     Converts the answer of the current question into a list of the specified type.
    /// </summary>
    /// <param name="type">The target type of the elements in the resulting list.</param>
    /// <typeparam name="T">The type of the elements in the resulting list.</typeparam>
    /// <returns>An IQuestion that resolves to a list of elements of the specified type.</returns>
    IQuestion<List<T>> AsListOf<T>(Type type)
    {
        return new ListConvertingQuestion<ANSWER, T>(this, type);
    }

    /// <summary>
    ///     Converts the answer of the current question into a collection of the specified target type.
    /// </summary>
    /// <typeparam name="T">The target type of the elements in the resulting collection.</typeparam>
    /// <param name="type">The .NET type representing the target collection's item type.</param>
    /// <returns>A question that produces a collection of the specified target type when answered by an actor.</returns>
    IQuestion<ICollection<T>> AsCollectionOf<T>(Type type)
    {
        return new CollectionConvertingQuestion<ANSWER, T>(this, type);
    }

    /// <summary>
    ///     Provides extension methods for the <see cref="IQuestion{T}" /> interface.
    /// </summary>
    public static class QuestionExtensions
    {
        public static IQuestion<TTarget> As<TSource, TTarget>(IQuestion<TSource> question)
        {
            return new TypeConvertingQuestion<TSource, TTarget>(question);
        }

        private class TypeConvertingQuestion<TSource, TTarget>(IQuestion<TSource> sourceQuestion) : IQuestion<TTarget>
        {
            public TTarget AnsweredBy(Actor actor)
            {
                var sourceValue = sourceQuestion.AnsweredBy(actor);
                return (TTarget)DefaultConverters.ConverterFor<object>(typeof(TTarget)).Convert(sourceValue);
            }
        }
    }

    /// <summary>
    ///     Represents a question that converts the result of a source question into a list of a specified target type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source question's answer.</typeparam>
    /// <typeparam name="TTarget">The desired type of the elements in the converted list.</typeparam>
    public class ListConvertingQuestion<TSource, TTarget> : IQuestion<List<TTarget>>
    {
        private readonly IQuestion<TSource> _sourceQuestion;
        private readonly Type _targetType;

        public ListConvertingQuestion(IQuestion<TSource> sourceQuestion, Type targetType)
        {
            _sourceQuestion = sourceQuestion;
            _targetType = targetType;
        }

        public List<TTarget> AnsweredBy(Actor actor)
        {
            var sourceValue = _sourceQuestion.AnsweredBy(actor);
            var list = sourceValue as IEnumerable<object>;
            return list?
                .Select(value => (TTarget)DefaultConverters.ConverterFor<object>(_targetType).Convert(value))
                .ToList();
        }
    }

    /// <summary>
    ///     Represents a question that converts a source question's answer into a collection of a specified target type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source question's answer.</typeparam>
    /// <typeparam name="TTarget">The target type into which each element in the source collection is converted.</typeparam>
    private class CollectionConvertingQuestion<TSource, TTarget>(IQuestion<TSource> sourceQuestion, Type targetType)
        : IQuestion<ICollection<TTarget>>
    {
        public ICollection<TTarget> AnsweredBy(Actor actor)
        {
            var sourceValue = sourceQuestion.AnsweredBy(actor);
            var collection = sourceValue as IEnumerable<object>;
            return collection?
                .Select(value => (TTarget)DefaultConverters.ConverterFor<object>
                    (targetType).Convert(value))
                .ToList();
        }
    }
}
