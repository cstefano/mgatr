using DbUp.ScriptProviders;

namespace Migratonator;

internal class MigrateScriptCommand : BaseCommand
{
    // TODO: provide ability to apply a single script
    // TODO: make well-known directories configurable

    // the paths to scripts, relative to the base script path
    private static readonly string[] ScriptPaths =
    {
        "Assemblies",
        "Functions",
        "Procedures",
        "Stored Procedures", // deprecate this path (spaces in paths can be problematic)
        "Views",
        "Jobs"
    };

    public MigrateScriptCommand(Options configuration, bool dryRun) :
        base(configuration)
    {
        DryRun = dryRun;
    }

    private bool DryRun { get; }

    public override void Perform()
    {
        var builder = CreateUnJournaledMigrator()
            // disable a single transaction since scripts can contain multiple GO statements
            // which implicitly commit the transaction, and cases such as ALTER DATABASE statements 
            // don't support nested transactions
            // if specific transactions are required, they will need to be included in the scripts
            .WithoutTransaction();

        // only add paths which exist
        foreach (var path in ScriptPaths)
        {
            var qualifiedPath = Path.Combine(Options.ScriptsPath, path);
            if (Directory.Exists(qualifiedPath))
                builder.WithScripts(new FileSystemScriptProvider(
                    qualifiedPath,
                    new FileSystemScriptOptions { IncludeSubDirectories = true }
                ));
            else if (Options.Verbose)
                Console.WriteLine($"WARNING: {path} path not found. Ignoring.");
        }

        Apply(builder.Build(), DryRun, Options.Confirm);
    }
}