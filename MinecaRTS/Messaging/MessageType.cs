﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public enum MessageType
    {
        ResourceDepleted,
        ResourcesReceived,
        GiveMeResources,
        UnitSpawned,
        UnitMoved,
        SearchComplete,
        ProductionBuildingTaskComplete,
        SupplyChanged,
        BuildingComplete,
    }
}
