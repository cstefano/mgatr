using System.Text.Json;
using System.Diagnostics;

namespace Migratonator;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Macro
{
    public static IDictionary<string, Macro> LoadMacros(string fileName)
    {
        List<Macro>? macros;
        using (var reader = new StreamReader(fileName))
        {
            var json = reader.ReadToEnd();
            macros = JsonSerializer.Deserialize<List<Macro>>(json);
        }

        return macros == null ? new Dictionary<string, Macro>() : macros.ToDictionary(v => v.Name, v => v);
    }

    public Macro()
    {
        Name = string.Empty;
        Template = string.Empty;
        Parameters = Array.Empty<string>();
    }

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string Name { get; set; }

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string Template { get; set; }

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string[] Parameters { get; set; }

    public string Render(string parameterString)
    {
        var parameters = ParseParameters(parameterString);

        // TODO: raise helpful error
        Debug.Assert(parameters.Length == Parameters.Length);

        var rendered = Template;
        Parameters.EachWithIndex((name, index) =>
        {
            rendered = rendered.Replace($"%{name}%", parameters[index], StringComparison.Ordinal);
        });
        return rendered;
    }

    private static string[] ParseParameters(string parameters)
    {
        return parameters.Split('|');
    }
}