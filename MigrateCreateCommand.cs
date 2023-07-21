namespace Migratonator;

internal class MigrateCreateCommand : BaseCommand
{
    public MigrateCreateCommand(Options configuration, string name) :
        base(configuration)
    {
        Name = name;
    }

    private string Name { get; }

    public override void Perform()
    {
        var now = DateTime.Now;
        var dateStamp = now.ToString("yyyyMMddHHmmss");
        var cleanedName = Name.Replace(" ", "-");
        var fileName = $"{dateStamp}-{cleanedName}.sql";

        using var writer =
            File.CreateText(Path.Combine(Options.ScriptsPath, MigrationPath, fileName));
        writer.WriteLine("-- TODO: write migration script");
        writer.WriteLine("-- Use $DatabaseName$ and $SchemaName$ variables to reference the database and schema names");
        writer.WriteLine();
        writer.WriteLine("GO");
        writer.WriteLine();

        ReportSuccess($"Created migration script '{fileName}'!");
    }
}