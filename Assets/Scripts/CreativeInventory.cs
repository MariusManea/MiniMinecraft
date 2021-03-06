using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    World world;

    List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        world = FindObjectOfType<World>();

        for (int i = 0; i < world.itemTypes.Length; ++i)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);
            ItemStack stack = new ItemStack((byte)i, world.itemTypes[i].maxItemStack);
            slots.Add(new ItemSlot(newSlot.GetComponent<UIItemSlot>(), stack));
            slots[i].isCreative = true;
        }
    }
}
