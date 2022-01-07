using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack 
{
    public byte ID;
    public int amount;


    public ItemStack()
    {
        ID = 0;
        amount = 0;
    }
    public ItemStack(byte _id, int _amount)
    {
        ID = _id;
        amount = _amount;
    }

    public ItemStack(ItemStack _other)
    {
        if (_other != null)
        {
            ID = _other.ID;
            amount = _other.amount;
        }
    }
}
