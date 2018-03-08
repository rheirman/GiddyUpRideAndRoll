using GiddyUpCore.Storage;
using GiddyUpCore.Zones;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Harmony
{
    //Make sure mounted pawns avoid mount forbidden areas
    //TODO: test impact on performance
    /*
    [HarmonyPatch(typeof(GenPath), "ShouldNotEnterCell")]
    static class GenPath_ShouldNotEnterCell
    {
        static void Postfix(Pawn pawn, Map map, IntVec3 dest, ref bool __result)
        {
            if(pawn.Drafted || pawn.Faction != Faction.OfPlayer)
            {
                return;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store == null)
            {
                return;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);
            if(pawnData.mount == null)
            {
                return;
            }
            Area_GU areaNoMount = (Area_GU) pawn.Map.areaManager.GetLabeled(Base.NOMOUNT_LABEL);
            if (areaNoMount == null)
            {
                return;
            }
            if (areaNoMount.ActiveCells.Contains(dest))
            {
                Log.Message("pawn " + pawn.Name + " should not enter cell " + dest);
                __result = true;
            }

        }
    }
    */
}
