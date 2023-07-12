using GenerateAst;
using System.CommandLine;

// Note to self: In the future, consider using python or another language better suited to
// generating textual output easily.

var rootCommand = new RootCommand();

rootCommand.SetHandler(CLI.HandleMainCommand);

await rootCommand.InvokeAsync(args);
