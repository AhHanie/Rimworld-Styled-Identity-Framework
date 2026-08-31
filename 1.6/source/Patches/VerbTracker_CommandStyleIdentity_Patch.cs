using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
    public static class VerbTracker_CommandStyleIdentity_Patch
    {
        public static void Postfix(Thing ownerThing, Command_VerbTarget __result)
        {
            if (__result == null || !(ownerThing is ThingWithComps equipment))
            {
                return;
            }

            if (!VerbStyleUtility.TryGetStyledEquipment(equipment, out ThingStyleDef styleDef, out StyleIdentityExtension extension))
            {
                return;
            }

            if (!styleDef.overrideLabel.NullOrEmpty())
            {
                __result.defaultLabel = ownerThing.LabelCap;
            }

            if (!extension.description.NullOrEmpty())
            {
                __result.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.DescriptionFlavor.CapitalizeFirst();
            }
        }
    }
}
