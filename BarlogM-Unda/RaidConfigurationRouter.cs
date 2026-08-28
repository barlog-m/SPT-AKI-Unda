using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace BarlogM_Unda;

[Injectable]
public class RaidConfigurationRouter(
    JsonUtil jsonUtil,
    UpdateRaidConfigurationCallback updateRaidConfigurationCallback
)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<GetRaidConfigurationRequestData>(
                "/client/raid/configuration",
                async (url, info, sessionID, output, cancellationToken) => await updateRaidConfigurationCallback.UpdateRaidConfiguration(url, info, sessionID)
            ),
        ]
    )
{
}
