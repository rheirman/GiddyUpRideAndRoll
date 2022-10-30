using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Multiplayer.API;
using Verse;
using RimWorld;
using HarmonyLib;

namespace GiddyUpRideAndRoll.ModExtensions
{
    [StaticConstructorOnStartup]
    public static class MultiplayerPatch
    {
        //static Type AreaGUType;

        static FieldInfo AreaLabelField;
        //static FieldInfo AreaManagerAreasField;
        static MethodInfo SetSelectedAreaMethod;

        static Type DesignatorDropAnimalExpandType;
        static Type DesignatorDropAnimalClearType;
        static Type DesignatorNoMountExpandType;
        static Type DesignatorNoMountClearType;
        static Type DesignatorNPCDropAnimalExpandType;
        static Type DesignatorNPCDropAnimalClearType;

        static MultiplayerPatch()
        {
            if (MP.enabled)
            {
                Type type = AccessTools.TypeByName("GiddyUpCore.Zones.Designator_GU");
                MP.RegisterAll();

                AreaLabelField = AccessTools.Field(type, "areaLabel");
                SetSelectedAreaMethod = AccessTools.Method(type, "setSelectedArea");

                MP.RegisterSyncWorker<Designator>(DesignatorGU, type, true);

                DesignatorDropAnimalExpandType = AccessTools.TypeByName("GiddyUpRideAndRoll.Zones.Designator_GU_DropAnimal_Expand");
                DesignatorDropAnimalClearType = AccessTools.TypeByName("GiddyUpRideAndRoll.Zones.Designator_GU_DropAnimal_Clear");
                DesignatorNoMountExpandType = AccessTools.TypeByName("GiddyUpRideAndRoll.Zones.Designator_GU_NoMount_Expand");
                DesignatorNoMountClearType = AccessTools.TypeByName("GiddyUpRideAndRoll.Zones.Designator_GU_NoMount_Clear");
                DesignatorNPCDropAnimalExpandType = AccessTools.TypeByName("GiddyUpCaravan.Zones.Designator_GU_DropAnimal_NPC_Expand");
                DesignatorNPCDropAnimalClearType = AccessTools.TypeByName("GiddyUpCaravan.Zones.Designator_GU_DropAnimal_NPC_Clear");

            }
        }

        static void DesignatorGU(SyncWorker sync, ref Designator designator)
        {
            if (sync.isWriting)
            {
                Type t = designator.GetType();
                byte tByte = ByteFromType(t);

                sync.Write(tByte);

            }
            else
            {
                byte tByte = sync.Read<byte>();
                Type t = TypeFromByte(tByte);

                designator = (Designator)Activator.CreateInstance(t);

                string label = (string)AreaLabelField.GetValue(designator);

                SetSelectedAreaMethod.Invoke(designator, new object[] { label });
            }
        }
        static Type TypeFromByte(byte tByte)
        {
            switch (tByte)
            {
                case 1: return DesignatorDropAnimalExpandType;
                case 2: return DesignatorDropAnimalClearType;
                case 3: return DesignatorNoMountExpandType;
                case 4: return DesignatorNoMountClearType;
                case 5: return DesignatorNPCDropAnimalExpandType;
                case 6: return DesignatorNPCDropAnimalClearType;
                default: throw new Exception("Unknown Designator for GiddyUP");
            }
        }
        static byte ByteFromType(Type type)
        {
            if (type == DesignatorDropAnimalExpandType)
            {
                return 1;
            }
            if (type == DesignatorDropAnimalClearType)
            {
                return 2;
            }
            if (type == DesignatorNoMountExpandType)
            {
                return 3;
            }
            if (type == DesignatorNoMountClearType)
            {
                return 4;
            }
            if (type == DesignatorNPCDropAnimalExpandType)
            {
                return 5;
            }
            if (type == DesignatorNPCDropAnimalClearType)
            {
                return 6;
            }
            throw new Exception("Unknown Designator for GiddyUP");
        }

    }
}
