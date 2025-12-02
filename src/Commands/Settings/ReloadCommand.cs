using RequeueRelief.CommandAPI;
using Vintagestory.API.Common;

namespace RequeueRelief.Commands.Settings;

internal class ReloadCommand(Config config, ICoreAPI api) : Command("reloadtimings")
{
    protected override string? Description { get; init; } = "Reloads the queue timings configuration from the config file";

    protected override string? RequiredPrivilege { get; init; } = config.Privileges.Configure;

    protected override TextCommandResult Handle(TextCommandCallingArgs args)
    {
        var newConfig = Config.Load(api);
        foreach (var fieldInfo in typeof(Config.TimingsConfig).GetFields())
        {
            fieldInfo.SetValue(config.Timings, fieldInfo.GetValue(newConfig.Timings));
        }
        return TextCommandResult.Success($"[RequeueRelief] Timing configuration reloaded from config file.");
    }
}