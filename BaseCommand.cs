using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Helpers;
using DbUp.SqlServer;

namespace Migratonator;

internal abstract class BaseCommand : ICommand
{
    // the path to migration scripts, relative to the base script path
    protected const string MigrationPath = "Migrations";

    // the name of the table used to track migrations (journal)
    protected const string SchemaVersionsTable = "SchemaVersions";

    protected BaseCommand(Options options)
    {
        Options = options;
    }

    protected Options Options { get; }

    public abstract void Perform();

    protected UpgradeEngineBuilder CreateJournaledMigrator()
    {
        // creates a journaled migrator which will be used
        // to modify the database schema and track which scripts
        // have been applied, so they only get applied once.
        //
        // manages the instance of the SQL journal here, so that 
        // the table name is owned by the migrator and safely used.
        // the database schema is given as null since that the underlying
        // provider will use the default schema, which is `dbo` for MSSQL
        // or is provided via the connection string
        return CreateMigrator()
            .JournalTo(
                (connectionManager, upgradeLog) =>
                    new SqlTableJournal(connectionManager, upgradeLog, null!, SchemaVersionsTable)
            );
    }

    protected UpgradeEngineBuilder CreateUnJournaledMigrator()
    {
        // creates an un-journaled migrator which will be used
        // to run scripts over and over again, such as for views
        // stored procedures, functions, etc.
        return CreateMigrator()
            .JournalTo(new NullJournal());
    }

    private UpgradeEngineBuilder CreateMigrator()
    {
        var variables = Variable.LoadVariables(Options.VariablesFile);

        // add built-in variables
        variables.Add("DatabaseName", Options.DatabaseName);
        variables.Add("SchemaName", Options.SchemaName);

        var builder = DeployChanges.To
            .SqlDatabase(Options.ConnectionString)

            // special handling for macros
            .WithPreprocessor(new MacroPreprocessor(Options.MacrosFile))

            // include defined variables
            .WithVariables(variables)

            // special handling for SQL CLR assemblies
            .WithPreprocessor(new SqlClrAssemblyPreprocessor(Options.ScriptsPath))

            // configure execution timeout; null clears underlying providers' default
            .WithExecutionTimeout(Options.ExecutionTimeout)

            // display script outputs, so that issues can be spotted
            .LogScriptOutput();

        if (Options.Debug)
            builder = builder
                .WithPreprocessor(new ScriptDebugPreprocessor());

        return builder;
    }

    protected void Apply(UpgradeEngine migrator, bool dryRun = true, bool confirm = false, bool dumpSchema = true)
    {
        if (!migrator.IsUpgradeRequired())
        {
            Console.WriteLine("👉 No scripts to run. Exiting.");
            return;
        }

        var scripts = migrator.GetScriptsToExecute();

        if (dryRun || !confirm)
        {
            if (Options.Verbose)
            {
                Console.WriteLine("👉 The following scripts will be executed:");
                Console.WriteLine();
                foreach (var script in scripts)
                    Console.WriteLine($" ● {script.Name}");
            }
            else
            {
                Console.WriteLine($"👉 {scripts.Count} scripts will be executed.");
            }

            if (dryRun) return;
            if (!PromptUserToContinue($"Apply changes to '{Options.DatabaseName}' database?")) return;
        }

        Console.WriteLine($"Applying {scripts.Count} changes...");
        var result = migrator.PerformUpgrade();

        if (!result.Successful)
        {
            ReportFailure(result.Error.Message);
            return;
        }

        // dump the database schema
        if (dumpSchema)
            new SchemaDumpCommand(Options).Perform();

        ReportSuccess("Applied changes successfully!");
    }

    public static bool PromptUserToContinue(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.Write($"{message} (Y/N): ");
        Console.ResetColor();

        var answer = Console.ReadLine();
        if (string.Equals(answer, "Y", StringComparison.InvariantCultureIgnoreCase)) return true;

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("👉 Operation cancelled, exiting.");
        Console.ResetColor();
        return false;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void ReportSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void ReportWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void ReportFailure(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void ReportError(Exception exception)
    {
        ReportFailure(exception.Message);
        Console.WriteLine(exception.StackTrace);
    }
}