using HarmonyLib;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), nameof(Verb_LaunchProjectile.Projectile), MethodType.Getter)]
    public static class Verb_LaunchProjectile_Projectile_Patch
    {
        public static void Postfix(Verb_LaunchProjectile __instance, ref ThingDef __result)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;
            if (equipmentSource == null)
            {
                return;
            }

            CompChangeableProjectile changeableProjectile = equipmentSource.GetComp<CompChangeableProjectile>();
            if (changeableProjectile != null && changeableProjectile.Loaded)
            {
                return;
            }

            ThingStyleDef styleDef = equipmentSource.StyleDef;
            if (styleDef == null)
            {
                return;
            }

            StyleIdentityExtension extension = styleDef.GetModExtension<StyleIdentityExtension>();
            if (extension?.projectile == null)
            {
                return;
            }

            __result = extension.projectile;
        }
    }
}
