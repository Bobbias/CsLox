﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ReadLineReboot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static CsLox.Token;

namespace CsLox
{
    public static class CLI
    {
        /// <summary>
        /// Indicates whether the CLI environment we're running in supports ANSI escape codes.
        /// </summary>
        public static bool SupportsColor { get; set; } = false;

        /// <summary>
        /// Tracks whether an error occurred during the course of parsing some Lox.
        /// </summary>
        public static bool HadError { get; set; } = false;

        /// <summary>
        /// Tracks whether a runtime error occured during the course of evaluating some Lox.
        /// </summary>
        public static bool HadRuntimeError { get; set; } = false;

        /// <summary>
        /// A static instance of <see cref="CsLox.Interpreter"/> to evaluate code in.
        /// <para>
        /// By making this instance static, we ensure that state is preserved between calls. This is important for the REPL to function properly.
        /// </para>
        /// </summary>
        public static Interpreter Interpreter { get; } = new Interpreter();

        /// <summary>
        /// Exit codes defined in <see href="https://man.freebsd.org/cgi/man.cgi?query=sysexits&amp;apropos=0&amp;sektion=0&amp;manpath=FreeBSD+4.3-RELEASE&amp;format=html">sysexit.h</see>.
        /// </summary>
        private enum ExitCode : int
        {
            EX_OK = 0,
            // Failure status codes
            EX_USAGE = 64,
            EX_DATAERR = 65,
            EX_NOINPUT = 66,
            EX_NOUSER = 67,
            EX_NOHOST = 68,
            EX_UNAVAILABLE = 69,
            EX_SOFTWARE = 70,
            EX_OSERR = 71,
            EX_OSFILE = 72,
            EX_CANTCREAT = 73,
            EX_IOERR = 74,
            EX_TEMPFAIL = 75,
            EX_PROTOCOL = 76,
            EX_NOPERM = 77,
            EX_CONFIG = 78
        }

        /// <summary>
        /// Handles the compile command.
        /// </summary>
        /// <param name="file">The file to compile.</param>
        public static void HandleCompileCommand(FileInfo file)
        {
            Console.WriteLine($"Compile `{file.FullName}`.");
        }

        /// <summary>
        /// Handles the interpret command.
        /// </summary>
        /// <param name="file">The file to interpret.</param>
        public static void HandleInterpretCommand(FileInfo file)
        {
            RunFile(file);
        }

        /// <summary>
        /// Handles the REPL command.
        /// </summary>
        public static void HandleReplCommand()
        {
            RunPrompt();
        }

        /// <summary>
        /// Handles the lex subcommand for printing out the tokens generated by the Scanner.
        /// </summary>
        /// <param name="file">The file to process.</param>
        public static void HandleDebugLexCommand(FileInfo file)
        {
            var reader = file.OpenText();
            var source = reader.ReadToEnd();

            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        /// <summary>
        /// Handles the parse subcommand for printing out the AST generated by the Parser.
        /// </summary>
        /// <param name="file"></param>
        public static void HandleDebugParseCommand(FileInfo file)
        {
            var reader = file.OpenText();
            var source = reader.ReadToEnd();

            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            foreach (var statement in statements)
            {
                Console.WriteLine($"Statement: {statement}");
            }
        }

        /// <summary>
        /// Wraps the <see cref="Run"/> method, accepting a file and passing it's text as the source to run.
        /// </summary>
        /// <param name="file">The file to run.</param>
        public static void RunFile(FileInfo file)
        {
            var reader = file.OpenText();
            var source = reader.ReadToEnd();
            Run(source);

            // Indicate an error in the exit code.
            if (HadError) Environment.Exit((int)ExitCode.EX_DATAERR);
            if (HadRuntimeError) Environment.Exit((int)ExitCode.EX_SOFTWARE);
        }

        /// <summary>
        /// Wraps the <see cref="Run"/> method for use in the REPL. 
        /// </summary>
        public static void RunPrompt()
        {
            // Set up Readline
            // Consider adding autocompletion capabilities at some point... This might be a very large task.
            ReadLine.AutoCompletionEnabled = false;
            ReadLine.HistoryEnabled = true;

            while (true)
            {
                var line = ReadLine.Read("> ");
                if (line == "" || line == "#quit")
                {
                    break;
                }

                Run(line);

                // Reset error, since a failure of a single input shouldn't kill the program.
                HadError = false;
            }
        }

        /// <summary>
        /// Scans, Parses, and Interprets a given chunk of source code.
        /// </summary>
        /// <param name="source">A string containing the source code to process.</param>
        public static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            // Stop if there was a syntax error.
            if (HadError) return;

            // Run variable resolution phase before interpreting.
            var resolver = new Resolver(Interpreter);
            resolver.Resolve(statements);

            // Stop if there was a resolution error.
            if (HadError) return;

            // TODO: Handle null expr
            Interpreter.Interpret(statements);
        }

        /// <summary>
        /// Reports an error with no known location.
        /// </summary>
        /// <param name="line">The line number where the error occurred.</param>
        /// <param name="message">The error message to display to the user.</param>
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        /// <summary>
        /// Reports an error with a known location. Also sets HadError.
        /// </summary>
        /// <param name="line">The line number where the error occurred.</param>
        /// <param name="where">The location where the error occurred.</param>
        /// <param name="message">The error message to display to the user.</param>
        public static void Report(int line, string where, string message)
        {
            var err = Console.Error;
            err.WriteLine($"[line {line}] Error {where}: {message}");
            HadError = true;
        }

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        /// <summary>
        /// Reports a runtime error.
        /// </summary>
        /// <param name="e"></param>
        public static void RuntimeError(CsLoxRuntimeException e)
        {
            if (e.HasToken())
            {
                Console.Error.WriteLine($"{e.Message}\n[line {e.Token!.Line}]");
            }
            else
            {
                Console.Error.WriteLine($"{e.Message}\n[line unknown]");
            }
            HadRuntimeError = true;
        }

        /// <summary>
        /// Collects a few utility features, such as checking whether the console supports ANSI escape sequences.
        /// </summary>
        internal static class ConsoleUtil
        {
            /// <summary>
            /// See <see href="https://learn.microsoft.com/en-us/windows/console/getconsolemode">GetConsoleMode</see> at MSDN.
            /// </summary>
            /// <param name="hConsoleHandle">A handle to a console.</param>
            /// <param name="lpMode">The <see cref="ConsoleMode"/> requested.</param>
            /// <returns></returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            /// <summary>
            /// See <see href="https://learn.microsoft.com/en-us/windows/console/getstdhandle">GetStdHandle</see> at MSDN.
            /// </summary>
            /// <param name="nStdHandle">The id of the standard IO to get a handle to. See <see cref="StandardIOHandles"/>.</param>
            /// <returns>An <see cref="IntPtr"/> containing a handle to the standard IO specified.</returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int nStdHandle);

            /// <summary>
            /// Defines various console mode values which can be get or set using the appropriate functions.
            /// </summary>
            [Flags]
            private enum ConsoleModes : uint
            {
                ENABLE_PROCESSED_INPUT = 0x0001,
                ENABLE_LINE_INPUT = 0x0002,
                ENABLE_ECHO_INPUT = 0x0004,
                ENABLE_WINDOW_INPUT = 0x0008,
                ENABLE_MOUSE_INPUT = 0x0010,
                ENABLE_INSERT_MODE = 0x0020,
                ENABLE_QUICK_EDIT_MODE = 0x0040,
                ENABLE_EXTENDED_FLAGS = 0x0080,
                ENABLE_AUTO_POSITION = 0x0100,
                ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,

                ENABLE_PROCESSED_OUTPUT = 0x0001,
                ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
                ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
                DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
                ENABLE_LVB_GRID_WORLDWIDE = 0x0010
            }

            /// <summary>
            /// The standard input, output, and error handles.
            /// </summary>
            private enum StandardIOHandles : int
            {
                STD_INPUT_HANDLE = -10,
                STD_OUTPUT_HANDLE = -11,
                STD_ERROR_HANDLE = -12
            }

            /// <summary>
            /// Checks whether the terminal we are running in supports ANSI escape sequences.
            /// <para/>
            /// This function makes use of the native functions <see href="https://learn.microsoft.com/en-us/windows/console/getconsolemode">GetConsoleMode</see> and <see href="https://learn.microsoft.com/en-us/windows/console/getstdhandle">GetStdHandle</see>.
            /// </summary>
            /// <returns><see langword="true"/> if the console supports ANSI escape sequences, otherwise <see langword="false"/>.</returns>
            public static bool DoesTerminalSupportAnsiEscapes()
            {
                IntPtr consoleHandle = GetStdHandle((int)StandardIOHandles.STD_INPUT_HANDLE);
                var mode = (uint)ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                return GetConsoleMode(consoleHandle, out mode);
            }
        }
    }
}
