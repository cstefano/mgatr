using System.Text.RegularExpressions;
using DbUp.Engine;

namespace Migratonator;

internal partial class MacroPreprocessor : IScriptPreprocessor
{
    public MacroPreprocessor(string macrosFilename)
    {
        Macros = Macro.LoadMacros(macrosFilename);

        // some fun...
        Macros["OhDearyMe"] = new Macro
        {
            Name = "OhDearyMe",
            Template = "/* Oh deary me, %WORD1%, %WORD2%!!! */",
            Parameters = new[] { "WORD1", "WORD2" }
        };
    }

    private IDictionary<string, Macro> Macros { get; }

    public string Process(string contents)
    {
        //
        // finds content matching the following pattern using a regular expression
        //
        // * the regular expression will have two named groups, "name" and "parameters"
        // * the "name" group will be the name of the macro to render
        // * the "parameters" group will be the parameters to pass to the macro
        // * the macro must be enclosed in "-- %" and "%" and be on a single line
        //
        // "-- %Macro:Name%"
        // "-- %Macro:Name[]%"
        // "-- %Macro:Name[Parameter1|...|ParameterN]%"
        //
        // NOTE: parameters can also be $Variable$ which will be replaced later on in the process
        //
        // E.g. "-- %Macro:Name[$Variable1$|$Variable2$]%"
        //

        var rendered = contents;
        foreach (Match match in MacroRegex().Matches(contents))
        {
            if (!Macros.TryGetValue(match.Groups["name"].Value, out var macro))
            {
                Console.WriteLine($"WARNING: Macro '{match.Groups["name"].Value}' not found.");
                continue;
            }

            var parameters = match.Groups["parameters"].Value;
            var template = macro.Render(parameters);
            rendered = rendered.Replace(match.Value, template);
        }

        return rendered;
    }

    [GeneratedRegex("^-- %Macro:(?<name>[^\\[\\]]+)(\\[(?<parameters>[^\\]]+)\\])?%$", RegexOptions.Multiline)]
    private static partial Regex MacroRegex();
}