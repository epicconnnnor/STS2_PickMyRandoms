using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PickMyRandoms.PickMyRandomsCode;

// Right-click a character portrait to exclude it from random. Excluded portraits dim.
[HarmonyPatch(typeof(NCharacterSelectButton), "Init")]
public static class ButtonTogglePatch
{
    public static void Postfix(NCharacterSelectButton __instance)
    {
        if (__instance.IsRandom)
            return;

        ApplyDim(__instance);

        __instance.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true })
            {
                ExcludedPool.Toggle(__instance.Character.Id.ToString());
                ApplyDim(__instance);
            }
        };
    }

    private static void ApplyDim(NCharacterSelectButton button)
    {
        bool excluded = ExcludedPool.IsExcluded(button.Character.Id.ToString());
        button.Modulate = excluded
            ? new Color(0.45f, 0.45f, 0.45f, 1f)
            : new Color(1f, 1f, 1f, 1f);
    }
}