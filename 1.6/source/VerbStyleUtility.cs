using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework
{
    public static class VerbStyleUtility
    {
        private static readonly ConditionalWeakTable<Verb, VerbProperties> OriginalVerbProps = new ConditionalWeakTable<Verb, VerbProperties>();

        public static void Refresh(ThingWithComps equipment)
        {
            if (equipment == null)
            {
                return;
            }

            CompEquippable equippable = equipment.GetComp<CompEquippable>();
            List<Verb> verbs = equippable?.AllVerbs;
            if (verbs == null)
            {
                return;
            }

            foreach (Verb verb in verbs)
            {
                Refresh(verb);
            }
        }

        public static void Refresh(Verb verb)
        {
            bool isBeam = verb is Verb_ShootBeam;
            bool isProjectile = verb is Verb_LaunchProjectile;
            if ((!isBeam && !isProjectile) || verb.verbProps == null)
            {
                return;
            }

            ThingWithComps equipment = verb.EquipmentSource;
            if (equipment == null)
            {
                return;
            }

            if (!OriginalVerbProps.TryGetValue(verb, out VerbProperties original))
            {
                original = verb.verbProps;
                OriginalVerbProps.Add(verb, original);
            }

            StyleIdentityExtension extension = ResolveExtension(equipment);

            if (isBeam)
            {
                RefreshBeam(verb, original, extension);
            }
            else
            {
                RefreshProjectile(verb, original, extension);
            }
        }

        private static void RefreshBeam(Verb verb, VerbProperties original, StyleIdentityExtension extension)
        {
            VerbProperties templateProps = GetPrimaryBeamVerbProperties(extension?.beamSource);
            if (templateProps == null || templateProps.verbClass != verb.GetType())
            {
                verb.verbProps = original;
                return;
            }

            verb.verbProps = templateProps.MemberwiseClone();
        }

        private static void RefreshProjectile(Verb verb, VerbProperties original, StyleIdentityExtension extension)
        {
            if (extension == null || (extension.projectile == null && extension.soundCast == null && extension.soundCastTail == null && extension.soundAiming == null))
            {
                verb.verbProps = original;
                return;
            }

            VerbProperties clone = original.MemberwiseClone();

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

            verb.verbProps = clone;
        }

        private static StyleIdentityExtension ResolveExtension(ThingWithComps equipment)
        {
            TryGetStyledEquipment(equipment, out _, out StyleIdentityExtension extension);
            return extension;
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
    }
}
