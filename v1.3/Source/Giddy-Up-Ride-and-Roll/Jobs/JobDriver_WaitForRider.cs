using GiddyUpCore.Jobs;
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

        public Pawn Followee {
            get
            {
                if(TargetA.Thing is Pawn pawn){

                    return pawn;
                }
                else
                {
                    return null;
                }
            }
        }
        int moveInterval = Rand.Range(300, 1200);
        private JobDef initialJob;

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => pawn.Map == null || this.Followee == null);
            initialJob = Followee.CurJobDef;
            Toil firstToil = new Toil {
                initAction = delegate
                {
                    WalkRandomNearby();
                }
            };
            firstToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return firstToil;
            Toil toil = new Toil
            {
                tickAction = delegate
                {
                    if (this.Followee.Map == null ||
                       this.Followee.Dead ||
                       this.Followee.Downed ||
                       this.Followee.InMentalState ||
                       this.Followee.jobs.curJob.def == JobDefOf.LayDown ||
                       this.Followee.jobs.curJob.def == JobDefOf.Research ||
                       this.Followee.CurJobDef == GUC_JobDefOf.Mount ||
                       pawn.health.HasHediffsNeedingTend() ||
                       (pawn.needs.food != null && pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry) ||
                       pawn.needs.rest != null && pawn.needs.rest.CurCategory >= RestCategory.VeryTired ||
                       (this.Followee.GetRoom() != null && !(this.Followee.GetRoom().Role == GU_RR_DefOf.Barn || this.Followee.GetRoom().Role == RoomRoleDefOf.None)))//Don't allow animals to follow pawns inside
                    {
                        this.EndJobWith(JobCondition.Succeeded);
                        return;
                    }   
                    
                    if (pawn.IsHashIntervalTick(moveInterval) && !this.pawn.pather.Moving)
                    {
                        WalkRandomNearby();
                        moveInterval = Rand.Range(300, 1200);
                    }
                    if (TimeUntilExpire(pawn.CurJob) < 10 && Followee.CurJobDef == initialJob)
                    {
                        pawn.CurJob.expiryInterval += 1000;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            toil.AddFinishAction(() =>
            {
                UnsetOwnership();
            });
            yield return toil;
        }

        private void UnsetOwnership()
        {
            if (Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
            {
                ExtendedPawnData animalData = store.GetExtendedDataFor(pawn);
                if (animalData.ownedBy != null)
                {
                    ExtendedPawnData riderData = store.GetExtendedDataFor(animalData.ownedBy);
                    riderData.owning = null;
                }
                animalData.ownedBy = null;
            }
        }

        private void WalkRandomNearby()
        {
            IntVec3 target = RCellFinder.RandomWanderDestFor(Followee, this.Followee.Position, 8, ((Pawn p, IntVec3 loc, IntVec3 root) => true), Danger.Some);
            this.pawn.pather.StartPath(target, PathEndMode.Touch);
        }

        private int TimeUntilExpire(Job job)
        {
            return job.expiryInterval - (Find.TickManager.TicksGame - job.startTick);
        }
    }
}
