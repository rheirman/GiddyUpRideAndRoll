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
            if (this.job.targetA != null && this.job.targetB != null)
            {
                return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null);
            }
            else if (this.job.targetA != null)
            {
                Log.Message("targetAqueue count : + " + this.job.targetQueueA.Count);
                return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, this.job.count, -1, null);
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return reserveTargetA;
            if(this.job.targetB != null)
            {
                Toil reserveTargetB = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
                yield return reserveTargetB;
            }

            Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            this.AddFinishAction(delegate
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(this.pawn);
                pawnData.wasRidingToJob = true;
                ;
            });
            yield return gotoCell;
        }
        
    }
}
