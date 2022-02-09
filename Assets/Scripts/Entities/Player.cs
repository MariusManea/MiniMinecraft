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
    public Transform thirdPersonItemHolder;
    public Transform thirdPersonToolHolder;
    public Transform thirdPersonBowHolder;

    public Item equippedItem = null;

    private float breakingTime = 0f;
    private float eatingTime = 0f;

    public GameObject breakingTimerBackground;
    public Image breakingTimerBar;

    private bool attacking;
    private float attackingTimer;

    public GameObject flyingArrow;
    private bool bowMode = false;

    public int arrowsLoaded = 0;
    private SurvivalInventory inventory;

    public Text arrowSlotAmount;

    public Animator playerAnimator;

    public Camera itemsCamera;
    public GameObject steve;
    public Camera thirdPersonCamera;

    protected override void Awake()
    {
        base.Awake();
        EntitiesCounter.playerEntity = this;
    }

    protected override void Start()
    {
        base.Start();
        inventory = World.Instance.survivalInventoryMenu.GetComponentInChildren<SurvivalInventory>();

        world.inUI = false;
    }

    protected override void Update()
    {
        if (health <= 0)
        {
            world.toolbar.GetComponent<Toolbar>().emptyInventory();
            world.survivalInventoryMenu.SetActive(true);
            world.survivalInventoryMenu.GetComponentInChildren<SurvivalInventory>().emptyInventory();
            world.survivalInventoryMenu.SetActive(false);
            world.armorInventoryMenu.SetActive(true);
            world.armorInventoryMenu.GetComponent<ContainerInventory>().emptyInventory();
            world.armorInventoryMenu.SetActive(false);
            world.handCraftInventoryMenu.SetActive(true);
            world.handCraftInventoryMenu.GetComponentInChildren<ContainerInventory>().emptyInventory();
            world.handCraftInventoryMenu.SetActive(false);
            world.craftInventoryMenu.SetActive(false);
            this.transform.position = world.spawnPosition;
            HealthChange(100);
            HungerChange(100);

        }

        base.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            world.inUI = !world.inUI;
            world.mainUI = world.inUI;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            gameMode = GameMode.creative;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            gameMode = GameMode.survival;
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

            if (velocity.x != 0 || velocity.z != 0)
            {
                playerAnimator.SetBool("Walking", true);
                if (isSprinting) playerAnimator.SetBool("Running", true);
                else playerAnimator.SetBool("Running", false);
            } 
            else
            {
                playerAnimator.SetBool("Walking", false);
                playerAnimator.SetBool("Running", false);
            }
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
        if (Input.GetKeyDown(KeyCode.F5))
        {
            world.inFirstPerson = !world.inFirstPerson;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            bowMode = !bowMode;
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            ItemSlot arrowSlot = toolbar.GetFirstArrowSlot();
            if (arrowSlot == null && inventory != null)
            {
                arrowSlot = inventory.GetFirstArrowSlot();
            }
            if (arrowSlot != null)
            {
                ItemStack arrows = arrowSlot.TakeAll();
                arrowsLoaded += arrows.amount;
                arrowSlotAmount.text = arrowsLoaded.ToString();
            }
        }

        if (Input.GetMouseButtonDown(0) && !attacking)
        {
            Vector3 rayOrigin = FPSCamera.position;
            Vector3 rayDirection = FPSCamera.forward;
            RaycastHit hitObject;
            toolHolder.animateOnce = true;
            playerAnimator.SetTrigger("Attack");
            if (Physics.Raycast(rayOrigin, rayDirection, out hitObject, 2))
            {
                EnemyEntity entityHit = hitObject.transform.GetComponent<EnemyEntity>();
                if (entityHit != null)
                {
                    float damageAmount = -1;
                    ToolType toolType = ToolType.HAND;
                    ToolQuality toolQuality = ToolQuality.NON_DROP;
                    if (equippedItem != null && equippedItem.GetThisTool() != null)
                    {
                        toolType = equippedItem.GetThisTool().toolType;
                        toolQuality = equippedItem.GetThisTool().toolQuality;
                    }
                    switch (toolQuality)
                    {
                        case ToolQuality.STONE: damageAmount -= 1; break;
                        case ToolQuality.IRON: damageAmount -= 2; break;
                        case ToolQuality.DIAMOND: damageAmount -= 3; break;
                        default: break;
                    }
                    if (toolType == ToolType.AXE) damageAmount *= 2;
                    entityHit.ChangeHealth(damageAmount, this.transform.forward / 2);
                    attacking = true;
                    attackingTimer = 0.4f;
                }
            }
        }

        if (!bowMode)
        {
            if (Input.GetMouseButtonDown(1) && !attacking)
            {
                if (equippedItem.itemID == ItemID.BOW && arrowsLoaded > 0)
                {
                    itemHolder.animate = true;
                    itemHolder.shortAnimation = true;
                    itemHolder.animateOnce = true;
                    attacking = true;
                    attackingTimer = 0.4f;
                    Vector3 startPosition = new Vector3();
                    if (world.inFirstPerson)
                    {
                        startPosition = itemHolder.transform.position + itemHolder.transform.forward + itemHolder.transform.up / 2;
                    } 
                    else
                    {
                        startPosition = thirdPersonBowHolder.position;
                    }
                    Projectile projectile = GameObject.Instantiate(flyingArrow, startPosition, this.transform.rotation).GetComponent<Projectile>();
                    projectile.damage = 4;
                    projectile.verticalMomentum = (FPSCamera.rotation.eulerAngles.x < 180 ? -5 : -30) * Mathf.Sin(FPSCamera.rotation.eulerAngles.x * Mathf.Deg2Rad);
                    projectile.SetVertical(Mathf.Cos(FPSCamera.rotation.eulerAngles.x * Mathf.Deg2Rad));
                    projectile.SetRotation(FPSCamera.rotation);
                    arrowsLoaded--;
                    arrowSlotAmount.text = arrowsLoaded.ToString();
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(1) && !attacking)
            {
                if (equippedItem.itemID == ItemID.BOW && arrowsLoaded > 0)
                {
                    itemHolder.animate = true;
                    itemHolder.shortAnimation = true;
                    attacking = true;
                    attackingTimer = 0.1f;
                    Vector3 startPosition = new Vector3();
                    if (world.inFirstPerson)
                    {
                        startPosition = itemHolder.transform.position + itemHolder.transform.forward + itemHolder.transform.up / 2;
                    }
                    else
                    {
                        startPosition = thirdPersonBowHolder.position;
                    }
                    Projectile projectile = GameObject.Instantiate(flyingArrow, startPosition, this.transform.rotation).GetComponent<Projectile>();
                    projectile.damage = 1;
                    projectile.verticalMomentum = (FPSCamera.rotation.eulerAngles.x < 180 ? -5 : -30) * Mathf.Sin(FPSCamera.rotation.eulerAngles.x * Mathf.Deg2Rad);
                    projectile.SetVertical(Mathf.Cos(FPSCamera.rotation.eulerAngles.x * Mathf.Deg2Rad));
                    projectile.SetRotation(FPSCamera.rotation);
                    arrowsLoaded = arrowsLoaded - Random.Range(0, 2);
                    arrowSlotAmount.text = arrowsLoaded.ToString();
                }
            }
        }


        if (!attacking)
        {
            if (Input.GetMouseButton(1))
            {
                if (equippedItem != null && equippedItem.isFood && hunger < 20)
                {
                    itemHolder.animate = true;
                    playerAnimator.SetBool("Eating", true);
                    eatingTime += Time.deltaTime;
                    if (eatingTime > 1.5f)
                    {
                        HungerChange(equippedItem.hungerValue);
                        world.toolbar.GetComponent<Toolbar>().UseItemInHand();
                        playerAnimator.SetBool("Eating", false);
                    }
                }
                else
                {
                    eatingTime = 0;
                    itemHolder.animate = false;
                    playerAnimator.SetBool("Eating", false);
                }
            }
            else
            {
                eatingTime = 0;
                itemHolder.animate = false;
                playerAnimator.SetBool("Eating", false);
            }

            if (highLightBlock.gameObject.activeSelf)
            {
                // Destroy block
                if (gameMode == GameMode.survival)
                {
                    if (Input.GetMouseButton(0))
                    {
                        playerAnimator.SetBool("Mining", true);
                        if (oldHighlightPosition != highLightBlock.position) breakingTime = 0;

                        breakingTimerBackground.SetActive(true);
                        toolHolder.animate = true;
                        toolHolder.animateOnce = false;
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
                                bool hasMultipleRandomDrop = World.Instance.itemTypes[(byte)voxel.properties.itemID].hasMultipleRandomDrop;
                                if (Random.Range(0.0f, 1.0f) < (hasMultipleRandomDrop ? 2 : VoxelData.GetExtraDropChance))
                                {
                                    int randomDropNumber = Random.Range(2, 5);
                                    for (int i = 0; i < (hasMultipleRandomDrop ? randomDropNumber : 1); ++i)
                                    {
                                        ItemID randomExtraDropID = voxel.properties.extraDropItemsID[Mathf.Clamp(Random.Range(0, voxel.properties.extraDropItemsID.Length), 0, voxel.properties.extraDropItemsID.Length - 1)];
                                        Item extraDrop = GameObject.Instantiate(World.Instance.itemTypes[(byte)randomExtraDropID], highLightBlock.position + new Vector3(0.5f, 0.5f, 0.5f), new Quaternion());
                                        extraDrop.verticalMomentum = Random.Range(2f, 6f);
                                        extraDrop.horizontal = Random.Range(-1.0f, 1.0f);
                                        extraDrop.vertical = Random.Range(-1.0f, 1.0f);
                                    }
                                }
                            }

                            if (voxel.properties.isContainer && voxel.itemsContained != null)
                            {
                                foreach (ItemStack stack in voxel.itemsContained)
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
                        playerAnimator.SetBool("Mining", false);
                        breakingTimerBackground.SetActive(false);
                        breakingTimerBar.rectTransform.sizeDelta = new Vector2(0, 20);
                        if (!toolHolder.animateOnce)
                        {
                            toolHolder.animate = false;
                        }
                        handHolder.animate = false;
                        if (eatingTime == 0)
                        {
                            itemHolder.animate = false;
                        }
                        toolHolder.shortAnimation = false;
                        handHolder.shortAnimation = false;
                        itemHolder.shortAnimation = false;
                        breakingTime = 0f;
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        playerAnimator.SetTrigger("Use");
                        world.GetChunkFromVector3(highLightBlock.position).EditVoxel(highLightBlock.position, (byte)VoxelBlockID.AIR_BLOCK);
                    }
                }

                // Place block or enter container or hoe dirt/grass
                if (Input.GetMouseButtonDown(1))
                {
                    playerAnimator.SetTrigger("Use");
                    VoxelState voxel = world.GetChunkFromVector3(highLightBlock.position).GetVoxelFromGlobalVector3(highLightBlock.position);
                    if (equippedItem != null && equippedItem.itemID == ItemID.BOSS_SUMMONER)
                    {
                        toolbar.uiItemSlots[toolbar.slotIndex].itemSlot.Take(1);
                        GameObject.Instantiate(World.Instance.boss, new Vector3(voxel.globalPosition.x + 0.5f, voxel.position.y + 1, voxel.globalPosition.z + 0.5f), Quaternion.Euler(0, Random.Range(0f, 359.0f), 0));
                    }
                    else {
                        if (equippedItem != null && equippedItem.isTool && equippedItem.GetThisTool().toolType == ToolType.HOE)
                        {
                            if (voxel.id == (byte)VoxelBlockID.DIRT_BLOCK || voxel.id == (byte)VoxelBlockID.GRASS_BLOCK)
                            {
                                voxel.chunkData.chunk.EditVoxel(highLightBlock.position, (byte)VoxelBlockID.DRY_FARMLAND, 1);
                                toolHolder.animateOnce = true;
                            }
                        }
                        else
                        {
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
                                        if (World.Instance.itemTypes[itemInHandID].isSeed)
                                        {
                                            VoxelState voxelUnder = world.GetVoxelState(placeBlock.position).neighbours[3];
                                            if (!(voxelUnder.id == (byte)VoxelBlockID.DRY_FARMLAND || voxelUnder.id == (byte)VoxelBlockID.WET_FARMLAND))
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
                }
            }
            else
            {
                playerAnimator.SetBool("Mining", false);
                breakingTimerBackground.SetActive(false);
                breakingTimerBar.rectTransform.sizeDelta = new Vector2(0, 20);
                if (!toolHolder.animateOnce)
                {
                    toolHolder.animate = false;
                }
                handHolder.animate = false;
                if (eatingTime == 0)
                {
                    itemHolder.animate = false;
                }
                toolHolder.shortAnimation = false;
                handHolder.shortAnimation = false;
                itemHolder.shortAnimation = false;
                breakingTime = 0f;
            }
        }
        else
        {
            attackingTimer -= Time.deltaTime;
            if (attackingTimer < 0) attacking = false;
            eatingTime = 0;
            playerAnimator.SetBool("Eating", false);
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

    public void HealthChange(float amount, Vector3 knockbackDirection)
    {
        health = Mathf.Clamp(health + amount, 0, 20);

        verticalMomentum = 3;
        knockbackDirection.z = -knockbackDirection.z;
        Quaternion diff = Quaternion.LookRotation(knockbackDirection - transform.forward, Vector3.up);
        horizontalMomentum = diff * knockbackDirection / 2;

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
        if (world.inFirstPerson)
        {
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
        else
        {
            if (_item)
            {
                Transform parent = _item.isTool ? thirdPersonToolHolder : thirdPersonItemHolder;
                if (_item.itemID == ItemID.BOW)
                {
                    parent = thirdPersonBowHolder;
                    playerAnimator.SetBool("BowAiming", true);
                }
                else
                {
                    playerAnimator.SetBool("BowAiming", false);
                }
                handHolder.gameObject.SetActive(false);
                equippedItem = GameObject.Instantiate(_item.gameObject, parent).GetComponent<Item>();
                SetLayerRecursively(equippedItem.gameObject, LayerMask.NameToLayer("Default"));
                equippedItem.SetEquiped();
            }
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