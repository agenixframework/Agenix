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

namespace Agenix.Screenplay.Cast;

/// <summary>
///     The stage keeps track of the actors currently in a screenplay test, referenced by name.
///     You rarely need to use this class directly in your tests. Normally you would use the OnStage class instead.
/// </summary>
public class Stage
{
    private readonly Casting _cast;
    private Actor _actorInTheSpotlight;

    public Stage(Casting cast)
    {
        _cast = cast;
    }

    /// <summary>
    ///     Place an actor with a given name in the spotlight, without the intent to have them perform an action at this time.
    /// </summary>
    public Actor ShineSpotlightOn(string actorName)
    {
        var knownActor = _cast.GetActors()
            .FirstOrDefault(actor => actor.Name.Equals(actorName, StringComparison.OrdinalIgnoreCase));

        _actorInTheSpotlight = knownActor ?? _cast.ActorNamed(actorName);
        return TheActorInTheSpotlight().WithNoPronoun();
    }

    /// <summary>
    ///     Return the current actor in the spotlight.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when there is no actor in the spotlight.</exception>
    public Actor TheActorInTheSpotlight()
    {
        if (_actorInTheSpotlight == null)
        {
            throw new ArgumentNullException(nameof(_actorInTheSpotlight), "No actor is currently in the spotlight");
        }

        return _actorInTheSpotlight;
    }

    /// <summary>
    ///     A shortened form of TheActorInTheSpotlight()
    /// </summary>
    public Actor TheActor()
    {
        return TheActorInTheSpotlight();
    }

    /// <summary>
    ///     This method should be called at the end of the screenplay test to perform teardown actions on each actor.
    ///     It will generally be done automatically by the test framework.
    /// </summary>
    public void DrawTheCurtain()
    {
        _cast.DismissAll();
    }

    /// <summary>
    ///     Check whether there is any actor in the spotlight at the moment.
    /// </summary>
    public bool AnActorIsOnStage()
    {
        return _actorInTheSpotlight != null;
    }
}
