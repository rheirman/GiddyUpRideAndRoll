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
        IntVec3 dest;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
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
                return true;
            }
            result = ReserveUtility.ReserveEveryThingOfJob(pawnData.targetJob, this);
            pawnData.targetJob = null;
            return result;
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil goToToil = goToToil = Toils_Goto.GotoCell(TargetB.Cell, PathEndMode.Touch);

            this.AddFinishAction(delegate
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(this.pawn);
                pawnData.wasRidingToJob = true;
            });
            yield return goToToil;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref dest, "dest", TargetB.Cell);
        }



    }
}
