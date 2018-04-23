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
using Verse.AI.Group;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
    static class Pawn_Jobtracker_DetermineNextJob
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
            if (__result.Job.def == GUC_JobDefOf.Mount)
            {
                return;
            }
            if (pawn.Drafted)
            {
                return;
            }
            if (pawn.InMentalState)
            {
                return;
            }

            LocalTargetInfo target = null;
            LocalTargetInfo targetB = null;

            //For some jobs the first target is B, and the second A.
            if (__result.Job.def == JobDefOf.TendPatient || __result.Job.def == JobDefOf.Refuel)
            {
                target = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.B);
                targetB = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
            }
            else if (__result.Job.def == JobDefOf.DoBill && !__result.Job.targetQueueB.NullOrEmpty()) {
                target = __result.Job.targetQueueB[0];
                targetB = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
            }
            else
            {
                target = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
                targetB = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.B);
            }



            if (!target.IsValid)
            {
                return;
            }
            Area_GU area = (Area_GU)pawn.Map.areaManager.GetLabeled(Base.NOMOUNT_LABEL);
            //TODO: make sure mounts are parked of when pawn wants to enter area with mount.

            if (Base.Instance == null)
            {
                return;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);

            if (store == null || pawnData == null)
            {
                return;
            }
            if (pawnData.wasRidingToJob)
            {
                pawnData.wasRidingToJob = false;
                return;
            }

            if(pawn.mindState != null && pawn.mindState.duty != null && (pawn.mindState.duty.def == DutyDefOf.TravelOrWait || pawn.mindState.duty.def == DutyDefOf.TravelOrLeave))
            {
                return;
            }

            Pawn bestChoiceAnimal = pawnData.mount;
            //Pawn bestChoiceAnimal = null;

            float pawnTargetDistance = DistanceUtility.QuickDistance(pawn.Position, target.Cell);
            //Log.Message("pawnTargetDistance: " + pawnTargetDistance);
            float firstToSecondTargetDistance = 0;
            if (__result.Job.def == JobDefOf.HaulToCell || __result.Job.def == JobDefOf.HaulToContainer)
            {
                if (targetB.IsValid)
                {
                    firstToSecondTargetDistance = DistanceUtility.QuickDistance(target.Cell, targetB.Cell);
                }
            }

            float totalDistance = pawnTargetDistance + firstToSecondTargetDistance;
            


            if (totalDistance > Base.minAutoMountDistance)
            {
                if(pawnData.mount == null){
                    bestChoiceAnimal = GetBestChoiceAnimal(pawn, target, targetB, pawnTargetDistance, firstToSecondTargetDistance, store);
                }

                if (bestChoiceAnimal != null)
                {
                    __result = InsertMountingJobs(pawn, bestChoiceAnimal, target, targetB, ref pawnData, store.GetExtendedDataFor(bestChoiceAnimal), __instance, __result);
                }

                //Log.Message("timeNeededOriginal: " + timeNeededOriginal);
                //Log.Message("adjusted ticks per move: " + TicksPerMoveUtility.adjustedTicksPerMove(pawn, closestAnimal, true));
                //Log.Message("original ticks per move: " + pawn.TicksPerMoveDiagonal);
            }
        }


        //Gets animal that'll get the pawn to the target the quickest. Returns null if no animal is found or if walking is faster. 
        static Pawn GetBestChoiceAnimal(Pawn pawn, LocalTargetInfo target, LocalTargetInfo secondTarget, float pawnTargetDistance, float firstToSecondTargetDistance, ExtendedDataStorage store)
        {

            //float minDistance = float.MaxValue;
            Pawn closestAnimal = null;
            float timeNeededMin = (pawnTargetDistance + firstToSecondTargetDistance) * pawn.TicksPerMoveDiagonal;
            ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);
            bool firstTargetNoMount = false;
            bool secondTargetNoMount = false;

            Area_GU areaNoMount = (Area_GU)pawn.Map.areaManager.GetLabeled(Base.NOMOUNT_LABEL);
            Area_GU areaDropAnimal = (Area_GU)pawn.Map.areaManager.GetLabeled(Base.DROPANIMAL_LABEL);

            if (areaNoMount != null && areaNoMount.ActiveCells.Contains(target.Cell))
            {
                firstTargetNoMount = true;
                if(pawnTargetDistance < Base.minAutoMountDistance)
                {
                    return null;
                }
            }
            

            //If owning an animal, prefer this animal if it still gets you to the goal quicker than walking. 
            //This'll make sure pawns prefer the animals they were already riding previously.
            if (pawnData.owning != null && pawnData.owning.Spawned && !AnimalNotAvailable(pawnData.owning) && pawn.CanReserve(pawnData.owning))
            {
                if (CalculateTimeNeeded(pawn, ref target, secondTarget, firstToSecondTargetDistance, pawnData.owning, firstTargetNoMount, secondTargetNoMount, areaDropAnimal) < timeNeededMin)
                {
                    return pawnData.owning;
                }
            }
            //Otherwise search the animal on the map that gets you to the goal the quickest
            foreach (Pawn animal in from p in pawn.Map.mapPawns.AllPawnsSpawned
                                    where p.RaceProps.Animal && IsMountableUtility.isMountable(p) && p.CurJob != null && p.CurJob.def != GUC_JobDefOf.Mounted
                                    select p)
            {
                if (AnimalNotAvailable(animal) || !pawn.CanReserve(animal))
                {
                    continue;
                }
                float distanceFromAnimal = DistanceUtility.QuickDistance(animal.Position, target.Cell);
                if (!firstTargetNoMount)
                {
                    distanceFromAnimal += firstToSecondTargetDistance;
                }
                if(distanceFromAnimal < Base.minAutoMountDistanceFromAnimal)
                {
                    continue;
                }
                ExtendedPawnData animalData = store.GetExtendedDataFor(animal);
                if(animalData.ownedBy != null)
                {
                    continue;
                }
                if (!animalData.mountableByMaster && !animalData.mountableByAnyone)
                {
                    continue;
                }
                else if (!animalData.mountableByAnyone && animalData.mountableByMaster)
                {
                    if (animal.playerSettings != null && animal.playerSettings.master != pawn)
                    {
                        continue;
                    }
                }

                float timeNeeded = CalculateTimeNeeded(pawn, ref target, secondTarget, firstToSecondTargetDistance, animal, firstTargetNoMount, secondTargetNoMount, areaDropAnimal);

                if (timeNeeded < timeNeededMin)
                {
                    closestAnimal = animal;
                    timeNeededMin = timeNeeded;
                }
            }
            return closestAnimal;
        }

        private static ThinkResult InsertMountingJobs(Pawn pawn, Pawn closestAnimal, LocalTargetInfo target, LocalTargetInfo secondTarget, ref ExtendedPawnData pawnData, ExtendedPawnData animalData, Pawn_JobTracker __instance, ThinkResult __result)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            //__instance.jobQueue.EnqueueFirst(dismountJob);
            if (pawn.CanReserve(target) && pawn.CanReserve(closestAnimal))
            {

                Job oldJob = __result.Job;

                ExtendedPawnData pawnDataTest = store.GetExtendedDataFor(pawn);
                pawnDataTest.targetJob = oldJob;
                Job mountJob = new Job(GUC_JobDefOf.Mount, closestAnimal);
                Job rideToJob = new Job(GU_RR_JobDefOf.RideToJob, closestAnimal, target);

                if (pawnData.mount != null)
                {
                    __instance.jobQueue.EnqueueFirst(oldJob);
                    __result = new ThinkResult(rideToJob, __result.SourceNode, __result.Tag, false);
                }
                else
                {
                    __instance.jobQueue.EnqueueFirst(oldJob);
                    __instance.jobQueue.EnqueueFirst(rideToJob);
                    __result = new ThinkResult(mountJob, __result.SourceNode, __result.Tag, false);
                }
            }
            return __result;
        }

        private static bool AnimalNotAvailable(Pawn animal)
        {
            if (animal.Dead || animal.Downed || animal.IsBurning() || animal.InMentalState) //animal in bad state, should return before checking other things
            {
                return true; 
            }
            if(animal.Faction == null || animal.Faction != Faction.OfPlayer) //animal has wrong faction
            {
                return true;
            }
            if(animal.health.summaryHealth.SummaryHealthPercent < 1) //animal wounded
            {
                return true;
            }
            if((animal.needs.food.CurCategory == HungerCategory.UrgentlyHungry) || (animal.needs.rest.CurCategory == RestCategory.VeryTired)){ //animal needs break
                return true;
            }
            if(animal.GetLord() != null)
            {
                if(animal.GetLord().LordJob != null && animal.GetLord().LordJob is LordJob_FormAndSendCaravan) //animal forming caravan
                {
                    return true;
                }
            }
            if (animal.CurJob != null && (animal.CurJob.def == JobDefOf.LayDown || animal.CurJob.def == JobDefOf.Lovin || animal.CurJob.def == JobDefOf.Ingest || animal.CurJob.def == GUC_JobDefOf.Mounted)) //animal occupied
            {
                return true;
            }
            return false;

        }

        //uses abstract unit of time. Real time values aren't needed, only relative values. 
        private static float CalculateTimeNeeded(Pawn pawn, ref LocalTargetInfo target, LocalTargetInfo secondTarget, float firstToSecondTargetDistance, Pawn animal, bool firstTargetNoMount, bool secondTargetNoMount, Area_GU areaDropAnimal)
        {
            

            float walkDistance = DistanceUtility.QuickDistance(pawn.Position, animal.Position);
            float rideDistance = DistanceUtility.QuickDistance(animal.Position, target.Cell);
            if (firstTargetNoMount)
            {
                rideDistance = 0;
                IntVec3 parkLoc = DistanceUtility.getClosestAreaLoc(animal.Position, areaDropAnimal);
                rideDistance += DistanceUtility.QuickDistance(animal.Position, parkLoc);
                walkDistance += DistanceUtility.QuickDistance(parkLoc, target.Cell);
                walkDistance += firstToSecondTargetDistance;
            }
            else if (secondTargetNoMount && secondTarget != null && secondTarget.IsValid)
            {
                IntVec3 parkLoc = DistanceUtility.getClosestAreaLoc(target.Cell, areaDropAnimal);
                rideDistance += DistanceUtility.QuickDistance(target.Cell, parkLoc);
                walkDistance += DistanceUtility.QuickDistance(parkLoc, secondTarget.Cell);
            }
            else
            {
                rideDistance += firstToSecondTargetDistance;
            }

            int adjustedTicksPerMove = TicksPerMoveUtility.adjustedTicksPerMove(pawn, animal, true);

            float timeNeeded = walkDistance * pawn.TicksPerMoveDiagonal + rideDistance * adjustedTicksPerMove;
            return timeNeeded;
        }

    }

    //This patch ensures animals stop waiting for their owner if the owner departs without the animal. After this happens, the owner is no longer an owner. 
    [HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
    [HarmonyPriority(Priority.Low)]
    static class Pawn_Jobtracker_DetermineNextJob2
    {
        static void Postfix(Pawn_JobTracker __instance, ref ThinkResult __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.IsColonistPlayerControlled)
            {
                return;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);

            if (store == null || pawnData == null)
            {
                return;
            }

            if (pawnData.owning == null || !pawnData.owning.Spawned || pawnData.owning.Downed || pawnData.owning.Dead || pawnData.mount != null)
            {
                return;
            }

            LocalTargetInfo target = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
            float pawnTargetDistance = DistanceUtility.QuickDistance(pawnData.owning.Position, target.Cell);

            if (pawnTargetDistance > 10 || __result.Job.def == JobDefOf.LayDown || __result.Job.def == JobDefOf.Research || pawn.InMentalState || pawn.Dead || pawn.Downed)
            {

                if (pawnData.owning.jobs.curJob != null && pawnData.owning.jobs.curJob.def == JobDefOf.Wait)
                {

                    pawnData.owning.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                ExtendedPawnData animalData = store.GetExtendedDataFor(pawnData.owning);
                pawnData.owning = null;
                animalData.ownedBy = null;

            }

        }
    }

}
