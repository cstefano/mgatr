using System.CommandLine;
using System.CommandLine.Binding;
using System.Diagnostics;

namespace Migratonator;

// https://learn.microsoft.com/en-us/dotnet/standard/commandline/model-binding

internal class OptionsBinder : BinderBase<Options>
{
    private readonly Option<string> _optionConnectionString;
    private readonly Option<TimeSpan?> _optionExecutionTimeout;
    private readonly Option<string> _optionScriptsPath;
    private readonly Option<string> _optionVariablesFile;
    private readonly Option<string> _optionMacrosFile;
    private readonly Option<string> _optionSchemaFile;
    private readonly Option<string> _optionAssemblyBinariesPath;
    private readonly Option<bool> _optionConfirm;
    private readonly Option<bool> _optionVerbose;
    private readonly Option<bool> _optionDebug;

    public OptionsBinder(
        Option<string> optionConnectionString,
        Option<TimeSpan?> optionExecutionTimeout,
        Option<string> optionScriptsPath,
        Option<string> optionVariablesFile,
        Option<string> optionMacrosFile,
        Option<string> optionSchemaFile,
        Option<string> optionAssemblyBinariesPath,
        Option<bool> optionConfirm,
        Option<bool> optionVerbose,
        Option<bool> optionDebug)
    {
        _optionConnectionString = optionConnectionString;
        _optionExecutionTimeout = optionExecutionTimeout;
        _optionScriptsPath = optionScriptsPath;
        _optionVariablesFile = optionVariablesFile;
        _optionMacrosFile = optionMacrosFile;
        _optionSchemaFile = optionSchemaFile;
        _optionAssemblyBinariesPath = optionAssemblyBinariesPath;
        _optionConfirm = optionConfirm;
        _optionVerbose = optionVerbose;
        _optionDebug = optionDebug;
    }

    protected override Options GetBoundValue(BindingContext bindingContext)
    {
#pragma warning disable CS8601
        return new Options()
        {
            ConnectionString = bindingContext.ParseResult.GetValueForOption(_optionConnectionString),
            ExecutionTimeout = bindingContext.ParseResult.GetValueForOption(_optionExecutionTimeout),
            ScriptsPath = bindingContext.ParseResult.GetValueForOption(_optionScriptsPath),
            VariablesFile = bindingContext.ParseResult.GetValueForOption(_optionVariablesFile),
            MacrosFile = bindingContext.ParseResult.GetValueForOption(_optionMacrosFile),
            SchemaFile = bindingContext.ParseResult.GetValueForOption(_optionSchemaFile),
            AssemblyBinariesPath = bindingContext.ParseResult.GetValueForOption(_optionAssemblyBinariesPath),
            Confirm = bindingContext.ParseResult.GetValueForOption(_optionConfirm),
            Verbose = bindingContext.ParseResult.GetValueForOption(_optionVerbose),
            Debug = bindingContext.ParseResult.GetValueForOption(_optionDebug)
        };
#pragma warning restore CS8601
    }
}