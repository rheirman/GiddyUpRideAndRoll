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
    class JobDriver_WaitForRider : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private Pawn Followee {
            get
            {
                return (Pawn)TargetA;
            }
        }
        int moveInterval = Rand.Range(300, 1200);
        private JobDef initialJob;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Message("MakeNewToils called");
            initialJob = Followee.CurJobDef;
            Toil toil = new Toil
            {
                tickAction = delegate
                {
                    if (this.Followee.jobs.curJob.def == JobDefOf.LayDown ||
                       this.Followee.jobs.curJob.def == JobDefOf.Research ||
                       this.Followee.InMentalState ||
                       this.Followee.Dead ||
                       this.Followee.Downed ||
                       pawn.health.HasHediffsNeedingTend() ||
                       (pawn.needs.food != null && pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry) ||
                       pawn.needs.rest != null && pawn.needs.rest.CurCategory >= RestCategory.VeryTired ||
                       (this.Followee.GetRoom() != null && !(this.Followee.GetRoom().Role == GU_RR_DefOf.Barn || this.Followee.GetRoom().Role == RoomRoleDefOf.None)))//Don't allow animals to follow pawns inside
                    {
                        this.EndJobWith(JobCondition.Incompletable);
                    }   
                    
                    if (pawn.IsHashIntervalTick(moveInterval) && !this.pawn.pather.Moving)
                    {
                        IntVec3 target = RCellFinder.RandomWanderDestFor(Followee, this.Followee.Position, 8, ((Pawn p, IntVec3 loc, IntVec3 root) => true), Danger.Some);
                        this.pawn.pather.StartPath(target, PathEndMode.Touch);
                        moveInterval = Rand.Range(300, 1200);
                    }
                    if(TimeUntilExpire(pawn.CurJob) < 10 && Followee.CurJobDef == initialJob)
                    {
                        pawn.CurJob.expiryInterval += 1000;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            toil.AddFinishAction(() =>
            {
                if(Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
                {
                    ExtendedPawnData animalData = store.GetExtendedDataFor(pawn);
                    ExtendedPawnData pawnData = store.GetExtendedDataFor(Followee);
                    pawnData.owning = null;
                    animalData.ownedBy = null;
                }
            });
            yield return toil;
        }
        
        private int TimeUntilExpire(Job job)
        {
            return job.expiryInterval - (Find.TickManager.TicksGame - job.startTick);
        }
    }
}
