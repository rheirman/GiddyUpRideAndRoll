using GiddyUpCore.Utilities;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(CompAnimalPenMarker), "AcceptsToPen")]
    class CompAnimalPenMarker_AcceptsToPen
    {
        static void Postfix(Pawn animal, ref bool __result)
        {
            if (!__result)
            {
                __result = IsMountableUtility.IsCurrentlyMounted(animal);
            }
            //if(store != null && store.GetExtendedDataFor(pawn) is ExtendedPawnData pawnData)
            //{
            //    __result = pawnData.wasRidingToJob;
            //}
        }
    }

}
