using System.Reflection;
using HarmonyLib;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Location;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Services.InRaid;

namespace BarlogM_Unda;

[Injectable]
public class RaidTimeAdjustmentServicePatch: AbstractPatch
{
    private static ISptLogger<RaidTimeAdjustmentServicePatch> logger = default!;
    private static Config config = default!;

    public RaidTimeAdjustmentServicePatch(ISptLogger<RaidTimeAdjustmentServicePatch> logger, ConfigProvider configProvider)
    {
        RaidTimeAdjustmentServicePatch.logger = logger;
        config = configProvider. config;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(RaidTimeAdjustmentService),
            "AdjustPMCSpawns",
            new[] { typeof(LocationBase), typeof(RaidChanges) }
        );
    }

    [PatchPrefix]
    public static bool Prefix(LocationBase mapBase, RaidChanges raidAdjustments)
    {
        if (config.Debug)
        {
            logger.LogWithColor(
                "[Unda] path for RaidTimeAdjustmentService.AdjustPMCSpawns",
                Spectre.Console.Color.Yellow);
        }

        AdjustPMCSpawns(mapBase);

        return false;
    }

    static int GetBossPmcSpawnCount(List<BossLocationSpawn> bossLocationSpawns)
    {
        return bossLocationSpawns.Count(spawn =>
        {
            return string.Equals(spawn.BossName, "pmcusec",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(spawn.BossName, "pmcbear",
                       StringComparison.OrdinalIgnoreCase);
        });
    }

    static void AdjustPMCSpawns(LocationBase mapBase)
    {
        var result = new List<BossLocationSpawn>();

        var originalPmcWaveCount = GetBossPmcSpawnCount(mapBase.BossLocationSpawn);
        var skip = (int)(originalPmcWaveCount * 0.5);

        if (config.Debug)
        {
            logger.LogWithColor($"[Unda] original: {originalPmcWaveCount} removed: {skip} PMC waves",
                Spectre.Console.Color.Blue);
        }

        foreach (var spawn in mapBase.BossLocationSpawn)
        {
            if (skip > 0 && (
                    string.Equals(spawn.BossName, "pmcusec",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(spawn.BossName, "pmcbear",
                        StringComparison.OrdinalIgnoreCase)
                ))
            {
                skip--;
                continue;
            }

            result.Add(spawn);
        }

        mapBase.BossLocationSpawn = result;
    }
}