using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Game;
namespace PickMyRandoms.PickMyRandomsCode;

// The game resolves "Random" deterministically from the seed on every client, so
// altering the roll in multiplayer would desync character choices between machines.
// Singleplayer only: when the vanilla roll lands on an excluded character, re-roll
// from the allowed pool.
[HarmonyPatch(typeof(StartRunLobby), "ChangeCharacter")]
public static class RandomRollPatch
{
    private static readonly System.Random Reroll = new();

    public static void Prefix(StartRunLobby __instance, ref CharacterModel character, bool isRandomCharacterResolution)
    {
        if (!isRandomCharacterResolution)
            return;

        if (__instance.NetService.Type != NetGameType.Singleplayer)
            return;

        if (!ExcludedPool.IsExcluded(character.Id.ToString()))
            return; // vanilla roll already allowed, keep it

        List<CharacterModel> allowed = ModelDb.AllCharacters
            .Where(c => !ExcludedPool.IsExcluded(c.Id.ToString()))
            .ToList();

        if (allowed.Count == 0)
            return; // everything excluded: fall back to vanilla behavior

        character = allowed[Reroll.Next(allowed.Count)];
        MainFile.Logger.Info($"Random re-rolled to {character.Id} (vanilla pick was excluded)");
    }
}