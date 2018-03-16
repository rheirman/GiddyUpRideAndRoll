using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using GiddyUpRideAndRoll.Utilities;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(JobDriver_Mount), "TryMakePreToilReservations")]
    class JobDriver_Mount_TryMakePreToilReservations
    {

        static void Postfix(JobDriver_Mounted __instance, ref bool __result)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store == null)
            {
                __result = true;
                return;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
            if (pawnData.targetJob == null)
            {
                __result = true;
                return;
            }
            __result = ReserveUtility.ReserveEveryThingOfJob(pawnData.targetJob, __instance);
        }
    }
    [HarmonyPatch(typeof(JobDriver_Mount), "FinishAction")]
    class JobDriver_Mount_FinishAction
    {
        static void Postfix(JobDriver_Mount __instance)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
            ExtendedPawnData animalData = store.GetExtendedDataFor(__instance.Mount);
            pawnData.owning = __instance.Mount;
            animalData.ownedBy = __instance.pawn;
        }
    }
    
}
