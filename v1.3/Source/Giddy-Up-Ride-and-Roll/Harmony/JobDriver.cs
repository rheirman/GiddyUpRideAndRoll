using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using GiddyUpCore.Zones;
using HarmonyLib;
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
            if (pawnData.mount != null && areaNoMount != null && areaDropAnimal != null)
            {

                foreach (Toil toil in ___toils)
                {
                    //checkedToil makes sure the ActiveCells.Contains is only called once, preventing performance impact. 
                    bool checkedToil = false;
                    toil.AddPreTickAction(delegate
                    {

                        if (!checkedToil && pawnData.mount != null && __instance.pawn.CurJobDef != JobDefOf.RopeToPen && areaNoMount.ActiveCells.Contains(toil.actor.pather.Destination.Cell))
                        {

                            var pen = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(__instance.pawn, pawnData.mount, out string failReason, true, true, false, true);
                            if (pen != null)
                            {
                                parkLoc = AnimalPenUtility.FindPlaceInPenToStand(pen, __instance.pawn);
                                originalLoc = toil.actor.pather.Destination.Cell;

                                if (toil.actor.Map.reachability.CanReach(toil.actor.Position, parkLoc, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                                {
                                    toil.actor.pather.StartPath(parkLoc, PathEndMode.OnCell);
                                    startedPark = true;
                                }
                            }
                        }
                        checkedToil = true;
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

    }
            







}
