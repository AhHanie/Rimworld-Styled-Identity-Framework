using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            if (extension?.projectile == null)
            {
                verb.verbProps = original;
                return;
            }

            VerbProperties clone = original.MemberwiseClone();
            clone.defaultProjectile = extension.projectile;
            verb.verbProps = clone;
        }

        private static StyleIdentityExtension ResolveExtension(ThingWithComps equipment)
        {
            ThingStyleDef styleDef = equipment.StyleDef;
            return styleDef?.GetModExtension<StyleIdentityExtension>();
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
