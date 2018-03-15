using GiddyUpCore.Storage;
using GiddyUpRideAndRoll.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Jobs
{
    //Normal GoTo job won't suffice for impassible targets. Also, no need to exit map using this job. 
    class JobDriver_RideToJob : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            //For automatic mounting, reserve the mount aswell as targets of the job the pawn is riding to (target B and possibly C). 

            bool result = true;

            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store == null)
            {
                return true;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(this.pawn);
            if (pawnData.targetJob == null)
            {
                Log.Message("pawnData.targetJob was null for " + this.pawn.Name);
                return true;
            }
            Log.Message("JobDriver_RideToJob calling ReserveEveryThingOfJob, pawn" + this.pawn);
            result = ReserveUtility.ReserveEveryThingOfJob(pawnData.targetJob, this);
            Log.Message("JobDriver_RideToJob calling ReserveEveryThingOfJob, result: " + result);
            this.job.targetB = pawnData.targetJob.targetA;
            pawnData.targetJob = null;
            return result;
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            IntVec3 dest;
            Toil goToToil = null;

            if (this.job.targetB.Thing is Pawn)
            {
                goToToil = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            }
            else
            {
                bool cellFound = CellFinder.TryFindRandomReachableCellNear(this.job.targetB.Cell, Map, 2, TraverseMode.PassAllDestroyableThings, (IntVec3 c) => (c.Standable(pawn.Map)), null, out dest);
                if (!cellFound)
                {
                    dest = this.job.targetB.Cell;
                }
                goToToil = Toils_Goto.GotoCell(dest, PathEndMode.Touch);
            }

            this.AddFinishAction(delegate
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(this.pawn);
                pawnData.wasRidingToJob = true;
            });
            yield return goToToil;
        }

        
    }
}
