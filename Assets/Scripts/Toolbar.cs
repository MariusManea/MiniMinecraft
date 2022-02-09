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
        foreach (UIItemSlot s in uiItemSlots)
        {
            ItemSlot emptySlot = new ItemSlot(s);
        }
        new ItemSlot(uiItemSlots[0], new ItemStack((byte)ItemID.BOW, 1));
        new ItemSlot(uiItemSlots[1], new ItemStack((byte)ItemID.ARROW, 64));
        new ItemSlot(uiItemSlots[2], new ItemStack((byte)ItemID.BOSS_SUMMONER, 1));
        new ItemSlot(uiItemSlots[3], new ItemStack((byte)ItemID.DIAMOND_PICKAXE, 1));
        new ItemSlot(uiItemSlots[4], new ItemStack((byte)ItemID.OAK_PLANKS, 64));
        new ItemSlot(uiItemSlots[5], new ItemStack((byte)ItemID.DIAMOND, 64));
        new ItemSlot(uiItemSlots[6], new ItemStack((byte)ItemID.IRON_INGOT, 64));
        new ItemSlot(uiItemSlots[7], new ItemStack((byte)ItemID.DIAMOND_AXE, 1));
        new ItemSlot(uiItemSlots[8], new ItemStack((byte)ItemID.POTATO, 64));
    }

    private void Update()
    {
        if (player.equippedItem != null)
        {
            if (uiItemSlots[slotIndex].HasItem)
            {
                if ((byte)player.equippedItem.itemID != uiItemSlots[slotIndex].itemSlot.stack.ID)
                {
                    ChangeItemInHand(World.Instance.itemTypes[uiItemSlots[slotIndex].itemSlot.stack.ID]);
                }
            }
            else
            {
                ChangeItemInHand(null);
            }
        }
        else
        {
            if (uiItemSlots[slotIndex].HasItem)
            {
                ChangeItemInHand(World.Instance.itemTypes[uiItemSlots[slotIndex].itemSlot.stack.ID]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeSlot(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeSlot(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeSlot(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ChangeSlot(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ChangeSlot(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ChangeSlot(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            ChangeSlot(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ChangeSlot(8);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0) slotIndex--;
            if (scroll < 0) slotIndex++;

            if (slotIndex > uiItemSlots.Length - 1) slotIndex = 0;
            if (slotIndex < 0) slotIndex = uiItemSlots.Length - 1;

            highlight.position = uiItemSlots[slotIndex].slotIcon.transform.position;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (uiItemSlots[slotIndex].HasItem)
            {
                Item item = GameObject.Instantiate(World.Instance.itemTypes[uiItemSlots[slotIndex].itemSlot.stack.ID], player.transform.position + new Vector3(0, player.entityHeight, 0), player.transform.rotation);
                item.vertical = 1;
                item.verticalMomentum = player.jumpForce;
                uiItemSlots[slotIndex].itemSlot.Take(1);
            }
        }
    }

    private void ChangeSlot(int _index)
    {
        slotIndex = _index;
        highlight.position = uiItemSlots[slotIndex].slotIcon.transform.position;
    }

    public ItemSlot GetFirstAvailableSlot(byte _id)
    {
        ItemSlot firstEmptySlot = null;

        foreach(UIItemSlot slot in uiItemSlots)
        {
            if (!slot.HasItem && firstEmptySlot == null) firstEmptySlot = slot.itemSlot;

            if (slot.HasItem)
            {
                if (slot.itemSlot != null && slot.itemSlot.stack.ID == _id && slot.itemSlot.stack.amount < World.Instance.itemTypes[slot.itemSlot.stack.ID].maxItemStack)
                {
                    return slot.itemSlot;
                }
            }
        }

        return firstEmptySlot;
    }

    public ItemSlot GetFirstArrowSlot()
    {
        foreach(UIItemSlot slot in uiItemSlots)
        {
            if (slot.HasItem && slot.itemSlot.stack.ID == (byte)ItemID.ARROW)
            {
                return slot.itemSlot;
            } 
        }

        return null;
    }

    public void emptyInventory()
    {
        foreach (UIItemSlot slot in uiItemSlots)
        {
            if (slot.HasItem)
            {
                ItemStack items = slot.itemSlot.TakeAll();
                for (int i = 0; i < items.amount; ++i)
                {
                    Item blockItem = GameObject.Instantiate(World.Instance.itemTypes[items.ID], player.transform.position + new Vector3(0.5f, 0.5f, 0.5f), new Quaternion());
                    blockItem.verticalMomentum = Random.Range(2f, 6f);
                    blockItem.horizontal = Random.Range(-1.0f, 1.0f);
                    blockItem.vertical = Random.Range(-1.0f, 1.0f);
                }
            }
        }
    }

    public void UseItemInHand()
    {
        uiItemSlots[slotIndex].itemSlot.Take(1);
    }

    public void ChangeItemInHand(Item _item)
    {
        player.EquipItem(_item);
    }
}
