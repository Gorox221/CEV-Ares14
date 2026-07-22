using Robust.Shared.Configuration;
using Content.Shared.Atmos;
using Robust.Shared;

namespace Content.Shared._ADT.CCVar;

[CVarDefs]
public sealed class ADTCCVars
{
    public static readonly CVarDef<string> HeadshotDomain =
        CVarDef.Create("ic.headshot_domain", "i.pinimg.com", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Maximum size of headshot images in bytes (default: 5MB).
    /// </summary>
    public static readonly CVarDef<int> HeadshotMaxSize =
        CVarDef.Create("ic.headshot_max_size", 5 * 1024 * 1024, CVar.SERVER);

    /// <summary>
    /// Headshot cache duration in minutes (default: 30 minutes).
    /// </summary>
    public static readonly CVarDef<float> HeadshotCacheDuration =
        CVarDef.Create("ic.headshot_cache_duration", 30f, CVar.SERVER);

    /// <summary>
    /// Maximum number of headshot images in cache (default: 100).
    /// </summary>
    public static readonly CVarDef<int> HeadshotMaxCacheCount =
        CVarDef.Create("ic.headshot_max_cache_count", 100, CVar.SERVER);
}

