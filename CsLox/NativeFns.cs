﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Contains runtime implementations for native functions in Lox.
    /// </summary>
    public static class NativeFns
    {
        /// <summary>
        /// Represents the native function clock() in Lox, which returns the number of seconds since some fixed point in time.
        /// </summary>
        /// <remarks>
        /// In this implementation, I have chosen to return the number of seconds since system boot, rather than unix time, which the original Java implementation does.
        /// </remarks>
        public class Clock : ILoxCallable
        {
            /// <summary>
            /// The number of operands a call to this function takes.
            /// </summary>
            /// <remarks>
            /// Always 0 for Clock.
            /// </remarks>
            public int Arity { get; set; } = 0;

            /// <summary>
            /// Initializes a new instance of the Clock class.
            /// </summary>
            public Clock() { }

            /// <summary>
            /// Returns the number of seconds that have passed since the system booted.
            /// </summary>
            /// <remarks>
            /// While this implementation is not quite the same as the original jlox, it should suffice. The definition of clock
            /// given in Crafting Interpreters specifically reads "a native function that returns the number of seconds that have
            /// passed since some fixed point in time." meaning that the fixed time is unimportant to the functionality.
            /// </remarks>
            /// <param name="interpreter"></param>
            /// <param name="args"></param>
            /// <returns>A <see langword="double"/> containing the number of seconds passed since the system booted.</returns>
            public object? Call(Interpreter interpreter, List<object?> args)
            {
                return Environment.TickCount64 / 1000.0;
            }
        }
    }
}
