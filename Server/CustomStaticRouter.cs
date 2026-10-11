using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace DynamicMaps;

/// <summary>
/// Serves the mod's marker-visibility config to the client at GET /dynamicmaps/load.
/// </summary>
/// <remarks>
/// The client reads this in <c>MenuLoadPatch</c> and binds it onto <c>ModdedMapScreen.ServerConfig</c>
/// (public fields, case-insensitive). Property names here therefore serialise as PascalCase and still bind.
/// </remarks>
[Injectable]
public class CustomStaticRouter : StaticRouter
{
    private static ModConfig? _modConfig;

    public CustomStaticRouter(
        JsonUtil jsonUtil) : base(
        jsonUtil,
        GetCustomRoutes()
    )
    {}

    public void PassConfig(ModConfig config)
    {
        _modConfig = config;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/dynamicmaps/load",
                (
                    url,
                    info,
                    sessionId,
                    output,
                    cancellationToken
                ) => new ValueTask<string>(JsonSerializer.Serialize(_modConfig))
            )
        ];
    }
}
