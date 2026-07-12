using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace BarlogM_Unda;

[Injectable]
public class UpdateRaidConfigurationCallback(
    HttpResponseUtil httpResponseUtil,
    WeatherHelper weatherHelper,
    ModData modData
    )
{
    public ValueTask<string> UpdateRaidConfiguration(string url, GetRaidConfigurationRequestData info, MongoId sessionID)
    {
        modData.IsNightRaid = weatherHelper.IsNightTime(info.TimeVariant, info.Location);
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }
}