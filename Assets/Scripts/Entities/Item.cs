using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Entity
{
    public string itemName;
    public ItemID itemID;

    public bool isBlock;
    public bool isTool;
    public bool isFood;
    public bool isSmeltable;
    public bool isFuel;
    public float fuelTimer;
    public ItemID targetSmeltItem;
    public VoxelBlockID blockID;

    public Sprite icon;
    public int maxItemStack;

    public Player target;

    private float pickUpTimer = 0.75f;
    private float lifeTimer = 300f;

    private bool isEquiped = false;

    private Tool thisTool;

    protected override void Awake()
    {
        base.Awake();
        target = FindObjectOfType<Player>();
        thisTool = this.GetComponent<Tool>();
    }

    protected override void Update()
    {
        if (!isEquiped)
        {
            base.Update();

            if (front == 2 || back == 2 || left == 2 || right == 2 || isGrounded)
            {
                vertical = 0;
                horizontal = 0;
            }

            CalculateVelocity();
            transform.Translate(velocity, Space.World);
            transform.RotateAround(transform.position, Vector3.up, 20 * Time.deltaTime);

            if (pickUpTimer < 0) ItemPickUp();
            else pickUpTimer -= Time.deltaTime;

            lifeTimer -= Time.deltaTime;
            if (lifeTimer < 0) Destroy(this.gameObject);
        }
    }

    public void ItemPickUp()
    {
        if (Vector3.Distance(this.transform.position, target.transform.position) < 1.5f)
        {
            ItemSlot availableSlot = null;
            
            Toolbar toolbar = World.Instance.toolbar.GetComponent<Toolbar>();
            availableSlot = toolbar.GetFirstAvailableSlot((byte)itemID);
            if (availableSlot == null)
            {
                SurvivalInventory inventory = World.Instance.survivalInventoryMenu.GetComponentInChildren<SurvivalInventory>();
                availableSlot = inventory.GetFirstAvailableSlot((byte)itemID);
            } 
            
            if (availableSlot != null)
            {
                if (availableSlot.HasItem)
                {
                    ItemStack temporaryStack = availableSlot.TakeAll();
                    availableSlot.InsertStack(new ItemStack(temporaryStack.ID, temporaryStack.amount + 1));
                }
                else
                {
                    availableSlot.InsertStack(new ItemStack((byte)itemID, 1));
                }
                Destroy(this.gameObject);
            }
        }
    }

    public void SetEquiped()
    {
        isEquiped = true;
    }

    public Tool GetThisTool()
    {
        return isTool ? thisTool : null;
    }
}
