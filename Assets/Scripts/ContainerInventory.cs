using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    World world;
    public int inventorySize;

    private VoxelState container;

    List<ItemSlot> slots = new List<ItemSlot>();

    public Image fuelImage;
    public Image smeltImage;

    public string siblingSetting;

    public bool permanentContainer;
    public bool dropOnClose;
    public int offset;

    private void Start()
    {
        world = FindObjectOfType<World>();

        for (int i = 0; i < inventorySize; ++i)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);
            slots.Add(new ItemSlot(newSlot.GetComponent<UIItemSlot>()));
            slots[i].isCreative = false;

            switch (siblingSetting)
            {
                case "EVEN":
                    newSlot.transform.SetSiblingIndex(2 * i);
                    break;
                case "LAST":
                    newSlot.transform.SetSiblingIndex(this.transform.childCount - 1);
                    break;
                default:
                    break;
            }


            if (fuelImage != null) fuelImage.fillAmount = 0;
            if (smeltImage != null) smeltImage.fillAmount = 0;
        }
    }

    private void Update()
    {
        if (container != null)
        {
            if (container.id == (byte)VoxelBlockID.FURNANCE)
            {
                if (fuelImage != null) fuelImage.fillAmount = 0;
                if (smeltImage != null) smeltImage.fillAmount = 0;
            }
            if (container.id == (byte)VoxelBlockID.FURNANCE_LIT)
            {
                if (fuelImage != null) fuelImage.fillAmount = Mathf.Clamp01(container.remainingSmeltingTImer / container.maxRemainingSmeltingTImer);
                if (smeltImage != null) smeltImage.fillAmount = Mathf.Clamp01((4f - container.smeltingTimer) / 4f);
            }
        }
    }

    public void ClearContainerUI()
    {
        for (int i = 0; i < inventorySize; ++i)
        {
            if (dropOnClose && slots[i].stack != null)
            {
                VoxelState container = slots[i].GetContainer();
                if (container != null)
                { 
                    Item _item = GameObject.Instantiate(World.Instance.itemTypes[slots[i].stack.ID], container.globalPosition + new Vector3(0.5f, 1.5f, 0.5f), new Quaternion());
                    _item.verticalMomentum = Random.Range(2f, 6f);
                    _item.horizontal = Random.Range(-1.0f, 1.0f);
                    _item.vertical = Random.Range(-1.0f, 1.0f);
                    slots[i].GetContainer().itemsContained[i + offset] = null;
                }
            }
            slots[i].UnlinkContainer();
            slots[i].EmptySlot();
        }
    }

    public void PopulateContainerUI(VoxelState _container)
    {
        if (_container != null)
        {
            container = _container;
            ItemStack[] _items = _container.itemsContained;
            for (int i = 0; i < inventorySize; ++i)
            {
                slots[i].InsertStackAndLinkToContainer(_items[i], _container, i);
            }
        }
    }

    public void PopulateContainerUI(VoxelState _container, int offset)
    {
        if (_container != null)
        {
            container = _container;
            ItemStack[] _items = _container.itemsContained;
            for (int i = 0; i < inventorySize; ++i)
            {
                slots[i].InsertStackAndLinkToContainer(_items[i + offset], _container, i + offset);
            }
        }
    }

    public void OnDisable()
    {
        if (!permanentContainer)
        {
            ClearContainerUI();
        }
    }
}
