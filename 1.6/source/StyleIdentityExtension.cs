using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
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

        public List<Tool> tools;

        public SoundDef meleeHitSound;

        public List<StatModifier> statBases;

        private ThingStyleDef parent;

        public override void ResolveReferences(Def parentDef)
        {
            parent = parentDef as ThingStyleDef;

            if (tools != null)
            {
                for (int i = 0; i < tools.Count; i++)
                {
                    if (tools[i] != null)
                    {
                        tools[i].id = i.ToString();
                    }
                }
            }
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
            bool hasMeleeOverride = tools != null || meleeHitSound != null;
            bool hasStatBases = statBases != null;

            if (string.IsNullOrWhiteSpace(description) && !hasAnyProjectileOverride && beamSource == null && !hasMeleeOverride && !hasStatBases)
            {
                yield return "StyleIdentityExtension has neither a description, a projectile, a sound override, a projectileSource, a beamSource, tools, a meleeHitSound, nor statBases set.";
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

            if (tools != null)
            {
                foreach (string error in ValidateMeleeTools(mappedThingDefs))
                {
                    yield return error;
                }
            }

            if (meleeHitSound != null)
            {
                foreach (string error in ValidateMeleeHitSound(mappedThingDefs))
                {
                    yield return error;
                }
            }

            if (hasStatBases)
            {
                foreach (string error in ValidateStatBases(mappedThingDefs))
                {
                    yield return error;
                }
            }

            if ((hasAnyProjectileOverride || beamSource != null || hasMeleeOverride || hasStatBases) && mappedThingDefs.Count == 0)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets a projectile, sound, projectileSource, beamSource, tools, meleeHitSound, or statBases override, but the style is not mapped to any ThingDef via a StyleCategoryDef. The override can never be reached.";
            }
        }

        private IEnumerable<string> ValidateStatBases(List<ThingDef> mappedThingDefs)
        {
            if (statBases.NullOrEmpty())
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets an empty statBases list. Omit statBases to leave existing stat bases unchanged, or supply at least one replacement.";
                yield break;
            }

            HashSet<StatDef> seenStats = new HashSet<StatDef>();
            List<StatDef> validStats = new List<StatDef>();
            for (int i = 0; i < statBases.Count; i++)
            {
                StatModifier modifier = statBases[i];
                if (modifier == null)
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' has a null statBases entry at statBases[{i}].";
                    continue;
                }

                if (modifier.stat == null)
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' has a statBases entry at statBases[{i}] with no stat set.";
                    continue;
                }

                if (!seenStats.Add(modifier.stat))
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' has more than one statBases entry for '{modifier.stat.defName}'.";
                    continue;
                }

                if (modifier.stat == StatDefOf.MarketValue)
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' sets statBases for '{modifier.stat.defName}', but MarketValue is computed by a custom StatWorker (StatWorker_MarketValue) and cannot be replaced through statBases.";
                    continue;
                }

                if (modifier.stat.Worker.GetType() != typeof(StatWorker))
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' sets statBases for '{modifier.stat.defName}', whose StatWorker is {modifier.stat.Worker.GetType().Name} rather than the base RimWorld.StatWorker. Custom-worker stats are not supported by statBases.";
                    continue;
                }

                validStats.Add(modifier.stat);
            }

            if (mappedThingDefs.Count == 0)
            {
                yield break;
            }

            foreach (ThingDef mappedThingDef in mappedThingDefs)
            {
                if (!mappedThingDef.CanBeStyled())
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' sets statBases, but is mapped to '{mappedThingDef.defName}', which has no CompStyleable. statBases requires a styleable target.";
                    continue;
                }

                foreach (StatDef stat in validStats)
                {
                    if (!mappedThingDef.statBases.StatListContains(stat))
                    {
                        yield return $"StyleIdentityExtension on '{parent.defName}' sets statBases for '{stat.defName}', but mapped ThingDef '{mappedThingDef.defName}' does not define '{stat.defName}' in its own statBases. statBases can only replace an existing base value, not add a new one.";
                    }
                }
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

        private IEnumerable<string> ValidateMeleeTools(List<ThingDef> mappedThingDefs)
        {
            if (tools.NullOrEmpty())
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets an empty tools list. Omit tools to keep the base weapon's tools unchanged, or supply at least one replacement tool.";
                yield break;
            }

            for (int i = 0; i < tools.Count; i++)
            {
                Tool tool = tools[i];
                if (tool == null)
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' has a null tool entry at tools[{i}].";
                    continue;
                }

                foreach (string error in tool.ConfigErrors())
                {
                    yield return error;
                }

                if (tool.capacities.NullOrEmpty() || !tool.Maneuvers.Any())
                {
                    yield return $"StyleIdentityExtension on '{parent.defName}' has a tool ('{tool.label}') with no capacities matching a ManeuverDef; it would never produce a usable melee attack.";
                }
            }

            if (mappedThingDefs.Count == 0)
            {
                yield break;
            }

            bool anyMeleeMapping = mappedThingDefs.Any(td => td.IsMeleeWeapon);
            if (!anyMeleeMapping)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets tools, but the style is not mapped (via a StyleCategoryDef) to any melee ThingDef (ThingDef.IsMeleeWeapon). The override will have no effect.";
            }
        }

        private IEnumerable<string> ValidateMeleeHitSound(List<ThingDef> mappedThingDefs)
        {
            if (mappedThingDefs.Count == 0)
            {
                yield break;
            }

            bool anyMeleeMapping = mappedThingDefs.Any(td => td.IsMeleeWeapon);
            if (!anyMeleeMapping)
            {
                yield return $"StyleIdentityExtension on '{parent.defName}' sets meleeHitSound, but the style is not mapped (via a StyleCategoryDef) to any melee ThingDef (ThingDef.IsMeleeWeapon). The override will have no effect.";
            }
        }

        internal static IEnumerable<ThingDef> GetMappedThingDefs(ThingStyleDef styleDef)
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
