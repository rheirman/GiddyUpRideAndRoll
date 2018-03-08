using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
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
    [HarmonyPatch(typeof(JobDriver), "SetupToils")]
    class JobDriver_SetupToils
    {
        static void Postfix(ref JobDriver __instance)
        {
            JobDriver instance = __instance;
            List<Toil> toils = Traverse.Create(__instance).Field("toils").GetValue<List<Toil>>();
            if (__instance.pawn.Faction != Faction.OfPlayer || __instance.pawn.Drafted)
            {
                return;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store == null)
            {
                return;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
            Area_GU areaNoMount = (Area_GU)__instance.pawn.Map.areaManager.GetLabeled(Base.NOMOUNT_LABEL);
            Area_GU areaDropAnimal = (Area_GU)__instance.pawn.Map.areaManager.GetLabeled(Base.DROPANIMAL_LABEL);
            bool startedPark = false;
            IntVec3 originalLoc = new IntVec3();
            IntVec3 parkLoc = new IntVec3();
            if (pawnData.mount != null && areaNoMount != null && areaDropAnimal != null)
            {

                foreach (Toil toil in toils)
                {
                    /*
                    toil.AddFinishAction(delegate
                    {
                        Toil nextToil = null;
                        int nextToilIndex = Traverse.Create(__instance).Field("nextToilIndex").GetValue<int>();
                        if(nextToilIndex >= 0)
                        {
                            nextToil = toils[nextToilIndex];
                        }
                        nextToil.initAction.Method
                    });
                    */

                    //checkedToil makes sure the ActiveCells.Contains is only called once, preventing performance impact. 
                    bool checkedToil = false;
                    toil.AddPreTickAction(delegate
                    {

                        if (!checkedToil && pawnData.mount != null && areaNoMount.ActiveCells.Contains(toil.actor.pather.Destination.Cell))
                        {
                            //Toil parkToil = ParkToil(__instance, toils, pawnData, areaDropAnimal, toils[__instance.CurToilIndex]);
                            //toils.Add(parkToil);
                            parkLoc = DistanceUtility.getClosestAreaLoc(toil.actor.Position, areaDropAnimal);
                            originalLoc = toil.actor.pather.Destination.Cell;
                            toil.actor.pather.StartPath(parkLoc, PathEndMode.OnCell);
                            pawnData.selectedForCaravan = true;
                            startedPark = true;
                        }
                        checkedToil = true;
                        if (startedPark && toil.actor.pather.nextCell == parkLoc){
                            Log.Message("should get off animal");
                            pawnData.mount = null;
                            toil.actor.pather.StartPath(originalLoc, PathEndMode.ClosestTouch);
                            if (pawnData.owning != null)
                            {
                                ExtendedPawnData animalData = store.GetExtendedDataFor(pawnData.owning);
                                animalData.ownedBy = null;
                                pawnData.owning = null;
                            }

                        }
                    });
                }
            }

        }
        private static void Test(ref JobDriver __instance)
        {

        }

        private static Toil ParkToil(JobDriver __instance, List<Toil> toils, ExtendedPawnData pawnData, Area_GU areaDropAnimal, Toil OldToil)
        {

            Toil parkToil = new Toil();
            parkToil.initAction = delegate
            {
                Log.Message("parktoil initaction called");
                IntVec3 parkLoc = DistanceUtility.getClosestAreaLoc(__instance.pawn.Position, areaDropAnimal);
                parkToil.initAction = delegate
                {
                    Pawn actor = parkToil.actor;
                    actor.pather.StartPath(parkLoc, PathEndMode.ClosestTouch);
                };
                parkToil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                //parkToil.FailOnDespawnedOrNull(ind);

            };
            if (parkToil.finishActions == null)
            {
                parkToil.finishActions = new List<Action>();
            }
            parkToil.finishActions.Add(delegate {
                Log.Message("finishAction of parktoil called");
                pawnData.mount = null;
                //__instance.JumpToToil(OldToil);
            });
            return parkToil;
        }
    }




    
}
