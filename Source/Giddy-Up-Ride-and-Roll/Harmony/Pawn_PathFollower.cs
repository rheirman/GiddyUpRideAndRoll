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
    //Didn't get this working yet without severely affecting performance. Will probably be too much effort for the given benefit. 
    /*
    [HarmonyPatch(typeof(RegionCostCalculator), "GetCellCostFast")]
    static class Pawn_PathFollower_GetCellCostFast
    {
        static void Postfix(RegionCostCalculator __instance, int index, ref int __result)
        {

            TraverseParms parms = Traverse.Create(__instance).Field("traverseParms").GetValue<TraverseParms>();
            Pawn pawn = parms.pawn;
            IntVec3 cell = CellIndicesUtility.IndexToCell(index, pawn.Map.pathGrid.pathGrid.Count());
            Log.Message("cell: + " + cell.ToString());
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
            if (areaNoMount.ActiveCells.Contains(cell))
            {
                Log.Message("pawn " + pawn.Name + " should not enter cell " + cell);
                __result += 100;
            }

        }
    }
    */

}
