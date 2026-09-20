using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PickMyRandoms.PickMyRandomsCode;

[HarmonyPatch(typeof(NCharacterSelectButton), "Select")]
public static class MpRandomPatch
{
    private static readonly Random Roll = new();
    private static readonly AccessTools.FieldRef<NCharacterSelectButton, ICharacterSelectButtonDelegate> DelegateRef =
        AccessTools.FieldRefAccess<NCharacterSelectButton, ICharacterSelectButtonDelegate>("_delegate");

    public static bool Prefix(NCharacterSelectButton __instance)
    {
        if (!__instance.IsRandom || PoolConfig.ResolveOnEmbark)
            return true;

        ICharacterSelectButtonDelegate? selectDelegate = DelegateRef(__instance);
        if (selectDelegate == null || selectDelegate.Lobby.NetService.Type == NetGameType.Singleplayer)
            return true;

        List<NCharacterSelectButton> choices = __instance.GetParent().GetChildren()
            .OfType<NCharacterSelectButton>()
            .Where(button => !button.IsRandom && button.Visible && !button.IsLocked)
            .ToList();
        if (!choices.Any(button => ExcludedPool.IsExcluded(button.Character.Id.ToString())))
            return true;

        List<NCharacterSelectButton> allowed = choices
            .Where(button => !ExcludedPool.IsExcluded(button.Character.Id.ToString()))
            .ToList();
        if (allowed.Count == 0)
            return true;

        NCharacterSelectButton pick = allowed[Roll.Next(allowed.Count)];
        pick.Select();
        MainFile.Logger.Info($"MP: Random redirected to {pick.Character.Id} (excluded pool respected)");
        return false;
    }
}

[HarmonyPatch(typeof(StartRunLobby), nameof(StartRunLobby.SetReady))]
public static class MpEmbarkResolvePatch
{
    private static readonly Random Roll = new();
    private static bool _forcedPick;
    private static ulong _localId;

    public static bool IsMasked(ulong playerId) => _forcedPick && playerId == _localId;
    public static void ResetState() => _forcedPick = false;

    public static void Prefix(StartRunLobby __instance, bool ready)
    {
        if (!PoolConfig.ResolveOnEmbark || __instance.NetService.Type == NetGameType.Singleplayer)
            return;

        if (!ready)
        {
            if (_forcedPick)
            {
                _forcedPick = false;
                __instance.SetLocalCharacter(ModelDb.Character<RandomCharacter>());
                MainFile.Logger.Info("MP: un-ready, selection restored to Random");
            }
            return;
        }

        LobbyPlayer localPlayer = __instance.LocalPlayer;
        if (localPlayer.character is not RandomCharacter)
            return;

        List<CharacterModel> allCharacters = ModelDb.AllCharacters.ToList();
        List<CharacterModel> allowed = allCharacters
            .Where(character => !ExcludedPool.IsExcluded(character.Id.ToString()))
            .ToList();
        if (allowed.Count == 0 || allowed.Count == allCharacters.Count)
            return;

        CharacterModel pick = allowed[Roll.Next(allowed.Count)];
        _localId = localPlayer.id;
        _forcedPick = true;
        __instance.SetLocalCharacter(pick);
        MainFile.Logger.Info($"MP: Random resolved at embark to {pick.Id} (hidden locally)");
    }
}

[HarmonyPatch(typeof(NRemoteLobbyPlayerContainer), "OnPlayerChanged")]
public static class MaskLocalForcedPickPatch
{
    public static void Prefix(ref LobbyPlayer player)
    {
        if (MpEmbarkResolvePatch.IsMasked(player.id))
            player.character = ModelDb.Character<RandomCharacter>();
    }
}

[HarmonyPatch]
public static class EmbarkResolveResetPatch
{
    private static IEnumerable<MethodBase> TargetMethods() =>
        AccessTools.GetDeclaredConstructors(typeof(StartRunLobby));

    public static void Postfix() => MpEmbarkResolvePatch.ResetState();
}
