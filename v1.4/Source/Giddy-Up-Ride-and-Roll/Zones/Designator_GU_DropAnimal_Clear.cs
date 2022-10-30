using GiddyUpCore.Zones;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace GiddyUpRideAndRoll.Zones
{
    class Designator_GU_DropAnimal_Clear : Designator_GU
    {
        public Designator_GU_DropAnimal_Clear() : base(DesignateMode.Remove)
        {
            defaultLabel = "GU_RR_Designator_GU_DropAnimal_Clear_Label".Translate();
            defaultDesc = "GU_RR_Designator_GU_DropAnimal_Clear_Description".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/GU_RR_Designator_GU_DropAnimal_Clear", true);
            areaLabel = Base.DROPANIMAL_LABEL;
        }
        public override void DesignateSingleCell(IntVec3 c)
        {
            selectedArea[c] = false;
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.InBounds(base.Map) && selectedArea != null && selectedArea[c];
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
