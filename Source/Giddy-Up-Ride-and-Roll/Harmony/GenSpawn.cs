using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(GenSpawn), "Spawn")]
    [HarmonyPatch(new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
    class GenSpawn_Spawn
    {
        //Make sure pawns arrive on the map mounted when they have a caravan mount.
        static void Postfix(ref Thing newThing, bool respawningAfterLoad)
        {
            if(newThing is Pawn pawn && pawn.IsColonist && !respawningAfterLoad && Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);
                if (pawnData.caravanMount is Pawn animal)
                {
                    ExtendedPawnData animalData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(animal);
                    pawnData.mount = animal;
                    TextureUtility.setDrawOffset(pawnData);
                    Job jobAnimal = new Job(GUC_JobDefOf.Mounted, pawn);
                    jobAnimal.count = 1;
                    animal.jobs.TryTakeOrderedJob(jobAnimal);
                }
            }
        }
    }
}
