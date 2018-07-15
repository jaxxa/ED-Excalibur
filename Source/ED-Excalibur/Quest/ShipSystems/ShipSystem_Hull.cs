﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnhancedDevelopment.Excalibur.Quest.ShipSystems
{
    class ShipSystem_Hull : ShipSystem
    {
        public override string Name()
        {
            return "Hull";
        }

        public override int PowerForRepair()
        {
            return 1000;
        }

        public override int ResourceUnitsForRepair()
        {
            return 10;
        }
    }
}
