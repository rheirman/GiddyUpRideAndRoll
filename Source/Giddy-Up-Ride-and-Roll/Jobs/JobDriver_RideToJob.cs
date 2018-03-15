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
            int stackCount = -1;
            if(job.count > 1)
            {
                stackCount = 0;
            }
            bool result = true;
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();

            if(store != null )
            {
                LocalTargetInfo extraTarget = store.GetExtendedDataFor(this.pawn).extraJobTarget;
                if(extraTarget != null)
                {
                    result = this.pawn.Reserve(extraTarget, this.job, 1, -1, null);
                }
            }
            if (!this.job.targetQueueA.NullOrEmpty())
            {
                this.pawn.ReserveAsManyAsPossible(this.job.targetQueueA, this.job, 1, -1, null);
            }
            if (!this.job.targetQueueB.NullOrEmpty())
            {
                this.pawn.ReserveAsManyAsPossible(this.job.targetQueueB, this.job, 1, -1, null);
            }
            if (this.job.targetB != null && this.job.targetC != null)
            {
                return result && this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && 
                    this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null) && 
                    this.pawn.Reserve(this.job.GetTarget(TargetIndex.C), this.job, 1, -1, null);
            }
            else if (this.job.targetB != null)
            {
                return result && this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, this.job.count, stackCount, null);
            }
            return result;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            IntVec3 dest;
            bool cellFound = CellFinder.TryFindRandomReachableCellNear(this.job.targetB.Cell, Map, 2, TraverseMode.PassDoors, (IntVec3 c) => c.Standable(pawn.Map), null, out dest);
            if (!cellFound)
            {
                dest = this.job.targetB.Cell;
            }

            Toil gotoCell = Toils_Goto.GotoCell(dest, PathEndMode.Touch);
            this.AddFinishAction(delegate
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(this.pawn);
                pawnData.wasRidingToJob = true;
            });
            yield return gotoCell;
        }
        
    }
}
