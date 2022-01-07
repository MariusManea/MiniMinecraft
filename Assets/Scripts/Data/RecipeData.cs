using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "MiniMinecraft/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public ItemID craftResult;
    public int craftAmount;
    public int rows;
    public int columns;
    public RecipeRowData[] rowData;
}

[System.Serializable]
public class RecipeRowData
{
    public ItemID[] columnData;
}
