using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace BarlogM_Unda;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "li.barlog.unda";
    public string Name { get; init; } = "Unda";
    public string Author { get; init; } = "Barlog_M";
    public List<string>? Contributors { get; init; } = [];
    public SemanticVersioning.Version Version { get; init; } = new("3.2.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.3");
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new();
    public string? Url { get; init; } = "https://github.com/barlog-m/spt-unda";
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 1)]
public class Unda(
    ISptLogger<Unda> logger,
    IEnumerable<IRuntimePatch> patches
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        foreach (var patch in patches)
        {
            patch.Enable();
        }

        return Task.CompletedTask;
    }
}