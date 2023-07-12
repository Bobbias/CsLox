﻿using CsLox;
using System.CommandLine;

var rootCommand = new RootCommand("CsLox CLI.");

// Command to compile a given file.
// Has the form of `CsLox build <file>`
var compileCommand = new Command("build", "Compile a Lox source file into bytecode.");
rootCommand.Add(compileCommand);

var fileArgument = new Argument<FileInfo>(
    name: "input",
    description: "The file to compile. If no file is provided, CsLox looks for a file named main.lox.",
    getDefaultValue: () => new FileInfo("main.lox")
    );
compileCommand.Add(fileArgument);
compileCommand.SetHandler(CLI.HandleCompileCommand, fileArgument);

// Command to interpret a given file
var interpretCommand = new Command("run", "Interpret a lox source file.");
rootCommand.Add(interpretCommand);

interpretCommand.Add(fileArgument);
interpretCommand.SetHandler(CLI.HandleInterpretCommand, fileArgument);

// Command to start a Lox REPL
var replCommand = new Command("repl", "Start a Lox REPL.");
rootCommand.Add(replCommand);
replCommand.SetHandler(CLI.HandleReplCommand);

// Command for grouping debug commands together
var debugCommand = new Command("debug", "Debug CsLox.");
rootCommand.Add(debugCommand);

// Sub command to display the result of lexing a given file
var lexCommand = new Command("lex", "Print the lexer output");
debugCommand.Add(lexCommand);

lexCommand.Add(fileArgument);
lexCommand.SetHandler(CLI.HandleDebugLexCommand, fileArgument);

var parseCommand = new Command("parse", "Print the AST generated by the parser");
debugCommand.Add(parseCommand);

parseCommand.Add(fileArgument);
parseCommand.SetHandler(CLI.HandleDebugParseCommand, fileArgument);

// Determine if we can handle ANSI escape codes.
CLI.SupportsColor = CLI.ConsoleUtil.DoesTerminalSupportAnsiEscapes();

if (!CLI.SupportsColor)
{
    Console.NO_COLOR = true;
}

await rootCommand.InvokeAsync(args);