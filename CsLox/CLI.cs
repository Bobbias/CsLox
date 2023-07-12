using System;
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
    internal class CLI
    {
        /// <summary>
        /// Indicates whether the CLI environment we're running in supports ANSI escape codes.
        /// </summary>
        public static bool SupportsColor { get; set; } = false;

        /// <summary>
        /// Tracks whether an error occurred during the course of running some Lox.
        /// </summary>
        public static bool HadError { get; set; } = false;

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

        public static void HandleCompileCommand(FileInfo file)
        {
            Console.WriteLine($"Compile `{file.FullName}`.");
        }

        public static void HandleInterpretCommand(FileInfo file)
        {
            Console.WriteLine($"Interpret `{file.FullName}`.");
        }

        public static void HandleReplCommand()
        {
            Console.WriteLine($"Run REPL.");
        }

        public static void HandleDebugLexCommand(FileInfo file)
        {
            Console.WriteLine($"Print Lexer results for `{file.FullName}`.");
        }

        public static void HandleDebugParseCommand(FileInfo file)
        {
            Console.WriteLine($"Print Parser results for `{file.FullName}`.");
        }

        public static void RunFile(FileInfo file)
        {
            var reader = file.OpenText();
            var source = reader.ReadToEnd();
            Run(source);

            // Indicate an error in the exit code.
            if (HadError) Environment.Exit( (int)ExitCode.EX_DATAERR );
        }

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

        public static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            foreach ( var token in tokens )
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Report(int line, string where, string message)
        {
            var err = Console.Error;
            err.WriteLine($"[line {line}] Error {where}: {message}");
            HadError = true;
        }

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

        internal class ConsoleUtil
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int nStdHandle);

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

            private enum StandardIOHandles : int
            {
                STD_INPUT_HANDLE = -10,
                STD_OUTPUT_HANDLE = -11,
                STD_ERROR_HANDLE = -12
            }

            public static bool DoesTerminalSupportAnsiEscapes()
            {
                IntPtr consoleHandle = GetStdHandle((int)StandardIOHandles.STD_INPUT_HANDLE);
                var mode = (uint)ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                return GetConsoleMode(consoleHandle, out mode);
            }
        }
    }
}
