using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Utils;
using Verse;
using UnityEngine;
using HugsLib.Settings;
using GiddyUpCore.Storage;
using RimWorld;
using System.Reflection;
using Harmony;

namespace GiddyUpRideAndRoll
{
    public class Base : ModBase
    {
        internal static Base Instance { get; private set; }
        internal const string NOMOUNT_LABEL = "Gu_Area_NoMount";
        internal const string DROPANIMAL_LABEL = "Gu_Area_DropMount";
        internal static SettingHandle<int> minAutoMountDistance;
        internal static SettingHandle<bool> noMountedHunting;
        internal static SettingHandle<int> minAutoMountDistanceFromAnimal;

        public override string ModIdentifier
        {
            get { return "GiddyUpRideAndRoll"; }
        }
        public Base()
        {
            Instance = this;
        }
        public override void DefsLoaded()
        {

            bool defaultNoMountedHunting = AssemblyExists("CombatExtended");
            Log.Message("Combat extended loaded: " + defaultNoMountedHunting);
            minAutoMountDistance = Settings.GetHandle<int>("minAutoMountDistance", "GU_RR_MinAutoMountDistance_Title".Translate(), "GU_RR_MinAutoMountDistance_Description".Translate(), 16, Validators.IntRangeValidator(0, 500));
            noMountedHunting = Settings.GetHandle<bool>("noMountedHunting", "GU_RR_NoMountedHunting_Title".Translate(), "GU_RR_NoMountedHunting_Description".Translate(), defaultNoMountedHunting);

            minAutoMountDistanceFromAnimal = Settings.GetHandle<int>("minAutoMountDistanceFromAnimal", "GU_RR_MinAutoMountDistanceFromAnimal_Title".Translate(), "GU_RR_MinAutoMountDistanceFromAnimal_Description".Translate(), 12, Validators.IntRangeValidator(0, 500));

            PawnTableDef animalsTable = PawnTableDefOf.Animals;
            foreach (PawnColumnDef def in from td in DefDatabase<PawnColumnDef>.AllDefsListForReading
                                          orderby td.index descending
                                          select td)
            {
                if(def.defName == "MountableByMaster" || def.defName == "MountableByAnyone")
                {
                    animalsTable.columns.Add(def);
                }
            }
        }

        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return GiddyUpCore.Base.Instance.GetExtendedDataStorage();
        }

        private bool AssemblyExists(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith(assemblyName))
                    return true;
            }
            return false;
        }

    }


}
