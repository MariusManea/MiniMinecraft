using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{
    public Transform FPSCamera;

    public Transform highLightBlock;
    public Transform placeBlock;
    private Vector3 oldHighlightPosition;

    public float checkIncrement = 0.1f;
    public float reach = 5;

    public Toolbar toolbar;

    public float hunger = 20;

    public Image[] healthIcons;
    public Image[] hungerIcons;

    public Sprite[] healthStates;
    public Sprite[] hungerStates;

    private float healTimer = 1f;
    private float sprintHungerTimer = 15f;
    private float lowHungerTimer = 90f;
    private float hungerDamageTimer = 30f;

    public GameMode gameMode = GameMode.survival;

    public HolderAnimation toolHolder;
    public HolderAnimation handHolder;
    public HolderAnimation itemHolder;

    public Item equippedItem = null;

    private float breakingTime = 0f;

    public GameObject breakingTimerBackground;
    public Image breakingTimerBar;

    protected override void Awake()
    {
        base.Awake();
        EntitiesCounter.playerEntity = this;
    }

    protected override void Start()
    {
        base.Start();

        world.inUI = false;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            world.inUI = !world.inUI;
            world.mainUI = world.inUI;
        }

        if (world.inUI && Input.GetKeyDown(KeyCode.Escape))
        {
            world.inUI = false;
        }

        if (!world.inUI)
        {
            GetPlayerInput();
            if (jumpRequest) Jump();

            CalculateVelocity();
            transform.Rotate(Vector3.up * rotateHorizontal * world.settings.mouseSensitivity);
            FPSCamera.Rotate(Vector3.right * -rotateVertical * world.settings.mouseSensitivity);
            Vector3 cameraRotation = FPSCamera.localRotation.eulerAngles;
            cameraRotation.x = cameraRotation.x < 0 || cameraRotation.x > 90 ? 0 : cameraRotation.x;
            FPSCamera.localRotation = Quaternion.Euler(cameraRotation);

            transform.Translate(velocity, Space.World);
            PlaceCursorBlock();
        }
        Vector3 XZDirection = transform.forward;
        XZDirection.y = 0;

        if (Vector3.Angle(XZDirection, Vector3.forward) <= 45) orientation = 0;
        else if (Vector3.Angle(XZDirection, Vector3.right) <= 45) orientation = 5;
        else if (Vector3.Angle(XZDirection, Vector3.back) <= 45) orientation = 1;
        else orientation = 4;


        HealPlayer();
        HungerEffects();
    }

    

    private void GetPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Application.Quit();
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        rotateHorizontal = Input.GetAxis("Mouse X");
        rotateVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint") && hunger > 6)
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint") || hunger <= 6)
        {
            isSprinting = false;
        } 
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = true;
        }
        if (Input.GetButtonUp("Crouch"))
        {
            isCrouching = false;
        }
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        if (highLightBlock.gameObject.activeSelf)
        {
            // Destroy block
            if (gameMode == GameMode.survival)
            {
                if (Input.GetMouseButton(0))
                {
                    if (oldHighlightPosition != highLightBlock.position) breakingTime = 0;

                    breakingTimerBackground.SetActive(true);
                    toolHolder.animate = true;
                    handHolder.animate = true;
                    itemHolder.animate = true;

                    breakingTime += Time.deltaTime;

                    VoxelState voxel = world.GetChunkFromVector3(highLightBlock.position).GetVoxelFromGlobalVector3(highLightBlock.position);

                    ToolQuality equippedQuality = equippedItem != null ? (equippedItem.isTool ? equippedItem.GetThisTool().toolQuality : ToolQuality.WOOD) : ToolQuality.WOOD;
                    ToolType equippedType = equippedItem != null ? (equippedItem.isTool ? equippedItem.GetThisTool().toolType : ToolType.HAND) : ToolType.HAND;

                    float itemBaseBreakinTime = voxel.properties.breakingTime;

                    float multiplier = 1f;
                    if (equippedType != voxel.properties.prefferedTool)
                    {
                        multiplier *= 2f;
                    }
                    else
                    {
                        switch (equippedQuality)
                        {
                            case ToolQuality.STONE:
                                multiplier /= 2;
                                break;
                            case ToolQuality.IRON:
                                multiplier /= 4;
                                break;
                            case ToolQuality.DIAMOND:
                                multiplier /= 8;
                                break;
                            default:
                                break;
                        }
                    }

                    breakingTimerBar.rectTransform.sizeDelta = new Vector2(Mathf.Clamp((breakingTime / (itemBaseBreakinTime * multiplier)) * 400, 0, 400), 20);

                    if (breakingTime >= itemBaseBreakinTime * multiplier)
                    {
                        breakingTime = 0;

                        if ((voxel.properties.minimumTool == ToolType.HAND || (voxel.properties.minimumTool == equippedType)) && equippedQuality >= voxel.properties.minimumQuality)
                        {
                            Item blockItem = GameObject.Instantiate(World.Instance.itemTypes[(byte)voxel.properties.dropItemID], highLightBlock.position + new Vector3(0.5f, 0.5f, 0.5f), new Quaternion());
                            blockItem.verticalMomentum = Random.Range(2f, 6f);
                            blockItem.horizontal = Random.Range(-1.0f, 1.0f);
                            blockItem.vertical = Random.Range(-1.0f, 1.0f);
                        }

                        if (voxel.properties.extraDropItemsID != null && voxel.properties.extraDropItemsID.Length > 0)
                        {
                            if (Random.Range(0.0f, 1.0f) < VoxelData.GetExtraDropChance)
                            {
                                ItemID randomExtraDropID = voxel.properties.extraDropItemsID[Mathf.Clamp(Random.Range(0, voxel.properties.extraDropItemsID.Length), 0, voxel.properties.extraDropItemsID.Length - 1)];
                                Item extraDrop = GameObject.Instantiate(World.Instance.itemTypes[(byte)randomExtraDropID], highLightBlock.position + new Vector3(0.5f, 0.5f, 0.5f), new Quaternion());
                                extraDrop.verticalMomentum = Random.Range(2f, 6f);
                                extraDrop.horizontal = Random.Range(-1.0f, 1.0f);
                                extraDrop.vertical = Random.Range(-1.0f, 1.0f);
                            }
                        }

                        if (voxel.properties.isContainer && voxel.itemsContained != null)
                        {
                            foreach(ItemStack stack in voxel.itemsContained)
                            {
                                if (stack != null)
                                {
                                    for (int i = 0; i < stack.amount; ++i)
                                    {
                                        Item chestItem = GameObject.Instantiate(World.Instance.itemTypes[stack.ID], highLightBlock.position + new Vector3(0.5f, 0.5f, 0.5f), new Quaternion());
                                        chestItem.verticalMomentum = Random.Range(2f, 6f);
                                        chestItem.horizontal = Random.Range(-1.0f, 1.0f);
                                        chestItem.vertical = Random.Range(-1.0f, 1.0f);
                                    }
                                }
                            }
                        }

                        world.GetChunkFromVector3(highLightBlock.position).EditVoxel(highLightBlock.position, 0);
                    }
                }
                else
                {
                    breakingTimerBackground.SetActive(false);
                    breakingTimerBar.rectTransform.sizeDelta = new Vector2(0, 20);
                    toolHolder.animate = false;
                    handHolder.animate = false;
                    itemHolder.animate = false;
                    breakingTime = 0f;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    world.GetChunkFromVector3(highLightBlock.position).EditVoxel(highLightBlock.position, 0);
                }
            }

            // Place block or enter container
            if (Input.GetMouseButtonDown(1))
            {
                VoxelState voxel = world.GetChunkFromVector3(highLightBlock.position).GetVoxelFromGlobalVector3(highLightBlock.position);
                if (!voxel.properties.isContainer || Input.GetKey(KeyCode.LeftShift))
                {
                    if (toolbar.uiItemSlots[toolbar.slotIndex].HasItem)
                    {
                        byte itemInHandID = toolbar.uiItemSlots[toolbar.slotIndex].itemSlot.stack.ID;
                        if (World.Instance.itemTypes[itemInHandID].isBlock)
                        {
                            bool canPlaceBlock = true;
                            if (itemInHandID == (byte)ItemID.OAK_SAPLING)
                            {
                                VoxelState voxelUnder = world.GetVoxelState(placeBlock.position).neighbours[3];
                                if (!(voxelUnder.id == (byte)VoxelBlockID.GRASS_BLOCK || voxelUnder.id == (byte)VoxelBlockID.DIRT_BLOCK))
                                {
                                    canPlaceBlock = false;
                                }
                            }
                            if (canPlaceBlock)
                            {
                                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, (byte)world.itemTypes[itemInHandID].blockID);
                                toolbar.uiItemSlots[toolbar.slotIndex].itemSlot.Take(1);
                            }
                        }
                    }
                }
                else
                {
                    World.Instance.OpenContainer(voxel);
                }
            }
        }
        else
        {
            breakingTimerBackground.SetActive(false);
            breakingTimerBar.rectTransform.sizeDelta = new Vector2(0, 20);
            toolHolder.animate = false;
            handHolder.animate = false;
            itemHolder.animate = false;
            breakingTime = 0f;
        }
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = FPSCamera.position + (FPSCamera.forward * step);
            VoxelState voxel = world.GetVoxelState(pos);
            if (voxel != null && world.blockTypes[voxel.id].isSolid && !(world.blockTypes[voxel.id].isLiquid))
            {
                oldHighlightPosition = highLightBlock.position;

                highLightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highLightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
        highLightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }

    public void HealthChange(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, 20);

        for (int i = 0; i < healthIcons.Length; i++)
        {
            healthIcons[i].sprite = healthStates[(int)Mathf.Clamp((2 * (i + 1) - health), 0, 2)];
        }
    }

    public void HungerChange(float amount)
    {
        if (hunger < 20 || amount < 0)
        {
            hunger = Mathf.Clamp(hunger + amount, 0, 25);

            for (int i = 0; i < hungerIcons.Length; i++)
            {
                hungerIcons[i].sprite = hungerStates[(int)Mathf.Clamp((2 * (i + 1) - hunger), 0, 2)];
            }
        }
    }

    public void HealPlayer()
    {
        if (health < 20 && hunger >= 19)
        {
            healTimer -= Time.deltaTime;
            if (healTimer < 0)
            {
                HealthChange(1);
                HungerChange(-0.4f);
                healTimer = Mathf.Min(1 / Mathf.Clamp(hunger - 20, 1, 5), 1);
            }
        }
    }

    public void HungerEffects()
    {
        if (isSprinting)
        {
            sprintHungerTimer -= Time.deltaTime;
            if (sprintHungerTimer < 0)
            {
                sprintHungerTimer = 15f;
                HungerChange(-1);
            }
        }
        if (hunger <= 6)
        {
            lowHungerTimer -= Time.deltaTime;
            if (lowHungerTimer < 0)
            {
                lowHungerTimer = 90f;
                HungerChange(-1);
            }
        }
        else
        {
            lowHungerTimer = 90f;
            hungerDamageTimer = 30f;
        }
        if (hunger <= 0)
        {
            hungerDamageTimer -= Time.deltaTime;
            if (hungerDamageTimer < 0)
            {
                hungerDamageTimer = 30f;
                HealthChange(-1);
            }
        }
    }

    public void EquipItem(Item _item)
    {
        breakingTime = 0f;
        if (equippedItem != null)
        {
            Destroy(equippedItem.gameObject);
        }
        toolHolder.gameObject.SetActive(false);
        itemHolder.gameObject.SetActive(false);
        if (_item != null)
        {
            Transform parent = _item.isTool ? toolHolder.transform : itemHolder.transform;
            handHolder.gameObject.SetActive(false);
            parent.gameObject.SetActive(true);
            equippedItem = GameObject.Instantiate(_item.gameObject, parent).GetComponent<Item>();
            equippedItem.SetEquiped();
            SetLayerRecursively(equippedItem.gameObject, LayerMask.NameToLayer("ItemInHand"));
        }
        else
        {
            handHolder.gameObject.SetActive(true);
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}


[System.Serializable]
public enum GameMode
{
    survival = 0,
    creative = 1,
    spectator = 2,
}