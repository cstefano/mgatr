using DbUp.Engine;

namespace Migratonator;

internal class SeedsPreprocessor : IScriptPreprocessor
{
    public SeedsPreprocessor(string seedsPath)
    {
        SeedsPath = seedsPath;
    }

    private string SeedsPath { get; }

    public string Process(string contents)
    {
        // inject the setvar directive with the qualified path to seeds
        var setSeedsPathSql = $":setvar SeedsPath '{SeedsPath}';\n\n";
        return setSeedsPathSql + contents;
    }
}
