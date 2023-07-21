using System.Reflection;

namespace Migratonator;

internal static class Utilities
{
    public static string ToolName
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            return $"Migratonator v{assembly.GetName().Version}";
        }
    }
}