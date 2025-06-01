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
