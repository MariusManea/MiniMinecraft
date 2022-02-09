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

    public ItemSlot GetFirstArrowSlot()
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.HasItem && slot.stack.ID == (byte)ItemID.ARROW)
            {
                return slot;
            }
        }

        return null;
    }

    public void emptyInventory()
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.HasItem)
            {
                ItemStack items = slot.TakeAll();
                for (int i = 0; i < items.amount; ++i)
                {
                    Item blockItem = GameObject.Instantiate(World.Instance.itemTypes[items.ID], world._player.transform.position + new Vector3(0.5f, 0.5f, 0.5f), new Quaternion());
                    blockItem.verticalMomentum = Random.Range(2f, 6f);
                    blockItem.horizontal = Random.Range(-1.0f, 1.0f);
                    blockItem.vertical = Random.Range(-1.0f, 1.0f);
                }
            }
        }
    }
}
