using GiddyUpCore.Storage;
using GiddyUpCore.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Multiplayer.API;

namespace GiddyUpRideAndRoll.PawnColumns
{
    class PawnColumnWorker_RR_Mountable_Anyone : PawnColumnWorker_Checkbox
    {
        public override bool HasCheckbox(Pawn pawn)
        {
            return IsMountableUtility.isMountable(pawn);
        }
        public override bool GetValue(Pawn pawn)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            return pawnData.mountableByAnyone;
        }

        [SyncMethod]
        public override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.mountableByAnyone = value;
        }
    }
}
