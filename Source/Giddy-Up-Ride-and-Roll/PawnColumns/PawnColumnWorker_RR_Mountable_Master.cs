using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace GiddyUpRideAndRoll.PawnColumns
{
    class PawnColumnWorker_RR_Mountable_Master : PawnColumnWorker_Checkbox
    {
        protected override bool HasCheckbox(Pawn pawn)
        {
            return IsMountableUtility.isMountable(pawn) && pawn.playerSettings != null && pawn.playerSettings.Master != null;
        }

        protected override bool GetValue(Pawn pawn)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            return pawnData.mountableByMaster;
        }

        protected override void SetValue(Pawn pawn, bool value)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.mountableByMaster = value;
        }
       
    }
}
