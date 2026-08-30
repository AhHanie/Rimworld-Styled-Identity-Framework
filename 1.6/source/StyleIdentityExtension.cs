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
            }

            if (string.IsNullOrWhiteSpace(description) && projectile == null)
            {
                yield return "StyleIdentityExtension has neither a description nor a projectile set.";
            }

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

                if (parent != null)
                {
                    StyleCategoryDef category = parent.Category;
                    bool usedByLaunchVerb = category != null && category.thingDefStyles.Any(mapping =>
                        mapping.StyleDef == parent
                        && mapping.ThingDef?.Verbs != null
                        && mapping.ThingDef.Verbs.Any(v => v.isPrimary && v.LaunchesProjectile));

                    if (!usedByLaunchVerb)
                    {
                        yield return $"StyleIdentityExtension on '{parent.defName}' sets a projectile override, but the style is not mapped (via a StyleCategoryDef) to a ThingDef whose primary verb launches a projectile. The override will have no effect.";
                    }
                }
            }
        }
    }
}
