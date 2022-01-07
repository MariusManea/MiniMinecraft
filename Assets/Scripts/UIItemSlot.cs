using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;

    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;

    public World world;

    public bool lockPlace;

    private void Awake()
    {
        world = FindObjectOfType<World>();
    }

    public bool HasItem
    {
        get
        {
            if (itemSlot == null) return false;
            return itemSlot.HasItem;
        }
    }

    public void Link(ItemSlot _itemSLot)
    {
        itemSlot = _itemSLot;
        isLinked = true;
        itemSlot.LinkUISlot(this);

        UpdateSlot();
    }

    public void UnLink()
    {
        itemSlot.UnLinkUISlot();
        itemSlot = null;

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.itemTypes[itemSlot.stack.ID].icon;
            if (itemSlot.stack.amount > 1)
            {
                slotAmount.text = itemSlot.stack.amount.ToString();
            }
            else
            {
                slotAmount.text = "";
            }
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
        {
            Clear();
        }
        if (itemSlot.isCraftSlot)
        {
            RecipeCrafter recipeCrafter = GetComponentInParent<RecipeCrafter>();
            if (recipeCrafter)
            {
                recipeCrafter.requestCheckRecipe = true;
            }
        }
        if (itemSlot.isCraftResult)
        {
            RecipeCrafter recipeCrafter = transform.parent.parent.GetComponentInChildren<RecipeCrafter>();
            if (recipeCrafter)
            {
                recipeCrafter.requestCheckRecipe = true;
            }
        }
    }

   

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if (isLinked)
        {
            itemSlot.UnLinkUISlot();
        }
    }

}


public class ItemSlot
{
    public ItemStack stack = null;
    private UIItemSlot uiItemSlot = null;
    public bool isCreative;
    public bool isCraftSlot;
    public bool isCraftResult;

    private VoxelState container;
    private int index;

    public ItemSlot(UIItemSlot _uiItemSlot, ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlot _uiItemSlot)
    {
        stack = null;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
        
    }

    public void LinkUISlot(UIItemSlot _uIItemSlot)
    {
        uiItemSlot = _uIItemSlot;
    }
    public void UnLinkUISlot()
    {
        uiItemSlot = null;
    }

    public bool IsContainerLinked()
    {
        return container != null;
    }

    public VoxelState GetContainer()
    {
        return container;
    }

    public int GetIndex()
    {
        return index;
    }

    public void EmptySlot()
    {
        stack = null;
        if (container != null && container.itemsContained != null && index >= 0 && index < container.itemsContained.Length)
        {
            container.itemsContained[index] = null;
            if (World.Instance.blockTypes[container.id].itemID == ItemID.FURNANCE)
            {
                container.chunkData.chunk.AddActiveVoxel(container);
            }
        }
        if (uiItemSlot != null)
        {
            uiItemSlot.UpdateSlot();
        }
    }

    public void UnlinkContainer()
    {
        container = null;
        index = -1;
    }

    public int Take(int _amount)
    {
        if (_amount > stack.amount)
        {
            int amt = stack.amount;
            EmptySlot();
            return amt;
        } 
        else
        {
            if (_amount < stack.amount)
            {
                stack.amount -= _amount;
                if (container != null && container.itemsContained != null && index >= 0 && index < container.itemsContained.Length)
                {
                    container.itemsContained[index].amount -= _amount;
                    if (World.Instance.blockTypes[container.id].itemID == ItemID.FURNANCE)
                    {
                        container.chunkData.chunk.AddActiveVoxel(container);
                    }
                }
                uiItemSlot.UpdateSlot();
                return _amount;
            }
            else
            {
                EmptySlot();
                return _amount;
            }
        }
    }

    public int Add(int _amount)
    {
        int newAmount = _amount + stack.amount;
        int maxItemStack = uiItemSlot.world.itemTypes[stack.ID].maxItemStack;

        if (newAmount > maxItemStack) return maxItemStack;
        stack.amount = newAmount;
        if (container != null && container.itemsContained != null && index >= 0 && index < container.itemsContained.Length)
        {
            container.itemsContained[index].amount = newAmount;
            if (World.Instance.blockTypes[container.id].itemID == ItemID.FURNANCE)
            {
                container.chunkData.chunk.AddActiveVoxel(container);
            }
        }
        uiItemSlot.UpdateSlot();
        return newAmount;

    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.ID, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStackAndLinkToContainer(ItemStack _stack, VoxelState _container, int _index)
    {
        if (_stack != null && _stack.amount > 0) InsertStack(_stack);
        else EmptySlot();
        container = _container;
        index = _index;
    }

    public void InsertStack(ItemStack _stack)
    {
        stack = _stack;
        if (container != null && container.itemsContained != null && index >= 0 && index < container.itemsContained.Length)
        {
            container.itemsContained[index] = new ItemStack(_stack);
            if (World.Instance.blockTypes[container.id].itemID == ItemID.FURNANCE)
            {
                container.chunkData.chunk.AddActiveVoxel(container);
            }
        }
        uiItemSlot.UpdateSlot();
    }


    public bool HasItem
    {
        get
        {
            if (stack != null) return true;
            return false;
        }
    }
}