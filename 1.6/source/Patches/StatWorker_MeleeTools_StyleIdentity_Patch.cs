using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    internal static class StatWorker_MeleeTools_TranspilerHelper
    {
        internal static IEnumerable<CodeInstruction> ReplaceToolsFieldReads(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo toolsField = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.tools));
            MethodInfo resolver = AccessTools.Method(typeof(VerbStyleUtility), nameof(VerbStyleUtility.ResolveMeleeStatTools));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.LoadsField(toolsField))
                {
                    yield return new CodeInstruction(OpCodes.Call, resolver);
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker_MeleeAverageDPS), nameof(StatWorker_MeleeAverageDPS.GetValueUnfinalized))]
    public static class StatWorker_MeleeAverageDPS_GetValueUnfinalized_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeToolsStyleLoaded();

        public static void Prefix(StatRequest req) => VerbStyleUtility.SetMeleeStatThing(req.Thing);
        public static void Postfix() => VerbStyleUtility.ClearMeleeStatThing();
    }

    [HarmonyPatch(typeof(StatWorker_MeleeAverageDPS), nameof(StatWorker_MeleeAverageDPS.GetExplanationUnfinalized))]
    public static class StatWorker_MeleeAverageDPS_GetExplanationUnfinalized_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeToolsStyleLoaded();

        public static void Prefix(StatRequest req) => VerbStyleUtility.SetMeleeStatThing(req.Thing);
        public static void Postfix() => VerbStyleUtility.ClearMeleeStatThing();
    }

    [HarmonyPatch(typeof(StatWorker_MeleeAverageDPS), "GetVerbsAndTools")]
    public static class StatWorker_MeleeAverageDPS_GetVerbsAndTools_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeToolsStyleLoaded();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return StatWorker_MeleeTools_TranspilerHelper.ReplaceToolsFieldReads(instructions);
        }
    }

    [HarmonyPatch(typeof(StatWorker_MeleeAverageArmorPenetration), nameof(StatWorker_MeleeAverageArmorPenetration.GetValueUnfinalized))]
    public static class StatWorker_MeleeAverageArmorPenetration_GetValueUnfinalized_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeToolsStyleLoaded();

        public static void Prefix(StatRequest req) => VerbStyleUtility.SetMeleeStatThing(req.Thing);
        public static void Postfix() => VerbStyleUtility.ClearMeleeStatThing();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return StatWorker_MeleeTools_TranspilerHelper.ReplaceToolsFieldReads(instructions);
        }
    }

    [HarmonyPatch(typeof(StatWorker_MeleeAverageArmorPenetration), nameof(StatWorker_MeleeAverageArmorPenetration.GetExplanationUnfinalized))]
    public static class StatWorker_MeleeAverageArmorPenetration_GetExplanationUnfinalized_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeToolsStyleLoaded();

        public static void Prefix(StatRequest req) => VerbStyleUtility.SetMeleeStatThing(req.Thing);
        public static void Postfix() => VerbStyleUtility.ClearMeleeStatThing();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return StatWorker_MeleeTools_TranspilerHelper.ReplaceToolsFieldReads(instructions);
        }
    }
}
