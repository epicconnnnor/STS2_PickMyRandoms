using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PickMyRandoms.PickMyRandomsCode;

// Right-click a character portrait to exclude it from random. Excluded portraits dim
// and carry a persistent badge, while remaining available for direct selection.
[HarmonyPatch(typeof(NCharacterSelectButton), "Init")]
public static class ButtonTogglePatch
{
    public static void Postfix(NCharacterSelectButton __instance)
    {
        if (__instance.IsRandom)
            return;

        AddExcludedBadge(__instance);
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

        Label? badge = button.GetNodeOrNull<Label>("PickMyRandomsExcludedBadge");
        if (badge != null)
            badge.Visible = excluded;
    }

    private static void AddExcludedBadge(NCharacterSelectButton button)
    {
        if (button.GetNodeOrNull<Label>("PickMyRandomsExcludedBadge") != null)
            return;

        Label badge = new()
        {
            Name = "PickMyRandomsExcludedBadge",
            Text = "🚫",
            TooltipText = "Excluded from Random\nRight-click to include",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorLeft = 1f,
            AnchorRight = 1f,
            OffsetLeft = -46f,
            OffsetTop = 6f,
            OffsetRight = -6f,
            OffsetBottom = 46f,
            ZIndex = 100
        };

        badge.AddThemeFontSizeOverride("font_size", 30);
        badge.AddThemeColorOverride("font_outline_color", new Color(0.1f, 0.05f, 0.05f, 1f));
        badge.AddThemeConstantOverride("outline_size", 4);
        button.AddChild(badge);
    }
}
