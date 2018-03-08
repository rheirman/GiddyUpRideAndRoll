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
    class PawnColumnWorker_RR_Mountable_Anyone : PawnColumnWorker_Checkbox
    {
        protected override bool HasCheckbox(Pawn pawn)
        {
            return IsMountableUtility.isMountable(pawn);
        }

        protected override bool GetValue(Pawn pawn)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            return pawnData.mountableByAnyone;
        }

        protected override void SetValue(Pawn pawn, bool value)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.mountableByAnyone = value;
        }
    }
}
