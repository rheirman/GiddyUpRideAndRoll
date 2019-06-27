using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using GiddyUpCore.Utilities;

namespace GiddyUpRideAndRoll.Harmony
{
    //Don't let pawns train mounted animals. 
    [HarmonyPatch(typeof(WorkGiver_Train),"JobOnThing")]
    class WorkGiver_Train_JobOnThing
    {
        static bool Prefix(WorkGiver_Train __instance, Pawn pawn, Thing t, ref Job __result)
        {
            if(t is Pawn animal && animal.RaceProps.Animal && IsMountableUtility.IsCurrentlyMounted(animal))
            {
                __result = null;
                return false;
            }
            return true;
        }
    }
}
