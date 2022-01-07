using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    World world;

    List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        world = FindObjectOfType<World>();

        for (int i = 0; i < 27; ++i)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);
            slots.Add(new ItemSlot(newSlot.GetComponent<UIItemSlot>()));
            slots[i].isCreative = false;
        }
    }

    public ItemSlot GetFirstAvailableSlot(byte _id)
    {
        ItemSlot firstEmptySlot = null;
        foreach(ItemSlot slot in slots)
        {
            if (!slot.HasItem && firstEmptySlot == null) firstEmptySlot = slot;

            if (slot.HasItem)
            {
                if (slot.stack.ID == _id && slot.stack.amount < World.Instance.itemTypes[slot.stack.ID].maxItemStack)
                {
                    return slot;
                }
            }
        }

        return firstEmptySlot;
    }
}
