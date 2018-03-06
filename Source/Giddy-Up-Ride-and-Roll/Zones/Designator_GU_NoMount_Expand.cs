using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using GiddyUpCore.Zones;

namespace GiddyUpRideAndRoll.Zones
{
    class Designator_GU_NoMount_Expand : Designator_GU
    {

        public Designator_GU_NoMount_Expand() : base(DesignateMode.Add)
        {
            defaultLabel = "GU_RR_Designator_GU_NoMount_Expand_Label".Translate();
            defaultDesc = "GU_RR_Designator_GU_NoMount_Expand_Description".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/GU_RR_Designator_GU_NoMount_Expand", true);
            areaLabel = Base.NOMOUNT_LABEL;
        }

        //public override AcceptanceReport CanDesignateCell(IntVec3 c)
        //{
        //    return c.InBounds(base.Map) && Designator_Stable.SelectedArea != null && Designator_Stable.SelectedArea[c];
        //}
        public override void DesignateSingleCell(IntVec3 c)
        {
            selectedArea[c] = true;
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.InBounds(base.Map) && selectedArea != null && !selectedArea[c];
        }
        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }
        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }


    }
}
