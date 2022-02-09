using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeCrafter : MonoBehaviour
{
    public int rows;
    public int columns;
    public bool requestCheckRecipe;

    public List<RecipeData> availableRecipes;
    public UIItemSlot[] slots;
    public UIItemSlot resultSlot;

    private int leftColumnWindow;
    private int rightColumnWindow;
    private int topRowWindow;
    private int bottomRowWindow;


    void Start()
    {
        resultSlot = transform.parent.GetChild(0).GetComponentInChildren<UIItemSlot>();

        if (rows < 3 && columns < 3)
        {
            availableRecipes = new List<RecipeData>();
            RecipeData[] recipes = World.Instance.recipes;
            foreach (RecipeData recipeData in recipes)
            {
                if (recipeData.rows < 3 && recipeData.columns < 3)
                {
                    availableRecipes.Add(recipeData);
                }
            }
        }
        else
        {
            availableRecipes = new List<RecipeData>(World.Instance.recipes);
        }

        slots = GetComponentsInChildren<UIItemSlot>();
    }

    void Update()
    {
        if (requestCheckRecipe)
        {
            requestCheckRecipe = false;
            ComputeWindowPositions();
            bool recipeFound = false;
            if (topRowWindow != -1 && bottomRowWindow != -1 && leftColumnWindow != -1 && rightColumnWindow != -1) {
                foreach (RecipeData recipe in availableRecipes)
                {
                    if (recipe.rows == (bottomRowWindow - topRowWindow + 1) && recipe.columns == (leftColumnWindow - rightColumnWindow + 1))
                    {
                        bool isGood = true;
                        for (int i = 0; i < recipe.rows && isGood; ++i)
                        {
                            for (int j = 0; j < recipe.columns && isGood; ++j)
                            {
                                if (!((recipe.rowData[i].columnData[j] == ItemID.NON_DROP_BLOCK && (!slots[(i + topRowWindow) * rows + (j + rightColumnWindow)].HasItem || slots[(i + topRowWindow) * rows + (j + rightColumnWindow)].itemSlot.stack.amount == 0)) ||
                                    (slots[(i + topRowWindow) * rows + (j + rightColumnWindow)].HasItem && (byte)recipe.rowData[i].columnData[j] == slots[(i + topRowWindow) * rows + (j + rightColumnWindow)].itemSlot.stack.ID)))
                                {
                                    isGood = false;
                                }
                            }
                        }
                        if (isGood)
                        {
                            recipeFound = true;
                            resultSlot.itemSlot.InsertStack(new ItemStack((byte)recipe.craftResult, recipe.craftAmount));
                           
                            break;
                        }
                    }
                    
                }
            }
            if (!recipeFound)
            {
                resultSlot.itemSlot.EmptySlot();
            }
        }
    }

    public void CraftObject()
    {
        foreach (UIItemSlot slot in slots)
        {
            if (slot.HasItem)
            {
                slot.itemSlot.Take(1);
            }
        }
    }

    private void ComputeWindowPositions()
    {
        leftColumnWindow =-1;
        rightColumnWindow = -1;
        topRowWindow = -1;
        bottomRowWindow = -1;

        for (int i = 0; i < rows && topRowWindow == -1; ++i)
            for (int j = 0; j < columns && topRowWindow == -1; ++j)
                if (slots[i * rows + j].HasItem && slots[i * rows + j].itemSlot.stack.amount > 0) topRowWindow = i;

        for (int i = rows - 1; i > - 1 && bottomRowWindow == -1; --i)
            for (int j = 0; j < columns && bottomRowWindow == -1; ++j)
                if (slots[i * rows + j].HasItem && slots[i * rows + j].itemSlot.stack.amount > 0) bottomRowWindow = i;

        for (int j = 0; j < columns && rightColumnWindow == -1; ++j)
            for (int i = 0; i < rows && rightColumnWindow == -1; ++i)
                if (slots[i * rows + j].HasItem && slots[i * rows + j].itemSlot.stack.amount > 0) rightColumnWindow = j;

        for (int j = columns - 1; j > -1 && leftColumnWindow == -1; --j)
            for (int i = 0; i < rows && leftColumnWindow == -1; ++i)
                if (slots[i * rows + j].HasItem && slots[i * rows + j].itemSlot.stack.amount > 0) leftColumnWindow = j;

    }
}
