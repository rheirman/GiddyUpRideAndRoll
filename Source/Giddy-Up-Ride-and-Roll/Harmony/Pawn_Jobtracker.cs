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
            if (pawn.Drafted)
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
            if (pawnData.wasRidingToJob)
            {
                pawnData.wasRidingToJob = false;
                return;
            }

            Pawn bestChoiceAnimal;

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

                bestChoiceAnimal = GetBestChoiceAnimal(pawn, target, pawnTargetDistance, firstToSecondTargetDistance, store);
                if (bestChoiceAnimal != null)
                {
                    __result = InsertMountingJobs(pawn, bestChoiceAnimal, target, targetB, pawnData, store.GetExtendedDataFor(bestChoiceAnimal), __instance, __result);
                }

                //Log.Message("timeNeededOriginal: " + timeNeededOriginal);
                //Log.Message("adjusted ticks per move: " + TicksPerMoveUtility.adjustedTicksPerMove(pawn, closestAnimal, true));
                //Log.Message("original ticks per move: " + pawn.TicksPerMoveDiagonal);
            }
        }

        private static ThinkResult InsertMountingJobs(Pawn pawn, Pawn closestAnimal, LocalTargetInfo target, LocalTargetInfo secondTarget, ExtendedPawnData pawnData, ExtendedPawnData animalData,  Pawn_JobTracker __instance, ThinkResult __result)
        {
            Job dismountJob = new Job(GUC_JobDefOf.Dismount);
            Job mountJob = new Job(GUC_JobDefOf.Mount, closestAnimal, target, secondTarget);
            Job rideToJob = new Job(GU_RR_JobDefOf.RideToJob, target, secondTarget);
            Job oldJob = __result.Job;

            mountJob.count = 1;
            rideToJob.count = 1;
            pawnData.owning = closestAnimal;
            animalData.ownedBy = pawn;
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            //__instance.jobQueue.EnqueueFirst(dismountJob);
            if (pawn.CanReserve(target))
            {
                if(target.Thing is Building_Bed)
                {
                    Building_Bed bed = (Building_Bed)target.Thing;
                    
                    mountJob.count = bed.SleepingSlotsCount;
                    rideToJob.count = bed.SleepingSlotsCount;
                }
                if(oldJob.def.joyMaxParticipants > 1)
                {
                    mountJob.count = oldJob.def.joyMaxParticipants;
                    rideToJob.count = oldJob.def.joyMaxParticipants;
                }
                
                __instance.jobQueue.EnqueueFirst(oldJob);
                __instance.jobQueue.EnqueueFirst(rideToJob);

                __result = new ThinkResult(mountJob, __result.SourceNode, __result.Tag, false);
            }
            else
            {
                Log.Message("cannot reserve target!" + pawn.Name);
            }


            return __result;
        }

        //Gets animal that'll get the pawn to the target the quickest. Returns null if no animal is found or if walking is faster. 
        static Pawn GetBestChoiceAnimal(Pawn pawn, LocalTargetInfo target, float pawnTargetDistance, float firstToSecondTargetDistance, ExtendedDataStorage store)
        {

            //float minDistance = float.MaxValue;
            Pawn closestAnimal = null;
            float timeNeededMin = (pawnTargetDistance + firstToSecondTargetDistance) * pawn.TicksPerMoveDiagonal;

            foreach (Pawn animal in from p in pawn.Map.mapPawns.AllPawnsSpawned
                                            where p.RaceProps.Animal && IsMountableUtility.isMountable(p) && p.CurJob.def != GUC_JobDefOf.Mounted
                                            select p)
            {
                if(animal.Dead || animal.Downed || animal.IsBurning() || animal.InMentalState)
                {
                    continue;
                }
                ExtendedPawnData pawnData = store.GetExtendedDataFor(animal);
                if (!pawnData.mountableByMaster && !pawnData.mountableByAnyone)
                {
                    continue;
                }
                else if (!pawnData.mountableByAnyone && pawnData.mountableByMaster)
                {
                    if(animal.playerSettings != null && animal.playerSettings.master != pawn)
                    {
                        continue;
                    }
                }

                float pawnAnimalDistance = DistanceUtility.QuickDistance(pawn.Position, animal.Position);
                float animalTargetDistance = DistanceUtility.QuickDistance(animal.Position, target.Cell);
                float timeNeeded = (pawnAnimalDistance + animalTargetDistance + firstToSecondTargetDistance) * TicksPerMoveUtility.adjustedTicksPerMove(pawn, animal, true);

                if(timeNeeded < timeNeededMin)
                {
                    closestAnimal = animal;
                }
            }

            //Abstract unit of time. Real time values aren't needed, only relative values. 
            //Log.Message("timeNeededAlternative: " + timeNeededAlternative);

            return closestAnimal;
        }
    }
}
