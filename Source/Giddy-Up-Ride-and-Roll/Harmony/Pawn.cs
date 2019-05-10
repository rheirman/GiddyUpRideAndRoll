using GiddyUpCore.Storage;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            if (__instance.RaceProps.Animal && __instance.CurJob != null && __instance.CurJob.def == GU_RR_DefOf.WaitForRider)
            {
                gizmoList.Add(CreateGizmo_LeaveRider(__instance));
            }
            __result = gizmoList;
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
                    __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            };
            return gizmo;
        }
    }
}
