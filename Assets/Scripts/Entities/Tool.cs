using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
    public ToolQuality toolQuality;
    public ToolType toolType;

}

[System.Serializable]
public enum ToolQuality
{
    WOOD = 0,
    STONE = 1,
    IRON = 2,
    DIAMOND = 3,
    NON_DROP = 4,
}

[System.Serializable]
public enum ToolType
{
    HAND = 0,
    PICKAXE = 1,
    AXE = 2,
    NON_DROP = 3,
    HOE = 4,
}
