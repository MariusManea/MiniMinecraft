using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster rayCaster = null;
    private PointerEventData pointerEventData;
    [SerializeField] private EventSystem eventSystem = null;

    World world;

    private void Start()
    {
        world = FindObjectOfType<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!world.inUI) return;

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleSlotRightClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null) return;

        if (!cursorSlot.HasItem && !clickedSlot.HasItem) return;

        if (clickedSlot.itemSlot.isCreative)
        {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }
        
        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            if (clickedSlot.itemSlot.isCraftResult)
            {
                if (World.Instance.handCraftInventoryMenu.activeSelf)
                {
                    World.Instance.handCraftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                }
                if (World.Instance.craftInventoryMenu.activeSelf)
                {
                    World.Instance.craftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                }
            }
            return;
        }

        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            if (clickedSlot.lockPlace) return;
            if (clickedSlot.itemSlot.IsContainerLinked())
            {
                VoxelState container = clickedSlot.itemSlot.GetContainer();
                if (container.properties.itemID == ItemID.FURNANCE)
                {
                    int index = clickedSlot.itemSlot.GetIndex();
                    if (index == 0)
                    {
                        if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isSmeltable) return;
                    }
                    if (index == 1)
                    {
                        if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isFuel) return;
                    }
                    if (index == 2) return;
                }
                if (container.properties.itemID == ItemID.CRAFTING_TABLE)
                {
                    int index = clickedSlot.itemSlot.GetIndex();
                    if (index == 0) return;
                }
            }
            clickedSlot.itemSlot.InsertStack(cursorSlot.itemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if (clickedSlot.itemSlot.isCraftResult)
            {
                if (cursorSlot.itemSlot.stack.ID == clickedSlot.itemSlot.stack.ID)
                {
                    int maxItemStack = world.itemTypes[cursorSlot.itemSlot.stack.ID].maxItemStack;

                    if (cursorSlot.itemSlot.stack.amount + clickedSlot.itemSlot.stack.amount <= maxItemStack)
                    {
                        int oldAmount = cursorSlot.itemSlot.stack.amount;
                        ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
                        ItemStack temporaryClickedSlot = clickedSlot.itemSlot.TakeAll();

                        int slotAmount = Mathf.Max(0, temporaryClickedSlot.amount - (maxItemStack - temporarySlot.amount));
                        int cursorAmount = Mathf.Min(maxItemStack, temporarySlot.amount + temporaryClickedSlot.amount);

                        if (slotAmount > 0)
                        {
                            clickedSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, slotAmount));
                        }
                        cursorSlot.itemSlot.InsertStack(new ItemStack(temporaryClickedSlot.ID, cursorAmount));

                        if (cursorAmount != oldAmount)
                        {
                            if (World.Instance.handCraftInventoryMenu.activeSelf)
                            {
                                World.Instance.handCraftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                            }
                            if (World.Instance.craftInventoryMenu.activeSelf)
                            {
                                World.Instance.craftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                            }
                        }
                    }
                }
            }
            else
            {
                bool normalBehaviour = true;
                if (clickedSlot.lockPlace) normalBehaviour = false;
                else
                {
                    if (clickedSlot.itemSlot.IsContainerLinked())
                    {
                        VoxelState container = clickedSlot.itemSlot.GetContainer();
                        if (container.properties.itemID == ItemID.FURNANCE)
                        {
                            int index = clickedSlot.itemSlot.GetIndex();
                            if (index == 0)
                            {
                                if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isSmeltable) return;
                            }
                            if (index == 1)
                            {
                                if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isFuel) return;
                            }
                            if (index == 2) normalBehaviour = false;
                        }
                        if (container.properties.itemID == ItemID.CRAFTING_TABLE)
                        {
                            int index = clickedSlot.itemSlot.GetIndex();
                            if (index == 0) normalBehaviour = false;
                        }
                    }
                }
                if (normalBehaviour)
                {
                    if (cursorSlot.itemSlot.stack.ID != clickedSlot.itemSlot.stack.ID)
                    {
                        ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
                        ItemStack temporaryClickedSlot = clickedSlot.itemSlot.TakeAll();

                        clickedSlot.itemSlot.InsertStack(temporarySlot);
                        cursorSlot.itemSlot.InsertStack(temporaryClickedSlot);
                    }
                    else
                    {
                        ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
                        ItemStack temporaryClickedSlot = clickedSlot.itemSlot.TakeAll();

                        int maxItemStack = world.itemTypes[temporarySlot.ID].maxItemStack;
                        int cursorAmount = Mathf.Max(0, temporarySlot.amount - (maxItemStack - temporaryClickedSlot.amount));
                        int slotAmount = Mathf.Min(maxItemStack, temporaryClickedSlot.amount + temporarySlot.amount);

                        if (cursorAmount > 0)
                        {
                            cursorSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, cursorAmount));
                        }
                        clickedSlot.itemSlot.InsertStack(new ItemStack(temporaryClickedSlot.ID, slotAmount));
                    }
                }
                else
                {
                    if (cursorSlot.itemSlot.stack.ID == clickedSlot.itemSlot.stack.ID)
                    {
                        ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
                        ItemStack temporaryClickedSlot = clickedSlot.itemSlot.TakeAll();

                        int maxItemStack = world.itemTypes[temporarySlot.ID].maxItemStack;
                        int slotAmount = Mathf.Max(0, temporaryClickedSlot.amount - (maxItemStack - temporarySlot.amount));
                        int cursorAmount = Mathf.Min(maxItemStack, temporarySlot.amount + temporaryClickedSlot.amount);

                        if (slotAmount > 0)
                        {
                            clickedSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, slotAmount));
                        }
                        cursorSlot.itemSlot.InsertStack(new ItemStack(temporaryClickedSlot.ID, cursorAmount));
                    }
                }
            }
            return;
        }
    }

    private void HandleSlotRightClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null) return;

        if (!clickedSlot.HasItem && !cursorSlot.HasItem) return;

        if (clickedSlot.itemSlot.isCreative)
        {
            cursorSlot.itemSlot.EmptySlot();
            cursorSlot.itemSlot.InsertStack(new ItemStack(clickedSlot.itemSlot.stack.ID, 1));
        }

        if (clickedSlot.HasItem && !cursorSlot.HasItem)
        {
            if (clickedSlot.itemSlot.isCraftResult)
            {
                cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());

                if (World.Instance.handCraftInventoryMenu.activeSelf)
                {
                    World.Instance.handCraftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                }
                if (World.Instance.craftInventoryMenu.activeSelf)
                {
                    World.Instance.craftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                }
            }
            else
            {
                ItemStack temporarySlot = clickedSlot.itemSlot.TakeAll();
                cursorSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, temporarySlot.amount / 2));
                clickedSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, temporarySlot.amount / 2 + (temporarySlot.amount % 2 == 0 ? 0 : 1)));
            }
            return;
        }

        if (!clickedSlot.HasItem && cursorSlot.HasItem)
        {
            if (clickedSlot.lockPlace) return;
            if (clickedSlot.itemSlot.IsContainerLinked())
            {
                VoxelState container = clickedSlot.itemSlot.GetContainer();
                if (container.properties.itemID == ItemID.FURNANCE)
                {
                    int index = clickedSlot.itemSlot.GetIndex();
                    if (index == 0)
                    {
                        if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isSmeltable) return;
                    }
                    if (index == 1)
                    {
                        if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isFuel) return;
                    }
                    if (index == 2) return;
                }
                if (container.properties.itemID == ItemID.CRAFTING_TABLE)
                {
                    int index = clickedSlot.itemSlot.GetIndex();
                    if (index == 0) return;
                }
            }
            ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
            clickedSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, 1));
            if (temporarySlot.amount > 1)
            {
                cursorSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, temporarySlot.amount - 1));
            }
            return;
        }

        if (clickedSlot.HasItem && cursorSlot.HasItem)
        {
            if (clickedSlot.itemSlot.isCraftResult)
            {
                if (cursorSlot.itemSlot.stack.ID == clickedSlot.itemSlot.stack.ID)
                {
                    int maxItemStack = world.itemTypes[cursorSlot.itemSlot.stack.ID].maxItemStack;

                    if (cursorSlot.itemSlot.stack.amount + clickedSlot.itemSlot.stack.amount <= maxItemStack)
                    {
                        int oldAmount = cursorSlot.itemSlot.stack.amount;
                        ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
                        ItemStack temporaryClickedSlot = clickedSlot.itemSlot.TakeAll();

                        int slotAmount = Mathf.Max(0, temporaryClickedSlot.amount - (maxItemStack - temporarySlot.amount));
                        int cursorAmount = Mathf.Min(maxItemStack, temporarySlot.amount + temporaryClickedSlot.amount);

                        if (slotAmount > 0)
                        {
                            clickedSlot.itemSlot.InsertStack(new ItemStack(temporarySlot.ID, slotAmount));
                        }
                        cursorSlot.itemSlot.InsertStack(new ItemStack(temporaryClickedSlot.ID, cursorAmount));

                        if (cursorAmount != oldAmount)
                        {
                            if (World.Instance.handCraftInventoryMenu.activeSelf)
                            {
                                World.Instance.handCraftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                            }
                            if (World.Instance.craftInventoryMenu.activeSelf)
                            {
                                World.Instance.craftInventoryMenu.GetComponentInChildren<RecipeCrafter>().CraftObject();
                            }
                        }
                    }
                }
            } 
            else
            {
                bool normalBehaviour = true;
                if (clickedSlot.lockPlace) normalBehaviour = false;
                else
                {
                    if (clickedSlot.itemSlot.IsContainerLinked())
                    {
                        VoxelState container = clickedSlot.itemSlot.GetContainer();
                        if (container.properties.itemID == ItemID.FURNANCE)
                        {
                            int index = clickedSlot.itemSlot.GetIndex();
                            if (index == 0)
                            {
                                if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isSmeltable) return;
                            }
                            if (index == 1)
                            {
                                if (!World.Instance.itemTypes[cursorSlot.itemSlot.stack.ID].isFuel) return;
                            }
                            if (index == 2) normalBehaviour = false;
                        }
                        if (container.properties.itemID == ItemID.CRAFTING_TABLE)
                        {
                            int index = clickedSlot.itemSlot.GetIndex();
                            if (index == 0) normalBehaviour = false;
                        }
                    }
                }
                if (normalBehaviour)
                {
                    if (clickedSlot.itemSlot.stack.ID == cursorSlot.itemSlot.stack.ID)
                    {
                        int maxItemStack = world.itemTypes[cursorSlot.itemSlot.stack.ID].maxItemStack;
                        if (clickedSlot.itemSlot.stack.amount < maxItemStack)
                        {
                            int oldAmount = clickedSlot.itemSlot.stack.amount;
                            int newAmount = clickedSlot.itemSlot.Add(1);
                            if (newAmount != oldAmount) cursorSlot.itemSlot.Take(1);
                        }
                    }
                    else
                    {
                        ItemStack temporarySlot = cursorSlot.itemSlot.TakeAll();
                        ItemStack temporaryClickedSlot = clickedSlot.itemSlot.TakeAll();

                        clickedSlot.itemSlot.InsertStack(temporarySlot);
                        cursorSlot.itemSlot.InsertStack(temporaryClickedSlot);
                    }
                }
                else
                {
                    if (clickedSlot.itemSlot.stack.ID == cursorSlot.itemSlot.stack.ID)
                    {
                        int maxItemStack = world.itemTypes[cursorSlot.itemSlot.stack.ID].maxItemStack;
                        if (cursorSlot.itemSlot.stack.amount < maxItemStack)
                        {
                            int oldAmount = clickedSlot.itemSlot.stack.amount;
                            int newAmount = cursorSlot.itemSlot.Add(1);
                            if (newAmount != oldAmount) clickedSlot.itemSlot.Take(1);
                        }
                    }
                }
            }
            return;
        }
    }

    private UIItemSlot CheckForSlot()
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        rayCaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
          
            if (result.gameObject.tag == "UIItemSlot")
            {
                return result.gameObject.GetComponent<UIItemSlot>();
            }
        }

        return null;
    }
}
