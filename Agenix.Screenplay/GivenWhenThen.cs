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

using Agenix.Screenplay.Consequence;
using Agenix.Screenplay.Questions;
using Agenix.Validation.NHamcrest.Validation.Matcher;
using NHamcrest;

namespace Agenix.Screenplay;

/// <summary>
///     Provides a set of static methods for creating and managing tasks or actions within the "Given-When-Then" style of
///     behavior-driven development context.
/// </summary>
/// <remarks>
///     The methods in this class facilitate fluent interaction patterns for defining actions, verifying expectations, and
///     asserting outcomes.
///     It is primarily used within the Screenplay pattern to enable describing interactions in a semantically meaningful
///     way.
/// </remarks>
public static class GivenWhenThen
{
    /// <summary>
    ///     Provides a starting point for an actor to perform tasks or actions, enabling a "Given-When-Then" style of
    ///     interaction.
    /// </summary>
    /// <typeparam name="T">
    ///     Represents an actor that implements the <see cref="IPerformsTasks" /> interface, allowing execution of tasks and
    ///     actions.
    /// </typeparam>
    /// <param name="actor">The actor who will perform tasks or actions in the screenplay context.</param>
    /// <returns>The same actor instance, enabling method chaining or fluent interaction patterns.</returns>
    public static T GivenThat<T>(T actor) where T : IPerformsTasks
    {
        return actor;
    }

    /// <summary>
    ///     Allows an actor to continue performing tasks or actions in the screenplay context, extending a sequence with
    ///     additional steps.
    /// </summary>
    /// <param name="actor">The actor who will continue to perform tasks or actions in the screenplay context.</param>
    /// <returns>The same actor instance, enabling method chaining or fluent interaction patterns.</returns>
    public static Actor AndThat(Actor actor) { return actor; }

    /// <summary>
    ///     Represents the "When" step in a screenplay-style interaction, defining the actions or tasks the actor performs.
    /// </summary>
    /// <param name="actor">The actor who will perform actions or tasks within the screenplay context.</param>
    /// <returns>The same actor instance, allowing method chaining for subsequent interactions or validations.</returns>
    public static Actor When(Actor actor) { return actor; }

    /// <summary>
    ///     Allows an actor to continue a sequence of tasks or actions in a screenplay-based testing workflow.
    /// </summary>
    /// <param name="actor">The actor performing tasks or actions in the screenplay context.</param>
    /// <returns>The same actor instance, enabling method chaining or fluent interaction patterns.</returns>
    public static Actor And(Actor actor) { return actor; }

    /// <summary>
    ///     Represents an alternative or contrasting point in a sequence of actions or tasks, providing the ability to branch
    ///     the narrative for the given actor.
    /// </summary>
    /// <param name="actor">The actor for whom the alternative point in the sequence is defined.</param>
    /// <returns>The same actor instance, allowing for continued actions or chaining within the screenplay context.</returns>
    public static Actor But(Actor actor) { return actor; }

    /// <summary>
    ///     Verifies that a given actual value satisfies the specified matcher condition in the context of the screenplay
    ///     pattern.
    ///     Throws an assertion exception if the condition is not met.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the actual value being validated.
    /// </typeparam>
    /// <param name="actual">The actual value to be matched against the provided matcher.</param>
    /// <param name="matcher">The matcher that specifies the condition to be satisfied by the actual value.</param>
    public static void Then<T>(T actual, IMatcher<T> matcher)
    {
        MatcherAssert.AssertThat(actual, matcher);
    }

    /// <summary>
    ///     Defines the final step in a "Given-When-Then" interaction pattern, used to assert outcomes or conclude actions.
    /// </summary>
    /// <param name="actor">The actor who will verify results or conclude actions in the screenplay interaction.</param>
    /// <returns>The same actor instance, enabling continuous interaction after the final step if required.</returns>
    public static Actor Then(Actor actor) { return actor; }

    /// <summary>
    ///     Verifies that the result of a given question matches the expected outcome, evaluating a consequence in the
    ///     Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of value that the question resolves to and is evaluated against the expected matcher.
    /// </typeparam>
    /// <param name="actual">The question of whose result will be evaluated.</param>
    /// <param name="expected">The matcher that defines the expected outcome for the given question.</param>
    /// <returns>
    ///     A consequence that can be used to assert the outcome of the evaluation, allowing further consequences or
    ///     complaints.
    /// </returns>
    public static IConsequence<T> SeeThat<T>(IQuestion<T> actual, IMatcher<T> expected)
    {
        return new QuestionConsequence<T>(actual, expected);
    }

    /// <summary>
    ///     Evaluates a specific question against an expected result or condition using the Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">The type of the answer provided by the question.</typeparam>
    /// <param name="actual">The question to be evaluated, which will provide an answer based on the actor's context.</param>
    /// <param name="expected">The condition to match or predicate to validate against the answer provided by the question.</param>
    /// <returns>A consequence that can be evaluated to determine if the condition or expectation is met.</returns>
    public static IConsequence<T> SeeThat<T>(IQuestion<T> actual, Func<T, bool> expected)
    {
        return new PredicateConsequence<T>(actual, expected);
    }

    /// <summary>
    ///     Evaluates a question's answer against a custom predicate and associates the evaluation with a specific subject,
    ///     enabling richer context and detailed feedback in the Screenplay interaction.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of value being evaluated, representing the answer to the question.
    /// </typeparam>
    /// <param name="subject">
    ///     The subject or description of the evaluation being performed, providing context for the assertion.
    /// </param>
    /// <param name="actual">
    ///     The question of whose answer is to be evaluated.
    /// </param>
    /// <param name="expected">
    ///     A custom predicate that determines whether the evaluation is successful.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="IConsequence{T}" /> that encapsulates the evaluation logic for the question's answer with
    ///     the provided subject.
    /// </returns>
    public static IConsequence<T> SeeThat<T>(string subject, IQuestion<T> actual, Func<T, bool> expected)
    {
        return new PredicateConsequence<T>(subject, actual, expected);
    }

    /// <summary>
    ///     Verifies whether an answer to a question matches an expected value within the context of the Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">The type of the answer provided by the question.</typeparam>
    /// <param name="subject">A textual description of the verification being performed.</param>
    /// <param name="actual">The question that provides the actual value to verify.</param>
    /// <param name="expected">The matcher or condition that determines whether the actual value meets the expected criteria.</param>
    /// <returns>A consequence that represents the result of the verification.</returns>
    public static IConsequence<T> SeeThat<T>(string subject, IQuestion<T> actual, IMatcher<T> expected)
    {
        return new QuestionConsequence<T>(subject, actual, expected);
    }

    /// <summary>
    ///     Evaluates a boolean-based question, encapsulating the result as a consequence
    ///     to be used within the Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">
    ///     The type associated with the consequence representing further evaluations or actions.
    /// </typeparam>
    /// <param name="actual">The boolean question to be evaluated, representing a specific condition or value.</param>
    /// <returns>
    ///     A consequence that encapsulates the evaluation of the given boolean question for further processing or assertions.
    /// </returns>
    public static IConsequence<T> SeeThat<T>(IQuestion<bool> actual)
    {
        return new BooleanQuestionConsequence<T>(actual);
    }

    /// <summary>
    ///     Evaluates whether the expected condition, derived from the result of a given question, is satisfied based on the
    ///     actual boolean value.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value being evaluated by the consequence.
    /// </typeparam>
    /// <param name="subject">The descriptive name or title of the consequence being evaluated.</param>
    /// <param name="actual">The question of whose boolean result will be evaluated.</param>
    /// <returns>A consequence that can be evaluated in the context of the Screenplay pattern.</returns>
    public static IConsequence<T> SeeThat<T>(string subject, IQuestion<bool> actual)
    {
        return new BooleanQuestionConsequence<T>(subject, actual);
    }

    /// <summary>
    ///     Defines a consequence to be evaluated based on the result of a question and one or more expected matchers.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value being questioned and matched.
    /// </typeparam>
    /// <param name="subject">
    ///     A descriptive label or subject for the consequence being evaluated.
    /// </param>
    /// <param name="actual">
    ///     The question being asked or evaluated, which provides the actual value.
    /// </param>
    /// <param name="expectedMatchers">
    ///     One or more matchers defining the expected outcome or conditions to validate the actual value.
    /// </param>
    /// <returns>
    ///     An array of consequences, each representing an evaluation of the actual value against an expected matcher.
    /// </returns>
    public static IConsequence<T>[] SeeThat<T>(string subject, IQuestion<T> actual,
        params IMatcher<T>[] expectedMatchers)
    {
        return expectedMatchers.Select(matcher => new QuestionConsequence<T>(subject, actual, matcher))
            .Cast<IConsequence<T>>().ToArray();
    }

    /// <summary>
    ///     Evaluates the outcome of a question against an expected condition, enabling verification of expected results within
    ///     the Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value returned by the question, which forms the basis of the verification.
    /// </typeparam>
    /// <param name="actual">The question that provides the actual value to be evaluated.</param>
    /// <param name="expected">A function that defines the expected condition or predicate to be met by the actual value.</param>
    /// <returns>An <see cref="IConsequence{T}" /> representing the verification or assertion of the expected condition.</returns>
    public static IConsequence<T> SeeThat<T>(Question<T> actual, Func<T, bool> expected)
    {
        return new PredicateConsequence<T>(actual, expected);
    }


    /// <summary>
    ///     Verifies the result of a specified question using one or more expected matchers, returning an array of consequences
    ///     to assert the outcomes.
    /// </summary>
    /// <typeparam name="T">The type of the value being questioned and matched.</typeparam>
    /// <param name="actual">The question that provides the actual value to be verified.</param>
    /// <param name="expectedMatchers">
    ///     An array of matchers used to validate that the actual value satisfies the expected
    ///     conditions.
    /// </param>
    /// <returns>An array of consequences that evaluate the validation between the actual value and the provided matchers.</returns>
    public static IConsequence<T>[] SeeThat<T>(IQuestion<T> actual, params IMatcher<T>[] expectedMatchers)
    {
        return ThereAreNo(expectedMatchers)
            ? ConsequenceGroupFor(actual)
            : ConsequencesForEachMatcher(actual, expectedMatchers);
    }

    /// <summary>
    ///     Creates a task for an actor to verify a specified condition, based on a question and its matcher.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the answer returned by the question being verified.
    /// </typeparam>
    /// <param name="question">
    ///     The question to be evaluated by the actor, producing a value to match against the specified matcher.
    /// </param>
    /// <param name="matcher">
    ///     The matcher used to validate the value produced by the question.
    /// </param>
    /// <returns>
    ///     A new task that evaluates the question and matcher, enabling the actor to validate the condition.
    /// </returns>
    public static ITask SeeIf<T>(IQuestion<T> question, IMatcher<T> matcher)
    {
        var consequence = SeeThat(question, matcher);
        return ITask.Where("See if " + question + " " + matcher,
            new AnonymousPerformableFunction("", actor => consequence.EvaluateFor(actor)));
    }

    /// <summary>
    ///     Creates a named predicate, associating a human-readable name with a given logical condition,
    ///     to enhance readability and debugging in the screenplay context.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the input to the predicate, representing the kind of object the predicate evaluates.
    /// </typeparam>
    /// <param name="name">The descriptive name for the predicate, providing context or relevance.</param>
    /// <param name="predicate">The logical condition to evaluate, defined as a delegate function.</param>
    /// <returns>
    ///     An instance of <see cref="NamedPredicate{T}" /> that encapsulates the named predicate logic.
    /// </returns>
    public static NamedPredicate<T> ReturnsAValueThat<T>(string name, Func<T, bool> predicate)
    {
        return new NamedPredicate<T>(name, predicate);
    }

    /// <summary>
    ///     Creates an eventual consequence that will wait for the specified consequence to be satisfied.
    /// </summary>
    /// <typeparam name="T">The type of the consequence.</typeparam>
    /// <param name="consequenceThatMightTakeSomeTime">The consequence that might take some time to be satisfied.</param>
    /// <returns>An EventualConsequence that can be configured with timeout settings.</returns>
    public static EventualConsequence<T> Eventually<T>(IConsequence<T> consequenceThatMightTakeSomeTime)
    {
        return new EventualConsequence<T>(consequenceThatMightTakeSomeTime);
    }

    private static bool ThereAreNo<T>(IMatcher<T>[] expectedMatchers)
    {
        return expectedMatchers.Length == 0;
    }

    private static IConsequence<T>[] ConsequenceGroupFor<T>(IQuestion<T> actual)
    {
        return [new ConsequenceGroup<T>(actual)];
    }

    private static IConsequence<T>[] ConsequencesForEachMatcher<T>(IQuestion<T> actual, IMatcher<T>[] expectedMatchers)
    {
        return expectedMatchers.Select(matcher => new QuestionConsequence<T>(actual, matcher)).Cast<IConsequence<T>>()
            .ToArray();
    }
}
