using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using GiddyUpCore.Zones;

namespace GiddyUpRideAndRoll.Alerts
{
    //class Alert_NoDropAnimal : RimWorld.Alert
    //{
    //    public Alert_NoDropAnimal()
    //    {
    //        this.defaultLabel = "GU_RR_NoDropAnimal_Label".Translate();
    //        this.defaultExplanation = "GU_RR_NoDropAnimal_Description".Translate();
    //    }
    //    public override AlertReport GetReport()
    //    {
    //        return this.ShouldAlert();
    //    }
    //    private bool ShouldAlert()
    //    {
    //        List<Map> maps = Find.Maps;
    //        foreach (Map map in maps)
    //        {
    //            Area_GU areaNoMount = (Area_GU)map.areaManager.GetLabeled(Base.NOMOUNT_LABEL);
    //            Area_GU areaDropAnimal = (Area_GU)map.areaManager.GetLabeled(Base.DROPANIMAL_LABEL);
    //            var unropablePlayerAnimals = map.mapPawns.SpawnedColonyAnimals.Where(animal => animal.Faction == Faction.OfPlayer && !AnimalPenUtility.NeedsToBeManagedByRope(animal));

    //            if (unropablePlayerAnimals.Count() > 0 && areaNoMount != null && areaDropAnimal != null && areaNoMount.ActiveCells.Count() > 0 && areaDropAnimal.ActiveCells.Count() == 0)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //}
}
