using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace GiddyUpRideAndRoll.Jobs
{
    //Normal GoTo job won't suffice for impassible targets. Also, no need to exit map using this job. 
    class JobDriver_RideToJob : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            this.pawn.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            yield return gotoCell;
        }
    }
}
