namespace Migratonator;

internal class CreateDatabaseCommand : DatabaseCommand
{
    public CreateDatabaseCommand(Options configuration) :
        base(configuration)
    {
        UserFriendlyAction = "Create";
    }

    protected override string UserFriendlyAction { get; }

    protected override string SqlStatement(string databaseName)
    {
        return
            @$"
                IF DB_ID(N'{databaseName}') IS NULL
                  CREATE DATABASE [{databaseName}];
                GO
            ";
    }
}