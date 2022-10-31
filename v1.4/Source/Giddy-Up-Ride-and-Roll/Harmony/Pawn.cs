using GiddyUpCore.Storage;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }

            if (__instance.RaceProps.Animal && __instance.CurJob != null && __instance.CurJob.def == GU_RR_DefOf.WaitForRider)
            {
                yield return CreateGizmo_LeaveRider(__instance);
            }
        }
        private static Gizmo CreateGizmo_LeaveRider(Pawn __instance)
        {
            Gizmo gizmo = new Command_Action
            {
                defaultLabel = "GU_RR_Gizmo_LeaveRider_Label".Translate(),
                defaultDesc = "GU_RR_Gizmo_LeaveRider_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("UI/" + "LeaveRider"), true),
                action = () =>
                {
                    //__instance.jobs.EndCurrentJob(JobCondition.InterruptForced); moving to external method to sync across multiplayer clients
                    PawnEndCurrentJob(__instance);
                }
            };
            return gizmo;
        }
        [SyncMethod]
        private static void PawnEndCurrentJob(Pawn pawn)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}
