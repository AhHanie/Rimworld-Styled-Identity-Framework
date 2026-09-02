using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Styled_Identity_Framework
{
    public class StyleIdentityExtension : DefModExtension
    {
        [MustTranslate]
        public string description;

        public ThingDef projectile;

        public ThingDef projectileSource;

        public ThingDef beamSource;

        public SoundDef soundCast;

        public SoundDef soundCastTail;

        public SoundDef soundAiming;

        private ThingStyleDef parent;

        public override void ResolveReferences(Def parentDef)
        {
            parent = parentDef as ThingStyleDef;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (parent == null)
            {
                yield return "StyleIdentityExtension is used on a def that is not a ThingStyleDef.";
                yield break;
            }

            bool hasLegacyProjectileOverride = projectile != null || soundCast != null || soundCastTail != null || soundAiming != null;
            bool hasProjectileSource = projectileSource != null;
            bool hasAnyProjectileOverride = hasLegacyProjectileOverride || hasProjectileSource;

            if (string.IsNullOrWhiteSpace(description) && !hasAnyProjectileOverride && beamSource == null)
            {
                yield return "StyleIdentityExtension has neither a description, a projectile, a sound override, a projectileSource, nor a beamSource set.";
            }

            if (hasLegacyProjectileOverride && hasProjectileSource)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets both projectileSource and a legacy projectile/sound field (projectile, soundCast, soundCastTail, and/or soundAiming). Use either projectileSource or the legacy fields, not both.";
            }

            List<ThingDef> mappedThingDefs = GetMappedThingDefs(parent).ToList();

            if (hasLegacyProjectileOverride)
            {
                foreach (string error in ValidateProjectile(mappedThingDefs))
                {
                    yield return error;
                }
            }

            if (hasProjectileSource)
            {
                foreach (string error in ValidateProjectileSource(mappedThingDefs))
                {
                    yield return error;
                }
            }

            if (beamSource != null)
            {
                foreach (string error in ValidateBeamSource(mappedThingDefs))
                {
                    yield return error;
                }
            }

            if ((hasAnyProjectileOverride || beamSource != null) && mappedThingDefs.Count == 0)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets a projectile, sound, projectileSource, or beamSource override, but the style is not mapped to any ThingDef via a StyleCategoryDef. The override can never be reached.";
            }
        }

        private IEnumerable<string> ValidateProjectile(List<ThingDef> mappedThingDefs)
        {
            if (projectile != null)
            {
                if (projectile.projectile == null)
                {
                    yield return $"StyleIdentityExtension projectile '{projectile.defName}' has no ProjectileProperties (projectile.projectile is null).";
                }
                else if (!typeof(Projectile).IsAssignableFrom(projectile.thingClass))
                {
                    yield return $"StyleIdentityExtension projectile '{projectile.defName}' has a thingClass ({projectile.thingClass}) that does not derive from Verse.Projectile.";
                }
            }

            if (mappedThingDefs.Count == 0)
            {
                yield break;
            }

            bool usedByLaunchVerb = mappedThingDefs.Any(td => td.Verbs != null && td.Verbs.Any(v => v.isPrimary && v.LaunchesProjectile));
            if (!usedByLaunchVerb)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets a projectile and/or sound override, but the style is not mapped (via a StyleCategoryDef) to any ThingDef whose primary verb launches a projectile. The override will have no effect.";
            }
        }

        private IEnumerable<string> ValidateProjectileSource(List<ThingDef> mappedThingDefs)
        {
            if (projectileSource.Verbs == null || projectileSource.Verbs.Count == 0)
            {
                yield return $"StyleIdentityExtension projectileSource '{projectileSource.defName}' has no verbs.";
                yield break;
            }

            List<VerbProperties> primaryProjectileVerbs = projectileSource.Verbs.Where(v => v.isPrimary && typeof(Verb_LaunchProjectile).IsAssignableFrom(v.verbClass)).ToList();
            if (primaryProjectileVerbs.Count == 0)
            {
                yield return $"StyleIdentityExtension projectileSource '{projectileSource.defName}' has no primary verb deriving from Verse.Verb_LaunchProjectile.";
                yield break;
            }

            if (primaryProjectileVerbs.Count > 1)
            {
                yield return $"StyleIdentityExtension projectileSource '{projectileSource.defName}' has more than one eligible primary Verb_LaunchProjectile-derived verb; the override is ambiguous.";
                yield break;
            }

            ThingDef templateProjectile = primaryProjectileVerbs[0].defaultProjectile;
            if (templateProjectile == null)
            {
                yield return $"StyleIdentityExtension projectileSource '{projectileSource.defName}' has a primary verb with no defaultProjectile set.";
            }
            else if (templateProjectile.projectile == null)
            {
                yield return $"StyleIdentityExtension projectileSource '{projectileSource.defName}' has a defaultProjectile '{templateProjectile.defName}' with no ProjectileProperties (projectile.projectile is null).";
            }
            else if (!typeof(Projectile).IsAssignableFrom(templateProjectile.thingClass))
            {
                yield return $"StyleIdentityExtension projectileSource '{projectileSource.defName}' has a defaultProjectile '{templateProjectile.defName}' whose thingClass ({templateProjectile.thingClass}) does not derive from Verse.Projectile.";
            }

            if (mappedThingDefs.Count == 0)
            {
                yield break;
            }

            List<ThingDef> projectileMappedThingDefs = mappedThingDefs.Where(td => td.Verbs != null && td.Verbs.Any(v => v.isPrimary && typeof(Verb_LaunchProjectile).IsAssignableFrom(v.verbClass))).ToList();
            if (projectileMappedThingDefs.Count == 0)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets a projectileSource override, but the style is not mapped (via a StyleCategoryDef) to any ThingDef whose primary verb is Verb_LaunchProjectile or a subclass. The override will have no effect.";
                yield break;
            }

            Type templateVerbClass = primaryProjectileVerbs[0].verbClass;
            foreach (ThingDef mappedThingDef in projectileMappedThingDefs)
            {
                VerbProperties mappedPrimaryProjectileVerb = mappedThingDef.Verbs.First(v => v.isPrimary && typeof(Verb_LaunchProjectile).IsAssignableFrom(v.verbClass));
                if (mappedPrimaryProjectileVerb.verbClass != templateVerbClass)
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' maps to '{mappedThingDef.defName}' (primary verb class {mappedPrimaryProjectileVerb.verbClass}), but projectileSource '{projectileSource.defName}' has primary verb class {templateVerbClass}. The verb classes must match exactly for the style to take effect.";
                }
            }
        }

        private IEnumerable<string> ValidateBeamSource(List<ThingDef> mappedThingDefs)
        {
            if (beamSource.Verbs == null || beamSource.Verbs.Count == 0)
            {
                yield return $"StyleIdentityExtension beamSource '{beamSource.defName}' has no verbs.";
                yield break;
            }

            List<VerbProperties> primaryBeamVerbs = beamSource.Verbs.Where(v => v.isPrimary && typeof(Verb_ShootBeam).IsAssignableFrom(v.verbClass)).ToList();
            if (primaryBeamVerbs.Count == 0)
            {
                yield return $"StyleIdentityExtension beamSource '{beamSource.defName}' has no primary verb deriving from Verse.Verb_ShootBeam.";
                yield break;
            }

            if (primaryBeamVerbs.Count > 1)
            {
                yield return $"StyleIdentityExtension beamSource '{beamSource.defName}' has more than one eligible primary Verb_ShootBeam-derived verb; the override is ambiguous.";
                yield break;
            }

            if (mappedThingDefs.Count == 0)
            {
                yield break;
            }

            List<ThingDef> beamMappedThingDefs = mappedThingDefs.Where(td => td.Verbs != null && td.Verbs.Any(v => v.isPrimary && typeof(Verb_ShootBeam).IsAssignableFrom(v.verbClass))).ToList();
            if (beamMappedThingDefs.Count == 0)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets a beamSource override, but the style is not mapped (via a StyleCategoryDef) to any ThingDef whose primary verb is Verb_ShootBeam or a subclass. The override will have no effect.";
                yield break;
            }

            Type templateVerbClass = primaryBeamVerbs[0].verbClass;
            foreach (ThingDef mappedThingDef in beamMappedThingDefs)
            {
                VerbProperties mappedPrimaryBeamVerb = mappedThingDef.Verbs.First(v => v.isPrimary && typeof(Verb_ShootBeam).IsAssignableFrom(v.verbClass));
                if (mappedPrimaryBeamVerb.verbClass != templateVerbClass)
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' maps to '{mappedThingDef.defName}' (primary verb class {mappedPrimaryBeamVerb.verbClass}), but beamSource '{beamSource.defName}' has primary verb class {templateVerbClass}. The verb classes must match exactly for the style to take effect.";
                }
            }
        }

        private static IEnumerable<ThingDef> GetMappedThingDefs(ThingStyleDef styleDef)
        {
            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            foreach (StyleCategoryDef category in DefDatabase<StyleCategoryDef>.AllDefs)
            {
                if (category.thingDefStyles == null)
                {
                    continue;
                }

                foreach (ThingDefStyle mapping in category.thingDefStyles)
                {
                    if (mapping.StyleDef == styleDef && mapping.ThingDef != null && seen.Add(mapping.ThingDef))
                    {
                        yield return mapping.ThingDef;
                    }
                }
            }
        }
    }
}
