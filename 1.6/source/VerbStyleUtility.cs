using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework
{
    public static class VerbStyleUtility
    {
        public static void Refresh(ThingWithComps equipment)
        {
            if (equipment == null)
            {
                return;
            }

            CompEquippable equippable = equipment.GetComp<CompEquippable>();
            if (equippable == null)
            {
                return;
            }

            equippable.VerbTracker.VerbsNeedReinitOnLoad();
            _ = equippable.AllVerbs;

            if (equipment.ParentHolder is Pawn_EquipmentTracker equipmentTracker && equipmentTracker.pawn?.meleeVerbs != null)
            {
                equipmentTracker.pawn.meleeVerbs.Notify_PawnDespawned();
            }
        }

        public static List<VerbProperties> GetVerbPropertiesForVerbInitialization(CompEquippable equippable, List<VerbProperties> original)
        {
            if (equippable?.parent == null || original.NullOrEmpty())
            {
                return original;
            }

            if (!TryGetStyledEquipment(equippable.parent, out _, out StyleIdentityExtension extension))
            {
                return original;
            }

            List<VerbProperties> result = original;

            if (extension.projectileSource != null)
            {
                result = ReplacePrimaryVerbProperties(result, typeof(Verb_LaunchProjectile), GetPrimaryProjectileVerbProperties(extension.projectileSource));
            }
            else if (extension.projectile != null || extension.soundCast != null || extension.soundCastTail != null || extension.soundAiming != null)
            {
                result = ApplyLegacyProjectileOverlay(result, extension);
            }

            if (extension.beamSource != null)
            {
                result = ReplacePrimaryVerbProperties(result, typeof(Verb_ShootBeam), GetPrimaryBeamVerbProperties(extension.beamSource));
            }

            return result;
        }

        private static List<VerbProperties> ReplacePrimaryVerbProperties(List<VerbProperties> current, Type family, VerbProperties templateProps)
        {
            if (templateProps == null)
            {
                return current;
            }

            int index = current.FindIndex(v => v.isPrimary && family.IsAssignableFrom(v.verbClass));
            if (index < 0)
            {
                Logger.Warning($"StyleIdentityExtension could not find a mapped primary {family.Name} verb to replace; keeping the base verb list unchanged.");
                return current;
            }

            VerbProperties original = current[index];
            VerbProperties clone = templateProps.MemberwiseClone();
            clone.label = original.label;
            clone.untranslatedLabel = original.untranslatedLabel;

            List<VerbProperties> copy = new List<VerbProperties>(current)
            {
                [index] = clone
            };
            return copy;
        }

        private static List<VerbProperties> ApplyLegacyProjectileOverlay(List<VerbProperties> current, StyleIdentityExtension extension)
        {
            int index = current.FindIndex(v => v.isPrimary && typeof(Verb_LaunchProjectile).IsAssignableFrom(v.verbClass));
            if (index < 0)
            {
                return current;
            }

            VerbProperties clone = current[index].MemberwiseClone();

            if (extension.projectile != null)
            {
                clone.defaultProjectile = extension.projectile;
            }

            if (extension.soundCast != null)
            {
                clone.soundCast = extension.soundCast;
            }

            if (extension.soundCastTail != null)
            {
                clone.soundCastTail = extension.soundCastTail;
            }

            if (extension.soundAiming != null)
            {
                clone.soundAiming = extension.soundAiming;
            }

            List<VerbProperties> copy = new List<VerbProperties>(current)
            {
                [index] = clone
            };
            return copy;
        }

        private static bool? anyMeleeToolsStyleLoaded;

        private static bool? anyMeleeHitSoundStyleLoaded;

        public static bool AnyMeleeToolsStyleLoaded()
        {
            if (!anyMeleeToolsStyleLoaded.HasValue)
            {
                anyMeleeToolsStyleLoaded = DefDatabase<ThingStyleDef>.AllDefsListForReading.Any(styleDef => styleDef.GetModExtension<StyleIdentityExtension>()?.tools != null);
            }

            return anyMeleeToolsStyleLoaded.Value;
        }

        public static bool AnyMeleeHitSoundStyleLoaded()
        {
            if (!anyMeleeHitSoundStyleLoaded.HasValue)
            {
                anyMeleeHitSoundStyleLoaded = DefDatabase<ThingStyleDef>.AllDefsListForReading.Any(styleDef => styleDef.GetModExtension<StyleIdentityExtension>()?.meleeHitSound != null);
            }

            return anyMeleeHitSoundStyleLoaded.Value;
        }

        public static List<Tool> GetToolsForVerbInitialization(IVerbOwner owner)
        {
            if (owner is CompEquippable equippable && TryGetStyledEquipment(equippable.parent, out _, out StyleIdentityExtension extension) && extension.tools != null)
            {
                return extension.tools;
            }

            return owner.Tools;
        }

        [ThreadStatic]
        private static Thing currentMeleeStatThing;

        public static void SetMeleeStatThing(Thing thing)
        {
            currentMeleeStatThing = thing;
        }

        public static void ClearMeleeStatThing()
        {
            currentMeleeStatThing = null;
        }

        public static List<Tool> ResolveMeleeStatTools(List<Tool> originalTools)
        {
            if (currentMeleeStatThing is ThingWithComps equipment && TryGetStyledEquipment(equipment, out _, out StyleIdentityExtension extension) && extension.tools != null)
            {
                return extension.tools;
            }

            return originalTools;
        }

        public static bool TryGetStyledEquipment(Verb verb, out ThingWithComps equipment, out ThingStyleDef styleDef, out StyleIdentityExtension extension)
        {
            equipment = verb?.EquipmentSource;
            return TryGetStyledEquipment(equipment, out styleDef, out extension);
        }

        public static string GetGizmoLabelCap(ThingWithComps equipment)
        {
            return GenLabel.ThingLabel(equipment, 1, includeHp: false, includeQuality: false).CapitalizeFirst();
        }

        public static bool TryGetStyledEquipment(ThingWithComps equipment, out ThingStyleDef styleDef, out StyleIdentityExtension extension)
        {
            styleDef = equipment?.StyleDef;
            extension = styleDef?.GetModExtension<StyleIdentityExtension>();
            return equipment != null && styleDef != null && extension != null;
        }

        private static VerbProperties GetPrimaryBeamVerbProperties(ThingDef source)
        {
            if (source?.Verbs == null)
            {
                return null;
            }

            List<VerbProperties> candidates = source.Verbs.Where(v => v.isPrimary && typeof(Verb_ShootBeam).IsAssignableFrom(v.verbClass)).ToList();
            return candidates.Count == 1 ? candidates[0] : null;
        }

        private static VerbProperties GetPrimaryProjectileVerbProperties(ThingDef source)
        {
            if (source?.Verbs == null)
            {
                return null;
            }

            List<VerbProperties> candidates = source.Verbs.Where(v => v.isPrimary && typeof(Verb_LaunchProjectile).IsAssignableFrom(v.verbClass)).ToList();
            return candidates.Count == 1 ? candidates[0] : null;
        }
    }
}
