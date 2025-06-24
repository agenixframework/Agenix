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

using System.Configuration;

namespace Agenix.Screenplay.Cast;

/// <summary>
///     The stage is used to keep track of the actors taking part in a Screenplay test.
///     It is useful if you don't keep track of the actors explicitly but just refer to them by name, as is often done
///     in SpecFlow/ ReqnRoll scenarios.
///     Actors can be referred to by name (which must be unique for a given actor) or a pronoun.
///     The default pronouns are "he","she","they" and "it", and they are used interchangeably - any pronoun will always
///     refer to the last named actor who performed some action.
///     Pronouns can be configured using the screenplay.pronouns configuration setting, e.g.,
///     screenplay.pronouns = il,elle
///     The current stage is kept as an AsyncLocal object, so if you have multiple threads in the same Screenplay test,
///     you need to propagate the stage to each new thread using the SetTheStage() method.
/// </summary>
public static class OnStage
{
    private const string DefaultPronouns = "he,she,they,it,his,her,their,its";
    private const string A_New_Actor = "An actor";

    /// <summary>
    ///     Represents an asynchronous local variable used to manage the stage within the OnStage context.
    ///     This variable encapsulates the current <see cref="Stage" /> instance and facilitates state isolation
    ///     in asynchronous operations.
    /// </summary>
    private static readonly AsyncLocal<Stage> Stage = new();

    /// <summary>
    ///     Set the stage before calling the actors
    /// </summary>
    public static Stage SetTheStage(Casting cast)
    {
        Stage.Value = new Stage(cast);
        return GetStage();
    }

    /// <summary>
    ///     Check whether the stage has already been set.
    /// </summary>
    public static bool TheStageIsSet()
    {
        return Stage.Value != null;
    }

    /// <summary>
    ///     Set the stage to a specific stage object.
    ///     This is rarely needed but sometimes comes in handy when running tasks in parallel.
    /// </summary>
    public static Stage SetTheStage(Stage stage)
    {
        Stage.Value = stage;
        return GetStage();
    }

    /// <summary>
    ///     Returns an actor with a given name, creating a new actor if the actor is not already on stage.
    ///     If a pronoun is used (e.g "she creates a new account") then the current actor in the spotlight will be used.
    /// </summary>
    public static Actor TheActorCalled(string requiredActor)
    {
        var theActorIsReferredToByAPronoun = GetPronouns().Any(pronoun =>
            pronoun.Equals(requiredActor, StringComparison.OrdinalIgnoreCase));

        if (theActorIsReferredToByAPronoun)
        {
            return GetStage().TheActorInTheSpotlight().UsingPronoun(requiredActor);
        }

        if (AnActorIsOnStage() && TheActorInTheSpotlight().Name.Equals(A_New_Actor))
        {
            TheActorInTheSpotlight().AssignName(requiredActor);
            return TheActorInTheSpotlight();
        }

        return GetStage().ShineSpotlightOn(requiredActor);
    }

    /// <summary>
    ///     Determines whether there is an actor currently present on the stage.
    /// </summary>
    /// <returns>A boolean value indicating if an actor is on the stage.</returns>
    private static bool AnActorIsOnStage()
    {
        return GetStage().AnActorIsOnStage();
    }

    /// <summary>
    ///     Create a new actor whose name is not yet known.
    ///     The next time the TheActorCalled() method is used, this name will be assigned to this actor.
    /// </summary>
    public static Actor ANewActor()
    {
        return GetStage().ShineSpotlightOn(A_New_Actor);
    }

    /// <summary>
    ///     A shorter version of "TheActorCalled()"
    /// </summary>
    public static Actor TheActor(string actorName)
    {
        return TheActorCalled(actorName);
    }

    /// <summary>
    ///     The actor in the spotlight is the last actor on the stage who has performed any activity.
    /// </summary>
    public static Actor TheActorInTheSpotlight()
    {
        return GetStage().TheActorInTheSpotlight();
    }

    /// <summary>
    ///     A shorter version of "TheActorInTheSpotlight().AttemptsTo(...)"
    /// </summary>
    public static void WithCurrentActor(params IPerformable[] performTasks)
    {
        TheActorInTheSpotlight().AttemptsTo(performTasks);
    }

    /// <summary>
    ///     Get the current stage. Rarely needed for non-internal use, except when running tasks in parallel.
    ///     In that case, you will need to call OnStage.SetTheStage(stage) in each parallel thread if you use
    ///     OnStage methods such as TheActorInTheSpotlight()
    /// </summary>
    public static Stage GetStage()
    {
        if (Stage.Value == null)
        {
            throw new NoStageException(
                "No stage available - it looks like you haven't called the SetTheStage() method before calling this one.");
        }

        return Stage.Value;
    }

    /// <summary>
    ///     Perform any cleanup actions on each actor on the stage.
    ///     This calls the WrapUp() method if defined on each actor on the stage.
    /// </summary>
    public static void DrawTheCurtain()
    {
        if (Stage.Value != null)
        {
            GetStage().DrawTheCurtain();
        }
    }

    /// <summary>
    ///     Retrieves a collection of configured pronouns available for use.
    /// </summary>
    /// <returns>
    ///     An enumerable collection of strings representing pronouns.
    /// </returns>
    private static IEnumerable<string> GetPronouns()
    {
        var config = ConfigurationManager.AppSettings["screenplay.pronouns"] ?? DefaultPronouns;
        return config.Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p));
    }
}
