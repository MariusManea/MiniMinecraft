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

        for (int i = 1; i < world.blockTypes.Length; ++i)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);
            ItemStack stack = new ItemStack((byte)i, 64);
            slots.Add(new ItemSlot(newSlot.GetComponent<UIItemSlot>(), stack));
            slots[i - 1].isCreative = true;
        }
    }
}
