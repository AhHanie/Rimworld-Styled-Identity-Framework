using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    public static class MVCF_CommandVerbTargetExtended_StyleIdentity_Patch
    {
        private const string CommandTypeName = "MVCF.Commands.Command_VerbTargetExtended";

        public static void TryPatch(Harmony harmony)
        {
            try
            {
                Type commandType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(CommandTypeName, throwOnError: false))
                    .FirstOrDefault(type => type != null);

                if (commandType == null)
                {
                    return;
                }

                ConstructorInfo constructor = commandType.GetConstructors().FirstOrDefault();
                if (constructor == null)
                {
                    Logger.Warning($"Detected {CommandTypeName} but found no public constructor to patch; MVCF gizmo styling will be skipped.");
                    return;
                }

                harmony.Patch(constructor, postfix: new HarmonyMethod(typeof(MVCF_CommandVerbTargetExtended_StyleIdentity_Patch), nameof(Postfix)));
                Logger.Message("Patched MVCF Command_VerbTargetExtended constructor for styled gizmo compatibility.");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"Failed to patch {CommandTypeName} for MVCF compatibility");
            }
        }

        public static void Postfix(object __instance)
        {
            if (!(__instance is Command_VerbTarget command) || command.verb?.verbProps == null)
            {
                return;
            }

            if (!VerbStyleUtility.TryGetStyledEquipment(command.verb, out ThingWithComps equipment, out ThingStyleDef styleDef, out StyleIdentityExtension extension))
            {
                return;
            }

            if (!styleDef.overrideLabel.NullOrEmpty())
            {
                command.defaultLabel = VerbStyleUtility.GetGizmoLabelCap(equipment);
            }

            if (!extension.description.NullOrEmpty())
            {
                command.defaultDesc = VerbStyleUtility.GetGizmoLabelCap(equipment) + ": " + equipment.DescriptionFlavor.CapitalizeFirst();
            }

            if (command.verb.verbProps.commandIcon.NullOrEmpty() && styleDef.UIIcon != null)
            {
                command.icon = styleDef.UIIcon;
            }
        }
    }
}
