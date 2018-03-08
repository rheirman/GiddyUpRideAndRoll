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
            PawnTableDef animalsTable = PawnTableDefOf.Animals;
            foreach (PawnColumnDef def in from td in DefDatabase<PawnColumnDef>.AllDefsListForReading
                                          orderby td.index descending
                                          select td)
            {
                if(def.defName == "MountableByMaster" || def.defName == "MountableByAnyone")
                {
                    animalsTable.columns.Add(def);
                }
                Log.Message("defname found: " + def.defName);
            }
            Log.Message("defsloaded");
        }

        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return GiddyUpCore.Base.Instance.GetExtendedDataStorage();
        }

    }


}
