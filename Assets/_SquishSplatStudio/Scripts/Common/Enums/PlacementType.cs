using System;

namespace SquishSplatStudio
{
    [System.Serializable]
    [Flags]
    public enum PlacementType
    {
        None            = 0,   //0b_0000_0000, // 0
        Agent           = 1,   //0b_0000_0001, // 1
        Structure       = 2,   //0b_0000_0010, // 2
        Waypoint        = 4,   //0b_0000_0100, // 4
        // SPECIFIC TYPE
        LightTower      = 8,   //0b_0000_1000, // 8
        LightWell       = 16,  //0b_0001_0000, // 16
        Purifier        = 32,  //0b_0010_0000, // 32
        ControlCrystal  = 64,  //0b_0100_0000, // 64
        DarkBeam        = 128, //0b_1000_0000, // 128
        LightBolt       = 256, //0b_1111_1100, // 256
        MineBeam        = 512,  //0b_1000_0000, // 512
        Shade           = 1024
    }
}