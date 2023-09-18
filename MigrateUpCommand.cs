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
            // disable a single transaction since migrations can contain multiple GO statements
            // which implicitly commit the transaction, and cases such as ALTER DATABASE statements 
            // don't support nested transactions
            // if specific transactions are required, they will need to be included in the scripts
            .WithoutTransaction();

        Apply(builder.Build(), DryRun, Options.Confirm);
    }
}