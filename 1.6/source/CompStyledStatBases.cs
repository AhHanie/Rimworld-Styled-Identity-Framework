using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework
{
    public class CompProperties_StyledStatBases : CompProperties
    {
        public CompProperties_StyledStatBases()
        {
            compClass = typeof(CompStyledStatBases);
        }
    }

    public class CompStyledStatBases : ThingComp
    {
        private ThingStyleDef cachedStyleDef;

        private ThingDef cachedStuffDef;

        private QualityCategory? cachedQuality;

        private Dictionary<StatDef, float> cachedDeltas;

        private bool cacheBuilt;

        public override float GetStatOffset(StatDef stat)
        {
            Dictionary<StatDef, float> deltas = GetDeltas();
            if (deltas == null)
            {
                return 0f;
            }

            return deltas.TryGetValue(stat, out float delta) ? delta : 0f;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
        {
            ThingStyleDef styleDef = parent.StyleDef;
            StyleIdentityExtension extension = styleDef?.GetModExtension<StyleIdentityExtension>();
            StatModifier overrideModifier = extension?.statBases?.FirstOrDefault(m => m != null && m.stat == stat);
            if (overrideModifier == null)
            {
                return;
            }

            float originalBase = parent.def.statBases.GetStatValueFromList(stat, stat.defaultBaseValue);
            sb.AppendLine(whitespace + "StyledIdentityFramework.StatsReport_StyleBase".Translate(styleDef.LabelCap, stat.Worker.ValueToString(originalBase, finalized: false), stat.Worker.ValueToString(overrideModifier.value, finalized: false)));
        }

        public void ClearCache()
        {
            cacheBuilt = false;
            cachedDeltas = null;
        }

        private Dictionary<StatDef, float> GetDeltas()
        {
            ThingStyleDef styleDef = parent.StyleDef;
            ThingDef stuffDef = parent.Stuff;
            QualityCategory? quality = parent.TryGetQuality(out QualityCategory qc) ? qc : (QualityCategory?)null;

            if (cacheBuilt && cachedStyleDef == styleDef && cachedStuffDef == stuffDef && cachedQuality == quality)
            {
                return cachedDeltas;
            }

            cachedStyleDef = styleDef;
            cachedStuffDef = stuffDef;
            cachedQuality = quality;
            cachedDeltas = BuildDeltas(styleDef, stuffDef, quality);
            cacheBuilt = true;
            return cachedDeltas;
        }

        private Dictionary<StatDef, float> BuildDeltas(ThingStyleDef styleDef, ThingDef stuffDef, QualityCategory? quality)
        {
            StyleIdentityExtension extension = styleDef?.GetModExtension<StyleIdentityExtension>();
            if (extension?.statBases == null || extension.statBases.Count == 0)
            {
                return null;
            }

            Dictionary<StatDef, float> deltas = null;
            for (int i = 0; i < extension.statBases.Count; i++)
            {
                StatModifier modifier = extension.statBases[i];
                if (modifier?.stat == null)
                {
                    continue;
                }

                StatDef stat = modifier.stat;
                float originalBase = parent.def.statBases.GetStatValueFromList(stat, stat.defaultBaseValue);
                float styledBase = modifier.value;

                float originalAfterStuff = ApplyStuffAdjustments(originalBase, stat, stuffDef, quality);
                float styledAfterStuff = ApplyStuffAdjustments(styledBase, stat, stuffDef, quality);

                float delta = styledAfterStuff - originalAfterStuff;
                if (delta != 0f)
                {
                    if (deltas == null)
                    {
                        deltas = new Dictionary<StatDef, float>();
                    }

                    deltas[stat] = delta;
                }
            }

            return deltas;
        }

        // Mirrors the req.StuffDef block of RimWorld.StatWorker.GetValueUnfinalized, applied
        // independently to the original and styled base so their difference already accounts
        // for stuff/quality factors and offsets by the time it reaches later component offsets.
        private static float ApplyStuffAdjustments(float value, StatDef stat, ThingDef stuffDef, QualityCategory? quality)
        {
            if (stuffDef?.stuffProps == null)
            {
                return value;
            }

            if (value > 0f || stat.applyFactorsIfNegative)
            {
                value *= stuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
                if (quality.HasValue)
                {
                    value *= stuffDef.stuffProps.statFactorsQuality.GetStatFactorFromList(stat, quality.Value);
                }
            }

            value += stuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
            if (quality.HasValue)
            {
                value += stuffDef.stuffProps.statOffsetsQuality.GetStatOffsetFromList(stat, quality.Value);
            }

            return value;
        }
    }
}
