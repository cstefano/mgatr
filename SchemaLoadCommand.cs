using DbUp.Engine;

namespace Migratonator;

internal class SchemaLoadCommand : BaseCommand
{
    public SchemaLoadCommand(Options configuration) :
        base(configuration)
    {
    }

    public override void Perform()
    {
        Console.WriteLine("Loading database schema from file...");

        var script = new SqlScript("Load Schema", File.ReadAllText(Options.SchemaFile));

        var migrator = CreateUnJournaledMigrator()
            .WithScript(script)
            // disable transactions since the Schema.sql is broken down with GO statements
            // which implicitly commit the transaction, and since ALTER DATABASE statements 
            // don't support nested transactions
            .WithoutTransaction()
            .Build();

        if (!Options.Confirm && !PromptUserToContinue($"Load database schema into '{Options.DatabaseName}' database?"))
            return;

        var result = migrator.PerformUpgrade();

        if (!result.Successful)
        {
            ReportFailure("Failed to load database schema.");
            return;
        }

        ReportSuccess("Loaded database schema successfully!");
    }
}