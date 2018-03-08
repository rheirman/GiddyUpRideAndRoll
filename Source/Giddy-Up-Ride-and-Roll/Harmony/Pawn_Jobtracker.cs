using GiddyUpCore;
using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using GiddyUpCore.Zones;
using GiddyUpRideAndRoll.Jobs;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
    static class Pawn_Jobtracker
    {
        static void Postfix(Pawn_JobTracker __instance, ref ThinkResult __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.IsColonistPlayerControlled)
            {
                return;
            }
            if (__result.Job == null)
            {
                return;
            }
            if(__result.Job.def == GUC_JobDefOf.Mount)
            {
                return;
            }
            LocalTargetInfo target = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);


            if (!target.IsValid)
            {
                return;
            }
            Area_GU area = (Area_GU) pawn.Map.areaManager.GetLabeled(Base.NOMOUNT_LABEL);
            //TODO: make sure mounts are parked of when pawn wants to enter area with mount.

            if(Base.Instance == null)
            {
                return;
            }

            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);

            if (store == null || pawnData == null)
            {
                return;
            }
            if( pawnData.mount != null) {
                return;
            }
            Pawn closestAnimal;         
            if(pawnData.mount != null)
            {
                closestAnimal = pawnData.mount;
            }
            else
            {
                closestAnimal = GetClosestAnimal(pawn);
            }
            if (closestAnimal == null)
            {
                return;
            }

            //LocalTargetInfo dest = __result.Job.GetDestination(pawn);

            //Log.Message("pawn Position: " + pawn.Position);
            //Log.Message("target is valid: " + target.IsValid);
            //Log.Message("Job.GetDestination Position: " + dest.Cell);
            //Log.Message("target Position: " + target.Cell);

            //Log.Message(pawn)
            float pawnTargetDistance = DistanceUtility.QuickDistance(pawn.Position, target.Cell);
            //Log.Message("pawnTargetDistance: " + pawnTargetDistance);
            LocalTargetInfo targetB = null;
            float firstToSecondTargetDistance = 0;
            if (__result.Job.def == JobDefOf.HaulToCell || __result.Job.def == JobDefOf.HaulToContainer)
            {
                targetB = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.B);
                if (targetB.IsValid)
                {
                    firstToSecondTargetDistance = DistanceUtility.QuickDistance(target.Cell, targetB.Cell);
                }
            }
            // Log.Message("pawnTargetDistance: " + pawnTargetDistance);
            //Log.Message("pawnTargetDistanceB: " + firstToSecondTargetDistance);

            float totalDistance = pawnTargetDistance + firstToSecondTargetDistance;

            if (totalDistance > 250)
            {

                float pawnAnimalDistance = DistanceUtility.QuickDistance(pawn.Position, closestAnimal.Position); 
                float animalTargetDistance = DistanceUtility.QuickDistance(closestAnimal.Position, target.Cell);
                //Abstract unit of time. Real time values aren't needed, only relative values. 
                float timeNeededAlternative = (pawnAnimalDistance + animalTargetDistance + firstToSecondTargetDistance) * TicksPerMoveUtility.adjustedTicksPerMove(pawn, closestAnimal, true);
                //Log.Message("timeNeededAlternative: " + timeNeededAlternative);
                float timeNeededOriginal = (pawnTargetDistance + firstToSecondTargetDistance) * pawn.TicksPerMoveDiagonal;
                //Log.Message("timeNeededOriginal: " + timeNeededOriginal);
                //Log.Message("adjusted ticks per move: " + TicksPerMoveUtility.adjustedTicksPerMove(pawn, closestAnimal, true));
                //Log.Message("original ticks per move: " + pawn.TicksPerMoveDiagonal);

                if (timeNeededAlternative < timeNeededOriginal)
                {
                    __result = InsertMountingJobs(pawn, closestAnimal, target, pawnData, store.GetExtendedDataFor(closestAnimal), __instance, __result);
                }
            }

        }

        private static ThinkResult InsertMountingJobs(Pawn pawn, Pawn closestAnimal, LocalTargetInfo target, ExtendedPawnData pawnData, ExtendedPawnData animalData,  Pawn_JobTracker __instance, ThinkResult __result)
        {
            Job dismountJob = new Job(GUC_JobDefOf.Dismount);
            dismountJob.count = 1;
            Job mountJob = new Job(GUC_JobDefOf.Mount, closestAnimal);
            mountJob.count = 1;
            Job oldJob = __result.Job;
            pawnData.owning = closestAnimal;
            animalData.ownedBy = pawn;
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            //__instance.jobQueue.EnqueueFirst(dismountJob);

            __instance.jobQueue.EnqueueFirst(oldJob);
            __instance.jobQueue.EnqueueFirst(new Job(GU_RR_JobDefOf.RideToJob, target));

            __result = new ThinkResult(mountJob, __result.SourceNode, __result.Tag, false);
            return __result;
        }


        static Pawn GetClosestAnimal(Pawn pawn)
        {

            float minDistance = float.MaxValue;
            Pawn closestAnimal = null;
            foreach (Pawn animal in from p in pawn.Map.mapPawns.AllPawns
                                            where p.RaceProps.Animal
                                            select p)
            {
                float distance = DistanceUtility.QuickDistance(animal.Position, pawn.Position);
                if (distance < minDistance && IsMountableUtility.isMountable(animal) && animal.CurJob != null && animal.CurJob.def != GUC_JobDefOf.Mounted)
                {
                    closestAnimal = animal;
                    minDistance = distance;
                }
            }
            return closestAnimal;


        }
    }
}
