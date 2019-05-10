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
            Toil goToToil = null;
            if (this.job.targetB.Thing is Pawn)
            {
                goToToil = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            }
            else
            {
                if(Scribe.mode != LoadSaveMode.PostLoadInit)
                {
                    dest = RCellFinder.RandomWanderDestFor(pawn, TargetB.Cell, 8, ((Pawn p, IntVec3 loc, IntVec3 root) => true), Danger.Some);
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
        public virtual void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref dest, "dest", TargetB.Cell);
        }



    }
}
