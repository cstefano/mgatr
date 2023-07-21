using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using Microsoft.Data.SqlClient;

namespace Migratonator;

internal abstract class DatabaseCommand : ICommand
{
    // putting this in a constant because it _will_ change someday!
    public const string PrimaryDatabaseName = "master";

    protected DatabaseCommand(Options configuration)
    {
        Configuration = configuration;
    }

    private Options Configuration { get; }

    public void Perform()
    {
        // parse the connection string, so the database name can be extracted
        var connectionStringBuilder = new SqlConnectionStringBuilder(Configuration.ConnectionString);
        var databaseName = connectionStringBuilder.InitialCatalog;

        // change the database name in the connection string
        // since creating or dropping a database requires being
        // connected to the primary database (`master` circa 2023)
        connectionStringBuilder.InitialCatalog = PrimaryDatabaseName;

        // create a script to perform the action
        var actionScript = new SqlScript(
            $"{UserFriendlyAction} Database",
            SqlStatement(databaseName)
        );

        var migrator = DeployChanges.To
            .SqlDatabase(connectionStringBuilder.ConnectionString)
            .JournalTo(new NullJournal())
            .WithScript(actionScript)
            .WithExecutionTimeout(Configuration.ExecutionTimeout)
            .WithoutTransaction()
            .Build();

        if (!Configuration.Confirm &&
            !BaseCommand.PromptUserToContinue($"{UserFriendlyAction} '{databaseName}' database?"))
            return;

        Console.WriteLine($"Applying {UserFriendlyAction} '{databaseName}' database...");
        var result = migrator.PerformUpgrade();

        if (!result.Successful)
        {
            BaseCommand.ReportFailure(result.Error.ToString());
            return;
        }

        BaseCommand.ReportSuccess($"{UserFriendlyAction} database succeeded!");
    }

    protected abstract string UserFriendlyAction { get; }
    protected abstract string SqlStatement(string databaseName);
}