using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace BarlogM_Unda;

[Injectable]
public class PmcWaveGeneratorPatch : AbstractPatch
{
    private static BotConfig botConfig = default!;
    private static LocationConfig locationConfig = default!;
    private static RandomUtil randomUtil = default!;
    private static ISptLogger<PmcWaveGeneratorPatch> logger = default!;
    private static Data data = default!;
    private static Config config = default!;

    public PmcWaveGeneratorPatch(
        BotConfig botConfig,
        LocationConfig locationConfig,
        RandomUtil randomUtil,
        ISptLogger<PmcWaveGeneratorPatch> logger,
        Data data,
        ConfigProvider configProvider
    )
    {
        PmcWaveGeneratorPatch.botConfig = botConfig;
        PmcWaveGeneratorPatch.locationConfig = locationConfig;
        PmcWaveGeneratorPatch.randomUtil = randomUtil;
        PmcWaveGeneratorPatch.logger = logger;
        PmcWaveGeneratorPatch.data = data;
        config = configProvider.config;
    }
    
    protected override MethodBase GetTargetMethod()
    {
        /*
        return AccessTools.Method(
            typeof(PmcWaveGenerator),
            "ApplyWaveChangesToMap",
            new[] { typeof(LocationBase) }
        );
        */
        
        return typeof(PmcWaveGenerator).GetMethod(nameof(PmcWaveGenerator.ApplyWaveChangesToMap)) ?? throw new InvalidOperationException("Could not find target method!");
    }

    [PatchPrefix]
    public static bool Prefix(LocationBase location)
    {
        if (config.Debug)
        {
            logger.LogWithColor(
                "[Unda] path for PmcWaveGenerator.ApplyWaveChangesToMap",
                Spectre.Console.Color.Yellow);
        }

        ApplyWaveChangesToMap(location);

        return false;
    }
    
    static void ApplyWaveChangesToMap(LocationBase location)
    {
        DeleteAllPmcBosses(location);
        var locationId = location.Id.ToLower();
        DeleteAllCustomWaves(locationId);

        var isNightRaid = data.IsNightRaid && locationId is not ("laboratory" or "labyrinth");

        if (config.Debug && isNightRaid)
        {
            logger.LogWithColor(
                $"[Unda] night quiet raid",
                Spectre.Console.Color.Blue);
        }

        if (!isNightRaid && !randomUtil.GetChance100(config.ChanceForQuietRaid))
        {
            UpdateMaxBotsAmount(location);
        }

        GeneratePmcBossWaves(location);
        ReplaceScavWaves(location);
    }

    static void DeleteAllPmcBosses(LocationBase location)
    {
        location.BossLocationSpawn = location.BossLocationSpawn
            .Where(bossLocationSpawn =>
                bossLocationSpawn.BossName != "pmcBEAR" &&
                bossLocationSpawn.BossName != "pmcUSEC")
            .ToList();

        if (config.Debug)
        {
            logger.LogWithColor(
                $"[Unda] delete all pmc bosses on location '{location.Name}' location.BossLocationSpawn: {JsonSerializer.Serialize(location.BossLocationSpawn)}",
                Spectre.Console.Color.Blue);
        }
    }

    static void DeleteAllCustomWaves(string locationName)
    {
        locationConfig.CustomWaves!.Boss[locationName] = [];
        locationConfig.CustomWaves.Normal[locationName] = [];

        if (config.Debug)
        {
            logger.LogWithColor(
                $"[Unda] after delete locationConfig.customWaves.boss: {JsonSerializer.Serialize(locationConfig.CustomWaves.Boss)}",
                Spectre.Console.Color.Blue);
        }
    }

    static void UpdateMaxBotsAmount(LocationBase location)
    {
        var locationId = location.Id.ToLower();
        var generalLocationInfo = data.GeneralLocationInfo[locationId];

        if (Data.SmallMaps.Contains(locationId))
        {
            var maxBots = generalLocationInfo.MaxBots;
            var maxPlayers = generalLocationInfo.MaxPlayers;
            IncreaseMaxBotsAmountForSmallLocation(location, maxBots,
                maxPlayers);
        }
        else
        {
            var maxBots = generalLocationInfo.MaxBots;
            var minPlayers = generalLocationInfo.MinPlayers;
            IncreaseMaxBotsAmountForLargeLocation(location, maxBots,
                minPlayers);
        }
    }

    static int IncreaseMaxBotsAmountForLargeLocation(
        LocationBase location,
        int maxBots,
        int minPlayers)
    {
        var term = randomUtil.GetInt(
            (int)Math.Round(minPlayers / 2.0),
            minPlayers + 1);
        return IncreaseMaxBotsAmountForLocation(location, maxBots, term);
    }

    static int IncreaseMaxBotsAmountForSmallLocation(
        LocationBase location,
        int maxBots,
        int maxPlayers)
    {
        var term = randomUtil.GetInt(
            (int)Math.Round(maxPlayers / 2.0),
            maxPlayers - 1);
        return IncreaseMaxBotsAmountForLocation(location, maxBots, term);
    }

    static int IncreaseMaxBotsAmountForLocation(
        LocationBase location,
        int maxBots,
        int term)
    {
        var newMaxBotsValue = maxBots + term;
        location.BotMax = newMaxBotsValue;

        var locationId = location.Id.ToLower();
        botConfig.MaxBotCap[locationId] = newMaxBotsValue;

        if (config.Debug)
        {
            logger.LogWithColor(
                $"[Unda] {locationId}.BotMax: {maxBots} -> {newMaxBotsValue}", Spectre.Console.Color.Blue);
        }

        return newMaxBotsValue;
    }

    static void GeneratePmcBossWaves(LocationBase location)
    {
        var locationId = location.Id.ToLower();
        if (locationId is "labyrinth") return;

        /*
        var minPlayers = data.GeneralLocationInfo[locationId].MinPlayers;
        var maxPlayers = data.GeneralLocationInfo[locationId].MaxPlayers;

        var maxPmcAmount = randomUtil.GetInt(minPlayers, maxPlayers) - 1;
        if (maxPmcAmount <= 0)
        {
            logger.Error(
                $"[Unda] {locationId}.maxPlayers: {maxPmcAmount}");
        }

        var groups =
            SplitMaxAmountIntoGroups(maxPmcAmount, _modConfig.MaxPmcGroupSize);

        if (_modConfig.Debug)
        {
            logger.LogWithColor(
                $"[Unda] '{locationId}' PMC groups {JsonSerializer.Serialize(groupsByZones)}", Spectre.Console.Color.Blue);
        }

        foreach (var groupByZone in groupsByZones)
        {
            location.BossLocationSpawn.Add(GeneratePmcAsBoss(groupByZone.GroupSize, _modConfig.PmcBotDifficulty));
        }
        */

        var maxPmcAmount = data.GeneralLocationInfo[locationId].MinPlayers - 1;
        if (maxPmcAmount <= 0)
        {
            logger.Error(
                $"[Unda] {locationId}.maxPlayers: {maxPmcAmount}");
        }

        var groups =
            SplitMaxAmountIntoGroups(maxPmcAmount, config.MaxPmcGroupSize);

        foreach (var group in groups)
        {
            location.BossLocationSpawn.Add(GeneratePmcAsBoss(group, config.PmcBotDifficulty));
        }

        if (config.Debug)
        {
            logger.LogWithColor(
                $"[Unda] location.BossLocationSpawn '{locationId}': {JsonSerializer.Serialize(location.BossLocationSpawn)}",
                Spectre.Console.Color.Blue);
        }
    }

    static List<ZoneGroupSize> SeparateGroupsByZones(List<string> zones,
        List<int> groups)
    {
        var shuffledZones = ShuffleZonesArray(zones);
        var groupsPool = new List<int>(groups);
        var result = new List<ZoneGroupSize>();

        foreach (var zoneName in shuffledZones)
        {
            if (!groupsPool.Any()) break;

            var groupSize = groupsPool[^1];
            groupsPool.RemoveAt(groupsPool.Count - 1);

            result.Add(new ZoneGroupSize
            {
                ZoneName = zoneName,
                GroupSize = groupSize
            });
        }

        return result;
    }

    static BossLocationSpawn GeneratePmcAsBoss(int groupSize, string difficulty)
    {
        var supports = new List<BossSupport>();
        var escortAmount = "0";

        var type = randomUtil.GetBool() ? "pmcBEAR" : "pmcUSEC";

        if (groupSize > 1)
        {
            escortAmount = $"{groupSize - 1}";

            supports.Add(new BossSupport
            {
                BossEscortType = type,
                BossEscortDifficulty = new ListOrT<string>([difficulty], null),
                BossEscortAmount = escortAmount
            });
        }

        return new BossLocationSpawn
        {
            BossChance = 100,
            BossDifficulty = difficulty,
            BossEscortAmount = escortAmount,
            BossEscortDifficulty = difficulty,
            BossEscortType = type,
            BossName = type,
            IsBossPlayer = true,
            BossZone = "",
            IsRandomTimeSpawn = false,
            ShowOnTarkovMap = false,
            ShowOnTarkovMapPvE = false,
            Time = -1,
            TriggerId = "",
            TriggerName = "",
            ForceSpawn = null,
            IgnoreMaxBots = true,
            DependKarma = null,
            DependKarmaPVE = null,
            Supports = supports,
            SptId = null,
            SpawnMode = new List<string> { "pve" }
        };
    }

    static List<int> SplitMaxAmountIntoGroups(int maxAmount, int maxGroupSize)
    {
        var result = new List<int>();
        var remainingAmount = maxAmount;

        do
        {
            var generatedGroupSize = randomUtil.GetInt(1, maxGroupSize);
            if (generatedGroupSize > remainingAmount)
            {
                result.Add(remainingAmount);
                remainingAmount = 0;
            }
            else
            {
                result.Add(generatedGroupSize);
                remainingAmount -= generatedGroupSize;
            }
        } while (remainingAmount > 0);

        return result;
    }

    static List<string> ShuffleZonesArray(List<string> array)
    {
        return randomUtil.Shuffle(array);
    }

    static void ReplaceScavWaves(LocationBase location)
    {
        var locationId = location.Id.ToLower();

        if (locationId is "laboratory" or "labyrinth") return;

        var marksmanZones =
            data.GeneralLocationInfo[locationId].MarksmanZones;
        var assaultZones =
            new List<string>(data.GeneralLocationInfo[locationId].Zones);

        /*
        if (locationId == "rezervbase")
        {
            // for Rezerv zone name should be empty string
            for (int i = 0; i < assaultZones.Count; i++)
                assaultZones[i] = "";
        }
        */

        // replace zones with empty names with BotZone
        for (var i = 0; i < assaultZones.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(assaultZones[i]))
                assaultZones[i] = "BotZone";
        }

        CleanWaves(location);

        var maxMarksmanGroupSize = locationId == "shoreline" ? 2 : 1;
        var currentWaveNumber = GenerateMarksmanWaves(
            location,
            marksmanZones,
            maxMarksmanGroupSize);

        int maxAssaultScavAmount =
            data.GeneralLocationInfo[locationId].MaxScavs;
        if (maxAssaultScavAmount <= 0)
        {
            logger.Error(
                $"[Unda] {locationId}.BotMax: {maxAssaultScavAmount}");
        }

        int maxScavGroupSize =
            locationId == "tarkovstreets"
                ? 3
                : config.MaxScavGroupSize;

        GenerateAssaultWaves(
            location,
            assaultZones,
            (int)location.EscapeTimeLimit!,
            maxAssaultScavAmount,
            maxScavGroupSize,
            currentWaveNumber);

        if (config.Debug)
        {
            logger.LogWithColor(
                $"[Unda] {locationId}.waves: {JsonSerializer.Serialize(location.Waves)}",
                Spectre.Console.Color.Blue);
        }
    }

    static void CleanWaves(LocationBase locationBase)
    {
        locationBase.Waves.Clear();
    }

    static int GenerateMarksmanWaves(
        LocationBase locationBase,
        HashSet<string> zones,
        int maxGroupSize)
    {
        var num = 0;
        var minGroupSize = maxGroupSize > 1 ? 1 : 0;

        foreach (var zone in zones)
        {
            locationBase.Waves.Add(
                GenerateWave(
                    WildSpawnType.marksman,
                    zone,
                    "hard",
                    num++,
                    minGroupSize,
                    maxGroupSize,
                    -1,
                    -1));
        }

        return num;
    }

    static void GenerateAssaultWaves(
        LocationBase location,
        List<string> zones,
        int escapeTimeLimit,
        int maxAssaultScavAmount,
        int maxScavGroupSize,
        int currentWaveNumber)
    {
        var groups =
            SplitMaxAmountIntoGroups(maxAssaultScavAmount, maxScavGroupSize);
        var groupsByZones =
            SeparateGroupsByZones(zones, groups);

        if (config.Debug)
        {
            logger.LogWithColor(
                $"[Unda] '{location.Id.ToLowerInvariant()}' scav groups {JsonSerializer.Serialize(groupsByZones)}",
                Spectre.Console.Color.Blue);
        }

        var firstWaveTimeMin = 60;
        var lastWaveTimeMin = (int)Math.Ceiling((escapeTimeLimit * 60) / 2.0);
        var middleWaveTimeMin = (int)Math.Ceiling(lastWaveTimeMin / 2.0);

        var difficulty = randomUtil.GetBool() ? "normal" : "hard";

        CreateAssaultWaves(groupsByZones, location, difficulty, firstWaveTimeMin,
            ref currentWaveNumber);
        CreateAssaultWaves(groupsByZones, location, difficulty, middleWaveTimeMin,
            ref currentWaveNumber);
        CreateAssaultWaves(groupsByZones, location, difficulty, lastWaveTimeMin,
            ref currentWaveNumber);
    }

    static void CreateAssaultWaves(
        List<ZoneGroupSize> groupsByZones,
        LocationBase locationBase,
        string difficulty,
        int timeMin,
        ref int currentWaveNumber)
    {
        const int MIN_GROUP_SIZE = 1;
        var timeMax = timeMin + 120;

        foreach (var zoneByGroup in groupsByZones)
        {
            var wave = GenerateWave(
                WildSpawnType.assault,
                zoneByGroup.ZoneName,
                difficulty,
                currentWaveNumber++,
                MIN_GROUP_SIZE,
                zoneByGroup.GroupSize,
                timeMin,
                timeMax);
            locationBase.Waves.Add(wave);
        }
    }

    static Wave GenerateWave(
        WildSpawnType botType,
        string zoneName,
        string difficulty,
        int number,
        int slotsMin,
        int slotsMax,
        int timeMin,
        int timeMax)
    {
        return new Wave
        {
            BotPreset = difficulty,
            BotSide = "Savage",
            KeepZoneOnSpawn = false,
            SpawnPoints = zoneName,
            WildSpawnType = botType,
            IsPlayers = false,
            Number = number,
            SlotsMin = slotsMin,
            SlotsMax = slotsMax,
            TimeMin = timeMin,
            TimeMax = timeMax,
            SpawnMode = ["pve"]
        };
    }
}