using HarmonyLib;
using UnityEngine;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(Verb), nameof(Verb.UIIcon), MethodType.Getter)]
    public static class Verb_UIIcon_StyleIdentity_Patch
    {
        public static void Postfix(Verb __instance, ref Texture2D __result)
        {
            if (!__instance.verbProps.commandIcon.NullOrEmpty())
            {
                return;
            }

            if (!VerbStyleUtility.TryGetStyledEquipment(__instance, out _, out ThingStyleDef styleDef, out _))
            {
                return;
            }

            if (styleDef.UIIcon != null)
            {
                __result = styleDef.UIIcon;
            }
        }
    }
}
