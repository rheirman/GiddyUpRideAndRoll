using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using GiddyUpCore.Zones;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Harmony
{

    [HarmonyPatch(typeof(JobDriver), "SetupToils")]
    class JobDriver_SetupToils
    {
        static void Postfix(JobDriver __instance, ref List<Toil> ___toils)
        {            
            if (__instance.pawn.Map == null)
            {
                return;
            }
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
            PathEndMode endMode;

            if (pawnData.mount != null && areaNoMount != null)
            {
                foreach (Toil toil in ___toils)
                {
                    //checkedToil makes sure the ActiveCells.Contains is only called once, preventing performance impact. 
                    toil.AddPreTickAction(delegate
                    {

                        if (__instance.pawn.IsHashIntervalTick(60) && !startedPark && pawnData.mount != null && __instance.pawn.CurJobDef != JobDefOf.RopeToPen && areaNoMount.ActiveCells.Contains(toil.actor.pather.Destination.Cell))
                        {
                            originalLoc = toil.actor.pather.Destination.Cell;
                            if (AnimalPenUtility.NeedsToBeManagedByRope(pawnData.mount) || areaDropAnimal == null)
                            {
                                startedPark = TryParkAnimalPen(__instance, pawnData, ref parkLoc, toil);
                            }
                            else
                            {
                                startedPark = TryParkAnimalDropSpot(areaDropAnimal, ref parkLoc, toil);
                            }

                        }
                        //checkedToil = true;
                        if (startedPark && toil.actor.pather.nextCell == parkLoc)
                        {
                            pawnData.mount = null;
                            toil.actor.pather.StartPath(originalLoc, PathEndMode.OnCell);
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

        private static bool TryParkAnimalPen(JobDriver __instance, ExtendedPawnData pawnData, ref IntVec3 parkLoc, Toil toil)
        {
            bool succeeded = false;
            var pen = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(__instance.pawn, pawnData.mount, out string failReason, true, true, false, true);
            if (pen != null)
            {
                parkLoc = AnimalPenUtility.FindPlaceInPenToStand(pen, __instance.pawn);

                if (toil.actor.Map.reachability.CanReach(toil.actor.Position, parkLoc, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                {
                    toil.actor.pather.StartPath(parkLoc, PathEndMode.OnCell);
                    succeeded = true;
                }
            }
            else
            {
                Log.Message(pawnData.mount.Name + " failed: " + failReason);
            }
            return succeeded;
        }

        private static bool TryParkAnimalDropSpot(Area_GU areaDropAnimal, ref IntVec3 parkLoc, Toil toil)
        {
            
            bool succeeded = false;
            parkLoc = DistanceUtility.getClosestAreaLoc(toil.actor.pather.Destination.Cell, areaDropAnimal);
            if (toil.actor.Map.reachability.CanReach(toil.actor.Position, parkLoc, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
            {
                toil.actor.pather.StartPath(parkLoc, PathEndMode.OnCell);
                succeeded = true;
            }
            else
            {
                Messages.Message("GU_RR_NotReachable_DropAnimal_Message".Translate(), new RimWorld.Planet.GlobalTargetInfo(parkLoc, toil.actor.Map), MessageTypeDefOf.NegativeEvent);
            }
            return succeeded;
        }
    }
            







}
