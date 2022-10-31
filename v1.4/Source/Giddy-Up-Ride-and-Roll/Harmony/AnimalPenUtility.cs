using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GiddyUpRideAndRoll.Harmony
{
    [HarmonyPatch(typeof(AnimalPenUtility), "GetPenAnimalShouldBeTakenTo")]
    class CompAnimalPenMarker_GetPenAnimalShouldBeTakenTo
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand as MethodInfo == typeof(AnimalPenUtility).GetMethod("NeedsToBeManagedByRope"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(CompAnimalPenMarker_GetPenAnimalShouldBeTakenTo).GetMethod("NeedsToBeManagedByRopeModified"));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        public static bool NeedsToBeManagedByRopeModified(Pawn animal)
        {
            if (IsMountableUtility.IsCurrentlyMounted(animal))
            {
                return true;
            }
            else
            {
                return AnimalPenUtility.NeedsToBeManagedByRope(animal);
            }
        }
    }
}
