﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ILogicOwnerInfo
    {
        LogicWorld GetLogicWorld();
        FixPoint GetCurrentTime();
        int GetOwnerObjectID();
        Object GetOwnerObject();
        int GetOwnerPlayerID();
        Player GetOwnerPlayer();
        int GetOwnerEntityID();
        Entity GetOwnerEntity();
    }
}