using GiddyUpCore.Storage;
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
            if (job.count == -1)
            {
                job.count = 1;
            }
            if (!this.job.targetQueueA.NullOrEmpty())
            {
                this.pawn.ReserveAsManyAsPossible(this.job.targetQueueA, this.job, 1, -1, null);
            }
            if (!this.job.targetQueueB.NullOrEmpty())
            {
                this.pawn.ReserveAsManyAsPossible(this.job.targetQueueA, this.job, 1, -1, null);
            }
            if (this.job.targetB != null && this.job.targetC != null)
            {
                return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.C), this.job, 1, -1, null);
            }
            else if (this.job.targetB != null)
            {
                return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, this.job.count, -1, null);
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            IntVec3 dest = CellFinder.RandomClosewalkCellNear(this.job.targetB.Cell, Map, 5);
            Toil gotoCell = Toils_Goto.GotoCell(dest, PathEndMode.OnCell);
            this.AddFinishAction(delegate
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(this.pawn);
                pawnData.wasRidingToJob = true;
            });
            yield return gotoCell;
        }
        
    }
}
