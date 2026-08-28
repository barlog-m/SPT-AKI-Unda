using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Utils;

namespace BarlogM_Unda;

[Injectable(InjectionType.Singleton)]
public class ConfigProvider
{
    public readonly Config config;
    public readonly string PathToMod;

    public ConfigProvider(ISptLogger<ConfigProvider> logger, ModHelper modHelper, JsonUtil jsonUtil)
    {
        PathToMod =
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        var pathToConfig = Path.Join(PathToMod, "config");
        config = jsonUtil.Deserialize<Config>(
            modHelper.GetRawFileData(pathToConfig, "config.json"))!;
        
        if (config.Debug)
        {
            logger.LogWithColor(
                "[Unda] config loaded",
                Spectre.Console.Color.Yellow);
        }
    }
}
