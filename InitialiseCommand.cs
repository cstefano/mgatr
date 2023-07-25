namespace Migratonator;

internal class InitialiseCommand : ICommand
{
    public InitialiseCommand(Options configuration)
    {
        Configuration = configuration;
    }

    private Options Configuration { get; }

    public void Perform()
    {
        // TODO: create empty directories for scripts, migrations, and seeds
        // TODO: create empty variables and macros file
        throw new NotImplementedException();
    }
}