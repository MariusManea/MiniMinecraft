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
            slotIcon.sprite = world.blockTypes[itemSlot.stack.ID].icon;
            slotAmount.text = itemSlot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
        {
            Clear();
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

    public void EmptySlot()
    {
        stack = null;
        if (uiItemSlot != null)
        {
            uiItemSlot.UpdateSlot();
        }
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
        int maxItemStack = uiItemSlot.world.blockTypes[stack.ID].maxItemStack;

        if (newAmount > maxItemStack) return maxItemStack;
        stack.amount = newAmount;
        uiItemSlot.UpdateSlot();
        return newAmount;

    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.ID, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStack(ItemStack _stack)
    {
        stack = _stack;
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