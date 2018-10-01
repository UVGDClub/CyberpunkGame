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
using System;

/// <summary>
/// A mode of the app that executes over time. This begins the transition process and receives
/// notifications about its progress.
/// </summary>
public interface IState
{
    /// <summary>
    /// Notify the state that the transition process has begun the entering phase
    /// </summary>
    void BeginEnter();

    /// <summary>
    /// Notify the state that the transition process has finished the entering phase
    /// </summary>
    void EndEnter();

    /// <summary>
    /// Execute the state's logic over time. This function should 'yield return' until it has
    /// nothing left to do. It may also dispatch OnBeginExit when it is ready to begin transitioning
    /// to the next state. If it does this, this funtion will no longer be resumed.
    /// </summary>
    IEnumerable Execute();

    /// <summary>
    /// Dispatched when the state is ready to begin transitioning to the next state. This implies
    /// that the transition process will immediately begin the exiting phase.
    /// </summary>
    event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    /// <summary>
    /// Notify the state that the transition process has finished exiting phase.
    /// </summary>
    void EndExit();
}
