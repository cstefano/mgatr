using DbUp.ScriptProviders;

namespace Migratonator;

internal class MigrateUpCommand : BaseCommand
{
    // TODO: provide ability to apply N number of migrations instead of all of them

    public MigrateUpCommand(Options configuration, bool dryRun) :
        base(configuration)
    {
        DryRun = dryRun;
    }

    private bool DryRun { get; }

    public override void Perform()
    {
        var builder = CreateJournaledMigrator()
            .WithScripts(new FileSystemScriptProvider(Path.Combine(Options.ScriptsPath, MigrationPath)))
            // configure to run a transaction per script
            .WithoutTransaction();

        Apply(builder.Build(), DryRun, Options.Confirm);
    }
}