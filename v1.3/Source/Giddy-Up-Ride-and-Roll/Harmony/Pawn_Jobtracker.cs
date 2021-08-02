using GiddyUpCore;
using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using GiddyUpCore.Zones;
using GiddyUpRideAndRoll.Jobs;
using HarmonyLib;
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
        static void Postfix(Pawn_JobTracker __instance, ref ThinkResult __result, ref Pawn ___pawn)
        {
            if (!___pawn.IsColonistPlayerControlled || !___pawn.RaceProps.Humanlike)
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
            if (___pawn.Drafted)
            {
                return;
            }
            if (___pawn.InMentalState)
            {
                return;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store == null)
            {
                return;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(___pawn);
            if(pawnData.mount != null)
            {
                return; 
            }

            LocalTargetInfo firstTarget = null;
            LocalTargetInfo secondTarget = null;

            //For some jobs the first target is B, and the second A.
            if (__result.Job.def == JobDefOf.TendPatient || __result.Job.def == JobDefOf.Refuel || __result.Job.def == JobDefOf.FixBrokenDownBuilding)
            {
                firstTarget = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.B);
                secondTarget = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
            }
            else if (__result.Job.def == JobDefOf.DoBill && !__result.Job.targetQueueB.NullOrEmpty()) {
                firstTarget = __result.Job.targetQueueB[0];
                secondTarget = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
            }
            else
            {
                firstTarget = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.A);
                secondTarget = DistanceUtility.GetFirstTarget(__result.Job, TargetIndex.B);
            }
            if (!firstTarget.IsValid)
            {
                return;
            }

            if (Base.Instance == null)
            {
                return;
            }
            if (store == null || pawnData == null)
            {
                return;
            }
            if (pawnData.wasRidingToJob)
            {
                pawnData.wasRidingToJob = false;
                return;
            }

            if(___pawn.mindState != null && ___pawn.mindState.duty != null && (___pawn.mindState.duty.def == DutyDefOf.TravelOrWait || ___pawn.mindState.duty.def == DutyDefOf.TravelOrLeave))
            {
                return;
            }

            Pawn bestChoiceAnimal = null;
            //Pawn bestChoiceAnimal = null;

            float pawnTargetDistance = DistanceUtility.QuickDistance(___pawn.Position, firstTarget.Cell);
            Log.Message("pawnTargetDistance: " + pawnTargetDistance);
            float firstToSecondTargetDistance = 0;
            if (__result.Job.def == JobDefOf.HaulToCell || __result.Job.def == JobDefOf.HaulToContainer)
            {
                if (secondTarget.IsValid)
                {
                    firstToSecondTargetDistance = DistanceUtility.QuickDistance(firstTarget.Cell, secondTarget.Cell);
                    Log.Message("firstToSecondTargetDistance: " + pawnTargetDistance);

                }
            }
            float totalDistance = pawnTargetDistance + firstToSecondTargetDistance;
            Log.Message("totalDistance: " + totalDistance);
            if (totalDistance > Base.minAutoMountDistance)
            {
                bestChoiceAnimal = GetBestChoiceAnimal(___pawn, firstTarget, secondTarget, pawnTargetDistance, firstToSecondTargetDistance, store);
                if (bestChoiceAnimal != null)
                {
                    __result = InsertMountingJobs(___pawn, bestChoiceAnimal, firstTarget, secondTarget, ref pawnData, store.GetExtendedDataFor(bestChoiceAnimal), __instance, __result);
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
            float timeNeededMin = (pawnTargetDistance + firstToSecondTargetDistance) / pawn.GetStatValue(StatDefOf.MoveSpeed);
            Log.Message("timeNeededMin: " + timeNeededMin);
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
            if (pawnData.owning != null && pawnData.owning.Spawned && !AnimalNotAvailable(pawnData.owning, pawn) && pawn.CanReserve(pawnData.owning))
            {
                var timeNeededWhenMounted = CalculateTimeNeeded(pawn, ref target, secondTarget, firstToSecondTargetDistance, pawnData.owning, firstTargetNoMount, secondTargetNoMount, areaDropAnimal);
                if (timeNeededWhenMounted < timeNeededMin)
                {
                    return pawnData.owning;
                }
            }
            //Otherwise search the animal on the map that gets you to the goal the quickest
            foreach (Pawn animal in from p in pawn.Map.mapPawns.AllPawnsSpawned
                                    where p.RaceProps.Animal && IsMountableUtility.isMountable(p) && p.CurJob != null && p.CurJob.def != GUC_JobDefOf.Mounted
                                    select p)
            {
                if (AnimalNotAvailable(animal, pawn) || !pawn.CanReserve(animal))
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
                    if (animal.playerSettings != null && animal.playerSettings.Master != pawn)
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
                __instance.jobQueue.EnqueueFirst(oldJob);
                __result = new ThinkResult(mountJob, __result.SourceNode, __result.Tag, false);
            }
            return __result;
        }

        private static bool AnimalNotAvailable(Pawn animal, Pawn rider)
        {
            if (animal.Dead || animal.Downed || animal.IsBurning() || animal.InMentalState || !animal.Spawned) //animal in bad state, should return before checking other things
            {
                return true;
            }

            if (animal.IsForbidden(rider))
            {
                return true;
            }

            if (animal.Faction == null || animal.Faction != Faction.OfPlayer) //animal has wrong faction
            {
                return true;
            }

            if (animal.health != null && animal.health.summaryHealth.SummaryHealthPercent < 1) //animal wounded
            {
                return true;
            }
            if (animal.health.HasHediffsNeedingTend())
            {
                return true;
            }
            if (animal.HungryOrTired())
            {
                return true;
            }

            if (animal.GetLord() != null)
            {
                if (animal.GetLord().LordJob != null && animal.GetLord().LordJob is LordJob_FormAndSendCaravan) //animal forming caravan
                {
                    return true;
                }
            }
            if (animal.CurJob != null && animal.CurJob.def == JobDefOf.LayDown && animal.needs != null && animal.needs.rest.CurLevelPercentage < 0.5f)//only allow resting animals if they have enough energy. 
            {
                return true;
            }

            if (animal.CurJob != null && (animal.CurJob.def == JobDefOf.Lovin || animal.CurJob.def == JobDefOf.Ingest || animal.CurJob.def == GUC_JobDefOf.Mounted)) //animal occupied
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
            if (firstTargetNoMount && areaDropAnimal != null)
            {
                rideDistance = 0;
                IntVec3 parkLoc = DistanceUtility.getClosestAreaLoc(animal.Position, areaDropAnimal);
                rideDistance += DistanceUtility.QuickDistance(animal.Position, parkLoc);
                walkDistance += DistanceUtility.QuickDistance(parkLoc, target.Cell);
                walkDistance += firstToSecondTargetDistance;
            }
            else if (secondTargetNoMount && secondTarget != null && secondTarget.IsValid && areaDropAnimal != null)
            {
                IntVec3 parkLoc = DistanceUtility.getClosestAreaLoc(target.Cell, areaDropAnimal);
                rideDistance += DistanceUtility.QuickDistance(target.Cell, parkLoc);
                walkDistance += DistanceUtility.QuickDistance(parkLoc, secondTarget.Cell);
            }
            else
            {
                rideDistance += firstToSecondTargetDistance;
            }
            Log.Message("Animal: " + animal.Name);
            Log.Message("walk distance to animal: " + walkDistance);
            Log.Message("ride distance: " + rideDistance);

            var animalBaseSpeed = animal.GetStatValue(StatDefOf.MoveSpeed);
            Log.Message("animal base speed: " + animalBaseSpeed);
            var pawnPaseSpeed = pawn.GetStatValue(StatDefOf.MoveSpeed);
            Log.Message("pawn base speed: " + pawnPaseSpeed);

            var animalMountedSpeed = GiddyUpCore.Stats.StatPart_Riding.GetRidingSpeed(animalBaseSpeed, animal, pawn);

            Log.Message("animalMountedSpeed: " + animalMountedSpeed);

            float timeNeeded = walkDistance/pawnPaseSpeed + rideDistance/animalMountedSpeed;
            Log.Message("timeNeeded: " + timeNeeded);
            return timeNeeded;
        }

    }

}
