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
        static void Postfix(JobDriver __instance)
        {            
            if (__instance.pawn.Map == null)
            {
                return;
            }
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
                    //checkedToil makes sure the ActiveCells.Contains is only called once, preventing performance impact. 
                    bool checkedToil = false;
                    toil.AddPreTickAction(delegate
                    {

                        if (!checkedToil && pawnData.mount != null && areaDropAnimal.ActiveCells.Count() > 0 && areaNoMount.ActiveCells.Contains(toil.actor.pather.Destination.Cell))
                        {
                            //Toil parkToil = ParkToil(__instance, toils, pawnData, areaDropAnimal, toils[__instance.CurToilIndex]);
                            //toils.Add(parkToil);
                            parkLoc = DistanceUtility.getClosestAreaLoc(toil.actor.pather.Destination.Cell, areaDropAnimal);
                            originalLoc = toil.actor.pather.Destination.Cell;
                            if (toil.actor.Map.reachability.CanReach(toil.actor.Position, parkLoc, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                            {
                                toil.actor.pather.StartPath(parkLoc, PathEndMode.OnCell);
                                startedPark = true;
                            }
                            else
                            {
                                Messages.Message("GU_RR_NotReachableDropAnimal_Message".Translate(), new RimWorld.Planet.GlobalTargetInfo(parkLoc, toil.actor.Map), MessageTypeDefOf.NegativeEvent);
                            }
                        }
                        checkedToil = true;
                        if (startedPark && toil.actor.pather.nextCell == parkLoc){
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

    }
            







}
