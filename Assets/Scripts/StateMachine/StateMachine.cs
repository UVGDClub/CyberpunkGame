////////////////////////////////////////////////////////////////////////////////////////////////////
// About:   Finite State Machine by Jackson Dunstan
// Article: http://JacksonDunstan.com/articles/3137
// License: MIT
// Copyright © 2015 Jackson Dunstan
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections;

/// <summary>
/// A state machine implementation that executes states and transitions
/// </summary>
public class StateMachine : IStateMachine
{
    /// <summary>
    /// The current state
    /// </summary>
    private IState state;

    /// <summary>
    /// The next state (to transition to)
    /// </summary>
    private IState nextState;

    /// <summary>
    /// Transition to use to get to the next state
    /// </summary>
    private IStateTransition transition;

    /// <summary>
    /// Create the state machine with an initial state
    /// </summary>
    /// <param name="initialState">Initial state. BeginEnter() and EndEnter() will be called
    /// on it immediately</param>
    public StateMachine(IState initialState)
    {
        State = initialState;
        state.EndEnter();
    }

    /// <summary>
    /// Execute the initial state and any subsequent states and transitions until there are no more
    /// states to execute.
    /// </summary>
    public IEnumerable Execute()
    {
        while (true)
        {
            // Execute the current state until it transitions or stops executing
            for (
                var e = state.Execute().GetEnumerator();
                transition == null && e.MoveNext();
            )
            {
                yield return e.Current;
            }

            // Wait until the current state transitions
            while (transition == null)
            {
                yield return null;
            }

            // Stop listening for the current state to transition
            // This prevents accidentally transitioning twice
            state.OnBeginExit -= HandleStateBeginExit;

            // There is no next state to transition to
            // This means the state machine is finished executing
            if (nextState == null)
            {
                break;
            }

            // Run the transition's exit process
            foreach (var e in transition.Exit())
            {
                yield return e;
            }
            state.EndExit();

            // Switch state
            State = nextState;
            nextState = null;

            // Run the transition's enter process
            foreach (var e in transition.Enter())
            {
                yield return e;
            }
            transition = null;
            state.EndEnter();
        }
    }

    /// <summary>
    /// Set the current state, call its BeginEnter(), and listen for it to transition
    /// </summary>
    /// <value>The state to be the new current state</value>
    private IState State
    {
        set
        {
            state = value;
            state.OnBeginExit += HandleStateBeginExit;
            state.BeginEnter();
        }
    }

    /// <summary>
    /// Handles the current state wanting to transition
    /// </summary>
    /// <param name="sender">The state that wants to transition</param>
    /// <param name="e">Information about the desired transition</param>
    private void HandleStateBeginExit(object sender, StateBeginExitEventArgs e)
    {
        nextState = e.NextState;
        transition = e.Transition;
    }
}