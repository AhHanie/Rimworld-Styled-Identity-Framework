using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.DescriptionFlavor), MethodType.Getter)]
    public static class Thing_DescriptionFlavor_Patch
    {
        public static void Postfix(Thing __instance, ref string __result)
        {
            ThingStyleDef styleDef = __instance.StyleDef;
            if (styleDef == null)
            {
                return;
            }

            StyleIdentityExtension extension = styleDef.GetModExtension<StyleIdentityExtension>();
            if (extension == null || extension.description.NullOrEmpty())
            {
                return;
            }

            __result = extension.description;
        }
    }
}
