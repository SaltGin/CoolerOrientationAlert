using HarmonyLib;
using RimWorld;
using Verse;

namespace CoolerOrientationAlert
{
    public class CoolerOrientationAlertMod : Mod
    {
        public CoolerOrientationAlertMod(ModContentPack content) : base(content)
        {
            new Harmony("saltgin.coolerorientationalert").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.DesignateSingleCell))]
    public static class Designator_Build_DesignateSingleCell_CoolerWarning
    {
        public static void Postfix(
            IntVec3 c,
            BuildableDef ___entDef,
            Rot4 ___placingRot)
        {
            ThingDef thingDef = ___entDef as ThingDef;
            if (!LooksLikeCooler(thingDef))
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null || !c.InBounds(map))
            {
                return;
            }

            IntVec3 coldCell = c + IntVec3.South.RotatedBy(___placingRot);

            // Treat out-of-bounds cold side as invalid orientation.
            if (!coldCell.InBounds(map))
            {
                Messages.Message(
                    "CoolerColdSideOutdoorWarning".Translate(thingDef.LabelCap),
                    new TargetInfo(c, map),
                    MessageTypeDefOf.CautionInput,
                    historical: false);
                return;
            }

            Room coldRoom = coldCell.GetRoom(map);
            if (coldRoom == null || coldRoom.UsesOutdoorTemperature)
            {
                Messages.Message(
                    "CoolerColdSideOutdoorWarning".Translate(thingDef.LabelCap),
                    new TargetInfo(c, map),
                    MessageTypeDefOf.CautionInput,
                    historical: false);
            }
        }

        private static bool LooksLikeCooler(ThingDef def)
        {
            if (def == null || def.category != ThingCategory.Building)
            {
                return false;
            }

            if (def.thingClass != null && typeof(Building_Cooler).IsAssignableFrom(def.thingClass))
            {
                return true;
            }

            if (def.PlaceWorkers != null)
            {
                for (int i = 0; i < def.PlaceWorkers.Count; i++)
                {
                    if (def.PlaceWorkers[i] is PlaceWorker_Cooler)
                    {
                        return true;
                    }
                }
            }

            if (def.comps != null && def.rotatable && def.building != null && def.building.canPlaceOverWall)
            {
                for (int i = 0; i < def.comps.Count; i++)
                {
                    CompProperties_TempControl temp = def.comps[i] as CompProperties_TempControl;
                    if (temp != null && temp.energyPerSecond < 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
