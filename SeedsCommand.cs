using DbUp.ScriptProviders;

namespace Migratonator;

internal class SeedsCommand : BaseCommand
{
    // the path to seeds scripts, relative to the base script path
    private const string SeedsPath = "Seeds";

    public SeedsCommand(Options configuration, bool dryRun) :
        base(configuration)
    {
        DryRun = dryRun;
    }

    private bool DryRun { get; }

    public override void Perform()
    {
        var builder = CreateUnJournaledMigrator();

        var qualifiedPath = Path.Combine(Options.ScriptsPath, SeedsPath);
        if (Directory.Exists(qualifiedPath))
        {
            builder.WithScripts(new FileSystemScriptProvider(qualifiedPath));
        }
        else
        {
            Console.WriteLine($"WARNING: {qualifiedPath} path not found. Ignoring.");
            return;
        }

        Apply(builder.Build(), DryRun, Options.Confirm, false);
    }
}