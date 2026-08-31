using HarmonyLib;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(ThingStyleHelper), nameof(ThingStyleHelper.SetStyleDef))]
    public static class ThingStyleHelper_SetStyleDef_VerbStyle_Patch
    {
        public static void Postfix(Thing thing)
        {
            VerbStyleUtility.Refresh(thing as ThingWithComps);
        }
    }

    [HarmonyPatch(typeof(CompStyleable), nameof(CompStyleable.SourcePrecept), MethodType.Setter)]
    public static class CompStyleable_SourcePrecept_VerbStyle_Patch
    {
        public static void Postfix(CompStyleable __instance)
        {
            VerbStyleUtility.Refresh(__instance.parent);
        }
    }
}
