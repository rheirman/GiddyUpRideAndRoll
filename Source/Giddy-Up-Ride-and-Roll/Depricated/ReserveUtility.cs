using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Utilities
{
    public static class ReserveUtility
    {
        /**
          * DEPRICATED. For save compatibility this class isn't removed. 
          */

        public static bool ReserveEveryThingOfJob(Job targetJob, JobDriver jobDriver)
        {
            int targetACount = 1;
            int targetBCount = 1;
            bool result = true;
            
            /*
            if (targetJob.targetA.Thing is Building_Bed)
            {
                Building_Bed bed = (Building_Bed)targetJob.targetB.Thing;
                //targetBCount = bed.SleepingSlotsCount;
                return jobDriver.pawn.Reserve(bed, targetJob, bed.SleepingSlotsCount, 0, null, false);
            }
            */
            
            /*
            if (targetJob.def.joyMaxParticipants > 1)
            {
                targetACount = targetJob.def.joyMaxParticipants;
            }
            */
            
            if (!targetJob.targetQueueA.NullOrEmpty())
            {
                jobDriver.pawn.ReserveAsManyAsPossible(targetJob.targetQueueA, jobDriver.job, 1, -1, null);
            }
            if (!targetJob.targetQueueB.NullOrEmpty())
            {
                jobDriver.pawn.ReserveAsManyAsPossible(targetJob.targetQueueB, jobDriver.job, 1, -1, null);
            }

            if (jobDriver.job.targetA != null)
            {
                result &= jobDriver.pawn.Reserve(jobDriver.job.targetA, jobDriver.job, 1, -1, null);
            }
            if (targetJob.targetA != null)
            {
                result &= jobDriver.pawn.Reserve(targetJob.targetA, jobDriver.job, targetACount, targetACount > 1 ? 0 : - 1, null);
            }
            if (targetJob.targetB != null)
            {
                result &= jobDriver.pawn.Reserve(targetJob.targetB, jobDriver.job, targetBCount, targetBCount > 1 ? 0 : -1, null);
            }
            if (targetJob.targetC != null)
            {
                result &= jobDriver.pawn.Reserve(targetJob.targetC, jobDriver.job, 1, -1, null);
            }
            return result;
        }
    }
}
