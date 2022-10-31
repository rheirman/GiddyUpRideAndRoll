using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GiddyUpRideAndRoll
{
    public static class Utitlities
    {
        public static bool HungryOrTired(this Pawn animal)
        {
            bool value = false;
            if (animal.needs != null && animal.needs.food != null && (animal.needs.food.CurCategory >= HungerCategory.UrgentlyHungry))
            { //animal needs break
                value = true;
            }
            if (animal.needs != null && animal.needs.rest != null && (animal.needs.rest.CurCategory >= RestCategory.VeryTired))
            {
                value = true;
            }

            return value;
        }
    }
}
