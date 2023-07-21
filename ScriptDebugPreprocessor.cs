using DbUp.Engine;

namespace Migratonator;

internal partial class ScriptDebugPreprocessor : IScriptPreprocessor
{
    // NB: this script preprocessor is only used when the --debug option is specified
    // and should be added as the last processor in the builder chain!
    public string Process(string contents)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("-- >> BEGIN SCRIPT CONTENTS");
        Console.WriteLine(contents);
        Console.WriteLine("-- << END SCRIPT CONTENTS");
        Console.ResetColor();

        return contents;
    }
}