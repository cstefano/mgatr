using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DbUp.Engine;

namespace Migratonator;

internal partial class SqlClrAssemblyPreprocessor : IScriptPreprocessor
{
    public SqlClrAssemblyPreprocessor(string assemblySearchPath)
    {
        AssemblySearchPath = assemblySearchPath;
    }

    private string AssemblySearchPath { get; }

    public string Process(string contents)
    {
        // look for variables in the script which denote the SQL CLR assembly
        // to be loaded into the database.  The variable is replaced with the
        // the hex encoded contents of the assembly.
        //
        // the format of the variable is:
        //    
        // %SqlCLRHex:AbsoluteOrRelativePathToAssembly%
        //

        foreach (Match match in SqlClrHexRegex().Matches(contents))
        {
            var path = match.Groups["path"].Value;

            // construct full path
            if (!Path.IsPathRooted(path))
                path = Path.Combine(AssemblySearchPath, path);

            if (File.Exists(path))
            {
                // HEX encode the contents of the assembly
                var hexString = GetHexString(path);
                contents = contents.Replace(match.Value, hexString);
            }
            else
            {
                throw new FileNotFoundException("SQL CLR Assembly not found", path);
            }
        }

        return contents;
    }

    [GeneratedRegex("%SqlCLRHex:(?<path>[^%]+)%", RegexOptions.Multiline)]
    private static partial Regex SqlClrHexRegex();

    private static string GetHexString(string assemblyPath)
    {
        var builder = new StringBuilder();
        builder.Append("0x");
        using (var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var currentByte = stream.ReadByte();
            while (currentByte > -1)
            {
                builder.Append(currentByte.ToString("X2", CultureInfo.InvariantCulture));
                currentByte = stream.ReadByte();
            }
        }

        return builder.ToString();
    }
}