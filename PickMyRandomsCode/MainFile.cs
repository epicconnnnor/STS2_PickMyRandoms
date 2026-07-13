using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace PickMyRandoms.PickMyRandomsCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "PickMyRandoms";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static PoolConfig Config { get; private set; } = null!;

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll();

        Config = new PoolConfig();
        ModConfigRegistry.Register(ModId, Config);
        ExcludedPool.LoadFromConfig();
    }
}