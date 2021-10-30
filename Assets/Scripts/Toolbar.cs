using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar : MonoBehaviour
{
    public UIItemSlot[] uiItemSlots;

    public RectTransform highlight;
    public int slotIndex = 0;
    public Player player;

    public void Start()
    {
        Debug.Log("toolbar start");
        byte index = 2;
        foreach (UIItemSlot s in uiItemSlots)
        {
            ItemStack stack = new ItemStack(index, Random.Range(2, 65));
            ItemSlot slot = new ItemSlot(s, stack);
            index++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0) slotIndex--;
            if (scroll < 0) slotIndex++;

            if (slotIndex > uiItemSlots.Length - 1) slotIndex = 0;
            if (slotIndex < 0) slotIndex = uiItemSlots.Length - 1;

            highlight.position = uiItemSlots[slotIndex].slotIcon.transform.position;
        }
    }
}
