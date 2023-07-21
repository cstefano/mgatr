namespace Migratonator;

internal class MigrateDownCommand : BaseCommand
{
    public MigrateDownCommand(Options configuration, int steps) :
        base(configuration)
    {
        Steps = steps;
    }

    private int Steps { get; }

    public override void Perform()
    {
        // TODO: get user confirmation to continue
        Console.WriteLine($"Rolling back {Steps} steps...");
        throw new NotImplementedException();
    }
}