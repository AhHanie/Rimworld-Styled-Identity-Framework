using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;

namespace Styled_Identity_Framework
{
    public static class DebugActions_StyledMeleeRaid
    {
        [DebugAction("Styled Identity Framework", "Spawn styled melee raid...", false, true, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnStyledMeleeRaid()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            foreach (float points in DebugActionsUtility.PointsOptions(extended: true))
            {
                float localPoints = points;
                DebugActionNode node = new DebugActionNode(localPoints + " points");
                node.action = delegate
                {
                    ExecuteStyledMeleeRaid(localPoints);
                };
                list.Add(node);
            }
            return list;
        }

        private static void ExecuteStyledMeleeRaid(float points)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            List<(ThingDef weaponDef, ThingStyleDef styleDef)> candidates = FindStyledMeleeWeaponOptions();
            if (candidates.Count == 0)
            {
                Messages.Message("No loaded ThingStyleDef sets StyleIdentityExtension.tools on a mapped melee weapon.", MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            HashSet<Pawn> pawnsBefore = new HashSet<Pawn>(map.mapPawns.AllPawns);

            IncidentParms parms = new IncidentParms
            {
                target = map,
                points = points,
                forced = true
            };

            if (!IncidentDefOf.RaidEnemy.Worker.TryExecute(parms))
            {
                Messages.Message("Raid failed to execute.", MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            int equippedCount = 0;
            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (pawnsBefore.Contains(pawn) || pawn.equipment == null)
                {
                    continue;
                }

                (ThingDef weaponDef, ThingStyleDef styleDef) = candidates[Rand.Range(0, candidates.Count)];
                ThingDef stuff = weaponDef.MadeFromStuff ? GenStuff.RandomStuffFor(weaponDef) : null;
                ThingWithComps weapon = (ThingWithComps)ThingMaker.MakeThing(weaponDef, stuff);
                weapon.StyleDef = styleDef;

                pawn.equipment.DestroyAllEquipment();
                pawn.equipment.AddEquipment(weapon);
                equippedCount++;
            }

            Messages.Message($"Equipped {equippedCount} raider(s) with styled melee weapons.", MessageTypeDefOf.TaskCompletion, historical: false);
        }

        private static List<(ThingDef, ThingStyleDef)> FindStyledMeleeWeaponOptions()
        {
            List<(ThingDef, ThingStyleDef)> result = new List<(ThingDef, ThingStyleDef)>();
            foreach (StyleCategoryDef category in DefDatabase<StyleCategoryDef>.AllDefs)
            {
                if (category.thingDefStyles == null)
                {
                    continue;
                }

                foreach (ThingDefStyle mapping in category.thingDefStyles)
                {
                    if (mapping.ThingDef == null || mapping.StyleDef == null || !mapping.ThingDef.IsMeleeWeapon)
                    {
                        continue;
                    }

                    StyleIdentityExtension extension = mapping.StyleDef.GetModExtension<StyleIdentityExtension>();
                    if (extension?.tools != null)
                    {
                        result.Add((mapping.ThingDef, mapping.StyleDef));
                    }
                }
            }

            return result;
        }
    }
}
