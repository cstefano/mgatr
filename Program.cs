using Migratonator;
using System.CommandLine;

// ------------------------------------------------------------
// global options

var optionConnectionString = new Option<string>(
    "--connection",
    description:
    "SQL Server connection string. Can also be provided using the MIGRATONATOR_CONNECTION environment variable.\n" +
    "E.g. \"Server=(local);Database=ApplicationDB;User=sa;Password=<redacted>;...etc...;\".",
    isDefault: true,
    parseArgument: result =>
    {
        var connectionString = string.Empty;
        if (result.Tokens.Any())
            connectionString = result.Tokens.Single().Value;
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = Environment.GetEnvironmentVariable("MIGRATONATOR_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            result.ErrorMessage = $"A connection string is required.";
        return connectionString!;
    }
) { IsRequired = true };

// if null, will use the underlying provider default
var optionExecutionTimeout = new Option<TimeSpan?>(
    new[] { "--timeout" },
    "Script execution timeout."
);

// base path to work off
// all relative paths will be in respect of this path
var optionScriptsPath = new Option<string>(
    new[] { "--path" },
    Directory.GetCurrentDirectory,
    "Base path for scripts."
) { IsRequired = true };

// ensure --path directory exists
optionScriptsPath.AddValidator(result =>
{
    var path = result.GetValueForOption(optionScriptsPath);
    if (!Directory.Exists(path))
        result.ErrorMessage = $"The base path '{path}' does not exist.";
});

var optionAssemblyBinariesPath = new Option<string>(
    new[] { "--binaries" },
    Directory.GetCurrentDirectory,
    "Path for SQL CLR assembly binaries."
) { IsRequired = true };

// ensure --binaries directory exists
optionAssemblyBinariesPath.AddValidator(result =>
{
    var path = result.GetValueForOption(optionAssemblyBinariesPath);
    if (!Directory.Exists(path))
        result.ErrorMessage = $"The path '{path}' for SQL CLR assembly binaries does not exist.";
});

var optionVariablesFile = new Option<string>(
    new[] { "--variables" },
    () => "Variables.json",
    "Variables file."
) { IsRequired = true };

// ensure --variables file exists
optionVariablesFile.AddValidator(result =>
{
    // ensure variables file exists
    var fileName = result.GetValueForOption(optionVariablesFile);
    if (!File.Exists(fileName))
        result.ErrorMessage = $"The variables file '{fileName}' does not exist.";
});

var optionMacrosFile = new Option<string>(
    new[] { "--macros" },
    () => "Macros.json",
    "Macros file."
) { IsRequired = true };

// ensure --macros file exists
optionMacrosFile.AddValidator(result =>
{
    // ensure variables file exists
    var fileName = result.GetValueForOption(optionMacrosFile);
    if (!File.Exists(fileName))
        result.ErrorMessage = $"The macros file '{fileName}' does not exist.";
});

var optionSchemaFile = new Option<string>(
    new[] { "--schema-file" },
    () => "Schema.sql",
    "Filename of the database schema file."
) { IsRequired = true };

// the schema file may not exist yet, so validate
// the directory instead if it's a rooted path
optionSchemaFile.AddValidator(result =>
{
    // ensure variables file exists
    var fileName = result.GetValueForOption(optionSchemaFile);

    if (string.IsNullOrWhiteSpace(fileName))
    {
        result.ErrorMessage = $"The schema file is required.";
        return;
    }

    if (!Path.IsPathRooted(fileName)) return;

    var path = Path.GetDirectoryName(fileName);
    if (!Directory.Exists(path))
        result.ErrorMessage = $"The schema file's path '{path}' does not exist.";
});

var optionConfirm = new Option<bool>(
    new[] { "--confirm" },
    "Hide confirmation prompt. Typically used for CI/CD pipelines or in non-interactive scenarios."
);

var optionVerbose = new Option<bool>(
    new[] { "--verbose", "-v" },
    "Enable verbose logging."
);

var optionDebug = new Option<bool>(
    new[] { "--debug" },
    "Output final scripts before execution."
);

// use binding to manage global options
var optionsBinding = new OptionsBinder(
    optionConnectionString,
    optionExecutionTimeout,
    optionScriptsPath,
    optionVariablesFile,
    optionMacrosFile,
    optionSchemaFile,
    optionAssemblyBinariesPath,
    optionConfirm,
    optionVerbose,
    optionDebug
);

// ------------------------------------------------------------
// db commands
//

// db create command
var commandCreateDatabase = new Command(
    "create",
    "Create the database."
);
commandCreateDatabase.SetHandler(
    (config) => { new CreateDatabaseCommand(config).Perform(); },
    optionsBinding
);

// db drop command
var commandDropDatabase = new Command(
    "drop",
    "Drop the database."
);
commandDropDatabase.SetHandler(
    (config) => { new DropDatabaseCommand(config).Perform(); },
    optionsBinding
);

var commandDatabase = new Command(
    "db",
    "Database management commands."
);
commandDatabase.AddCommand(commandCreateDatabase);
commandDatabase.AddCommand(commandDropDatabase);

// ------------------------------------------------------------
// migration commands
//

var optionSteps = new Option<int>(
    new[] { "--steps", "-n" },
    () => 1,
    "Number of migrations to execute."
);

var optionDryRun = new Option<bool>(
    new[] { "--dry-run" },
    "Run the migrations without actually applying them."
);

var argumentName = new Argument<string>
{
    Name = "name",
    Description = "Name of the migration. E.g. Add Measure to Table X"
};

// create command
var commandMigrateCreate = new Command(
    "create",
    "Create a new database migration."
);
commandMigrateCreate.AddArgument(argumentName);
commandMigrateCreate.SetHandler(
    (config, name) => { new MigrateCreateCommand(config, name).Perform(); },
    optionsBinding,
    argumentName
);

// up command
var commandMigrateUp = new Command(
    "up",
    "Apply the database migrations."
);
commandMigrateUp.AddOption(optionSchemaFile);
commandMigrateUp.AddOption(optionDryRun);
commandMigrateUp.AddOption(optionConfirm);
commandMigrateUp.SetHandler(
    (config, dryRun) => { new MigrateUpCommand(config, dryRun).Perform(); },
    optionsBinding,
    optionDryRun
);

// scripts command
var commandMigrateScripts = new Command(
    "scripts",
    "Apply the database scripts."
);
commandMigrateScripts.AddOption(optionSchemaFile);
commandMigrateScripts.AddOption(optionDryRun);
commandMigrateScripts.AddOption(optionConfirm);
commandMigrateScripts.SetHandler(
    (config, dryRun) => { new MigrateScriptCommand(config, dryRun).Perform(); },
    optionsBinding,
    optionDryRun
);

// down command
var commandMigrateDown = new Command(
    "down",
    "Rollback the database migrations. NOT YET IMPLEMENTED"
)
{
    IsHidden = true // not yet implemented
};
commandMigrateDown.AddOption(optionSteps);
commandMigrateDown.AddOption(optionDryRun);
commandMigrateDown.AddOption(optionConfirm);
commandMigrateDown.SetHandler(
    (config, steps) => { new MigrateDownCommand(config, steps).Perform(); },
    optionsBinding,
    optionSteps
);

var commandMigrate = new Command(
    "migrate",
    "Database migration commands."
);
commandMigrate.AddCommand(commandMigrateCreate);
commandMigrate.AddCommand(commandMigrateUp);
commandMigrate.AddCommand(commandMigrateScripts);
commandMigrate.AddCommand(commandMigrateDown);

// ------------------------------------------------------------
// schema commands
//

// dump command
var commandSchemaDump = new Command(
    "dump",
    "Dumps the database schema to file."
);
commandSchemaDump.SetHandler(
    config => { new SchemaDumpCommand(config).Perform(); },
    optionsBinding
);

// load command
var commandSchemaLoad = new Command(
    "load",
    "Loads the database schema from file into an empty database."
);
commandSchemaLoad.AddOption(optionConfirm);
commandSchemaLoad.SetHandler(
    config => { new SchemaLoadCommand(config).Perform(); },
    optionsBinding
);

var commandSchema = new Command(
    "schema",
    "Database schema commands."
);
commandSchema.AddGlobalOption(optionSchemaFile);
commandSchema.AddCommand(commandSchemaDump);
commandSchema.AddCommand(commandSchemaLoad);

// ------------------------------------------------------------
// seeds commands
//

var commandSeeds = new Command(
    "seeds",
    "Apply the database data seed scripts."
);
commandSeeds.AddOption(optionDryRun);
commandSeeds.AddOption(optionConfirm);
commandSeeds.SetHandler(
    (config, dryRun) => { new SeedsCommand(config, dryRun).Perform(); },
    optionsBinding,
    optionDryRun
);


// ------------------------------------------------------------
// init commands
//

var commandInit = new Command(
    "init",
    "Initialise a project directory."
);
commandSeeds.AddOption(optionConfirm);
commandSeeds.SetHandler(
    (config) => { new InitialiseCommand(config).Perform(); },
    optionsBinding
);


//
// root command
//

var rootCommand = new RootCommand($"{Utilities.ToolName} - Database Migration Tool");
rootCommand.AddGlobalOption(optionConnectionString);
rootCommand.AddGlobalOption(optionScriptsPath);
rootCommand.AddGlobalOption(optionVariablesFile);
rootCommand.AddGlobalOption(optionMacrosFile);
rootCommand.AddGlobalOption(optionAssemblyBinariesPath);
rootCommand.AddGlobalOption(optionVerbose);
rootCommand.AddGlobalOption(optionDebug);

rootCommand.AddCommand(commandDatabase);
rootCommand.AddCommand(commandMigrate);
rootCommand.AddCommand(commandSchema);
rootCommand.AddCommand(commandSeeds);
rootCommand.AddCommand(commandInit);

try
{
    return rootCommand.Invoke(args);
}
catch (Exception e)
{
    BaseCommand.ReportError(e);
    return -1;
}
