using RimWorld;
using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundHitPawn")]
    public static class Verb_MeleeAttack_SoundHitPawn_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeHitSoundStyleLoaded();

        public static void Postfix(Verb_MeleeAttack __instance, ref SoundDef __result)
        {
            if (!VerbStyleUtility.TryGetStyledEquipment(__instance, out _, out _, out StyleIdentityExtension extension) || extension.meleeHitSound == null)
            {
                return;
            }

            __result = extension.meleeHitSound;
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundHitBuilding")]
    public static class Verb_MeleeAttack_SoundHitBuilding_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeHitSoundStyleLoaded();

        public static void Postfix(Verb_MeleeAttack __instance, ref SoundDef __result)
        {
            if (!VerbStyleUtility.TryGetStyledEquipment(__instance, out _, out _, out StyleIdentityExtension extension) || extension.meleeHitSound == null)
            {
                return;
            }

            if (__instance.CurrentTarget.Thing is Building building && !building.def.building.soundMeleeHitOverride.NullOrUndefined())
            {
                return;
            }

            __result = extension.meleeHitSound;
        }
    }
}
