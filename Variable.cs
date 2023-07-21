using System.Text.Json;

namespace Migratonator;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Variable
{
    public static IDictionary<string, string> LoadVariables(string fileName)
    {
        List<Variable>? variables;
        using (var reader = new StreamReader(fileName))
        {
            var json = reader.ReadToEnd();
            variables = JsonSerializer.Deserialize<List<Variable>>(json);
        }

        return variables == null ? new Dictionary<string, string>() : variables.ToDictionary(v => v.Name, v => v.Value);
    }

    public Variable()
    {
        Name = string.Empty;
        Value = string.Empty;
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Name { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Value { get; set; }
}