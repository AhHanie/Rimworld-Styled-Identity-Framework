using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework
{
    public static class StyledStatBaseUtility
    {
        private static readonly HashSet<StatDef> overriddenStats = new HashSet<StatDef>();

        public static void Initialize()
        {
            overriddenStats.Clear();

            foreach (ThingStyleDef styleDef in DefDatabase<ThingStyleDef>.AllDefsListForReading)
            {
                StyleIdentityExtension extension = styleDef.GetModExtension<StyleIdentityExtension>();
                if (extension?.statBases == null || extension.statBases.Count == 0)
                {
                    continue;
                }

                foreach (StatModifier modifier in extension.statBases)
                {
                    if (modifier?.stat != null)
                    {
                        overriddenStats.Add(modifier.stat);
                    }
                }

                foreach (ThingDef thingDef in StyleIdentityExtension.GetMappedThingDefs(styleDef))
                {
                    if (!thingDef.CanBeStyled())
                    {
                        // Malformed mapping; StyleIdentityExtension.ConfigErrors reports this. Do not inject the component.
                        continue;
                    }

                    if (thingDef.GetCompProperties<CompProperties_StyledStatBases>() == null)
                    {
                        thingDef.comps.Add(new CompProperties_StyledStatBases());
                    }
                }
            }
        }

        public static void NotifyStyleChanged(Thing thing)
        {
            if (thing is ThingWithComps equipment)
            {
                equipment.GetComp<CompStyledStatBases>()?.ClearCache();
            }

            if (overriddenStats.Count == 0)
            {
                return;
            }

            foreach (StatDef stat in overriddenStats)
            {
                stat.Worker.ClearCacheForThing(thing);
            }
        }
    }
}
