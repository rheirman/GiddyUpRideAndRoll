using GiddyUpCore.Utilities;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(WorkGiver_Train), "JobOnThing")]
    class WorkGiver_Train_JobOnThing
    {
        static bool Prefix(WorkGiver_Train __instance, Pawn pawn, Thing t, ref Job __result)
        {
            if (t is Pawn animal && animal.CurJobDef == GU_RR_DefOf.WaitForRider)
            {
                __result = null;
                return false;
            }
            return true;
        }
    }
}
