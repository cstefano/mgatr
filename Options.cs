using Microsoft.Data.SqlClient;

namespace Migratonator;

internal class Options
{
    private string _databaseName;
    private string _schemaName;

    public Options()
    {
        ConnectionString = string.Empty;
        ExecutionTimeout = null;
        ScriptsPath = string.Empty;
        VariablesFile = string.Empty;
        MacrosFile = string.Empty;
        SchemaFile = string.Empty;
        AssemblyBinariesPath = string.Empty;
        Confirm = true;
        Verbose = false;
        Debug = false;

        _databaseName = string.Empty;
        _schemaName = string.Empty;
    }

    public string ConnectionString { get; init; }

    public string DatabaseName
    {
        get
        {
            if (string.IsNullOrEmpty(_databaseName))
                LoadDatabaseNameAndSchemaName();
            return _databaseName;
        }
    }

    public string SchemaName
    {
        get
        {
            if (string.IsNullOrEmpty(_schemaName))
                LoadDatabaseNameAndSchemaName();
            return _schemaName;
        }
    }

    public TimeSpan? ExecutionTimeout { get; init; }
    public string ScriptsPath { get; init; }
    public string VariablesFile { get; init; }
    public string MacrosFile { get; init; }
    public string SchemaFile { get; init; }
    public string AssemblyBinariesPath { get; init; }
    public bool Confirm { get; init; }
    public bool Verbose { get; init; }
    public bool Debug { get; init; }

    private void LoadDatabaseNameAndSchemaName()
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = new SqlCommand("SELECT DB_NAME(), SCHEMA_NAME()", connection);
        var data = command.ExecuteReader();
        if (data.Read())
        {
            _databaseName = data.GetString(0);
            _schemaName = data.GetString(1);
        }
        else
        {
            throw new InvalidOperationException("Unable to determine database name and schema.");
        }
    }
}
