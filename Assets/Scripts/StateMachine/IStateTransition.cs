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
/// A transition from one state to another. This is broken into two parts. The first is the 'exit'
/// process where the current state is left. The second is the 'enter' process where the next state
/// becomes the new current state. Both of these processes are executed over time.
/// </summary>
public interface IStateTransition
{
    /// <summary>
    /// Exit the current state over time. This function should 'yield return' until the exit process
    /// is finished.
    /// </summary>
    IEnumerable Exit();

    /// <summary>
    /// Enter the next state over time. This function should 'yield return' until the enter process
    /// is finished.
    /// </summary>
    IEnumerable Enter();
}