using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
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
            //TODO: refactor this. too much duplicate code in this and jobdriver_RideToJob
            if (__instance.job.count == -1)
            {
                __instance.job.count = 1;
            }
            int stackCount = -1;
            if (__instance.job.count > 1)
            {
                stackCount = 0;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();

            if (store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                LocalTargetInfo extraTarget = pawnData.extraJobTarget;
                if (extraTarget != null)
                {
                    __result = __instance.pawn.Reserve(extraTarget, __instance.job, 1, -1, null);
                }
                pawnData.extraJobTarget = null;
            }
            if (!__instance.job.targetQueueA.NullOrEmpty())
            {
                __instance.pawn.ReserveAsManyAsPossible(__instance.job.targetQueueA, __instance.job, 1, -1, null);
            }
            if (!__instance.job.targetQueueB.NullOrEmpty())
            {
                __instance.pawn.ReserveAsManyAsPossible(__instance.job.targetQueueB, __instance.job, 1, -1, null);
            }
            if (__instance.job.targetB != null && __instance.job.targetC != null)
            {
                __result = __result && __instance.pawn.Reserve(__instance.job.GetTarget(TargetIndex.A), __instance.job, 1, -1, null) &&
                    __instance.pawn.Reserve(__instance.job.GetTarget(TargetIndex.B), __instance.job, 1, -1, null) &&
                    __instance.pawn.Reserve(__instance.job.GetTarget(TargetIndex.C), __instance.job, 1, -1, null);
                return;
            }
            else if (__instance.job.targetB != null)
            {
                __result = __result && __instance.pawn.Reserve(__instance.job.GetTarget(TargetIndex.A), __instance.job, 1, -1, null) && __instance.pawn.Reserve(__instance.job.GetTarget(TargetIndex.B), __instance.job, __instance.job.count, stackCount, null);
            }
            return;
        }
    }
}
