using System;
using System.Linq;
using System.Reflection;
using RequeueRelief.CommandAPI;
using Vintagestory.API.Common;

namespace RequeueRelief.Commands.Settings;

internal class GetCommand : Command
{
    protected override string? Description { get; init; } = "View the current config value.";

    protected sealed override string? RequiredPrivilege { get; init; }

    protected sealed override Command[] SubCommands { get; init; }

    public GetCommand(Config config) : base("get")
    {
        RequiredPrivilege = config.Privileges.Configure;
        SubCommands = typeof(Config.TimingsConfig).GetFields()
            .Select(field => FieldToSetting(field, config))
            .ToArray<Command>();
    }

    /// <summary>
    /// Generates a subcommand from a field using reflection.
    /// </summary>
    private Setting FieldToSetting(FieldInfo field, Config config)
    {
        var name = field.Name;
        var description = field.GetCustomAttribute<ConfigDescriptionAttribute>()?.Value ?? "(No description available)";
        var units = field.GetCustomAttribute<ConfigUnitsAttribute>() ?? new ConfigUnitsAttribute(string.Empty, string.Empty);

        if (field.FieldType == typeof(double))
        {
            return new DoubleSetting(name, description, () => (double) field.GetValue(config.Timings)!, units);
        }

        throw new NotImplementedException($"Unsupported setting type: {field.FieldType}");
    }

    private abstract class Setting(string name, string description) : Command(name)
    {
        protected override string? Description { get; init; } = description;
    }

    private class DoubleSetting(string name, string description, Func<double> valueProvider, ConfigUnitsAttribute units) : Setting(name, description)
    {
        protected override TextCommandResult Handle(TextCommandCallingArgs args)
        {
            var value = valueProvider();
            return TextCommandResult.Success($"[RequeueRelief] The current value of {Name} is {value} {units.Resolve(value)}.");
        }
    }
}