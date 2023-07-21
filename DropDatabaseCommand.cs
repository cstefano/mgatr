namespace Migratonator;

internal class DropDatabaseCommand : DatabaseCommand
{
    public DropDatabaseCommand(Options configuration) :
        base(configuration)
    {
        UserFriendlyAction = "Drop";
    }

    protected override string UserFriendlyAction { get; }

    protected override string SqlStatement(string databaseName)
    {
        return
            @$"
                IF DB_ID(N'{databaseName}') IS NOT NULL
                  DROP DATABASE [{databaseName}];
                GO
            ";
    }
}