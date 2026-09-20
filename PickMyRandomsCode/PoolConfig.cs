using BaseLib.Config;

namespace PickMyRandoms.PickMyRandomsCode;

public class PoolConfig : SimpleModConfig
{
    // csv of excluded character ids; edited via right-click in character select,
    // exposed here mostly for persistence (and manual recovery)
    public static string ExcludedIds { get; set; } = "";

    // In multiplayer, preserve Random until ready-up. Set false for instant mode.
    public static bool ResolveOnEmbark { get; set; } = true;
}