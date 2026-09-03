using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(VerbTracker), "InitVerbs")]
    public static class VerbTracker_MeleeTools_StyleIdentity_Patch
    {
        public static bool Prepare() => VerbStyleUtility.AnyMeleeToolsStyleLoaded();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo toolsGetter = AccessTools.PropertyGetter(typeof(IVerbOwner), nameof(IVerbOwner.Tools));
            MethodInfo replacement = AccessTools.Method(typeof(VerbStyleUtility), nameof(VerbStyleUtility.GetToolsForVerbInitialization));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(toolsGetter))
                {
                    yield return new CodeInstruction(OpCodes.Call, replacement);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
