using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.VerbProperties), MethodType.Getter)]
    public static class CompEquippable_VerbProperties_StyleIdentity_Patch
    {
        public static void Postfix(CompEquippable __instance, ref List<VerbProperties> __result)
        {
            __result = VerbStyleUtility.GetVerbPropertiesForVerbInitialization(__instance, __result);
        }
    }
}
