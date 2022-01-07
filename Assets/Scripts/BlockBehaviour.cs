using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockBehaviour
{
    private static bool IsEmptyAbove(VoxelState voxel)
    {
        return (voxel.neighbours[2] == null || voxel.neighbours[2].id == (byte)VoxelBlockID.AIR_BLOCK);
    }

    private static bool IsEmptyOrLeavesAbove(VoxelState voxel)
    {
        return (voxel.neighbours[2] == null || voxel.neighbours[2].id == (byte)VoxelBlockID.AIR_BLOCK ||
            voxel.neighbours[2].id == (byte)VoxelBlockID.OAK_LEAVES);
    }

    public static bool Active (VoxelState voxel)
    {
        switch (voxel.id)
        {
            case (byte)VoxelBlockID.GRASS_BLOCK:
                if (
                    (voxel.neighbours[2] != null && voxel.neighbours[2].id != (byte)VoxelBlockID.AIR_BLOCK) ||
                    (voxel.neighbours[0] != null && voxel.neighbours[0].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[0])) ||
                    (voxel.neighbours[1] != null && voxel.neighbours[1].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[1])) ||
                    (voxel.neighbours[4] != null && voxel.neighbours[4].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[4])) ||
                    (voxel.neighbours[5] != null && voxel.neighbours[5].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[5])) ||
                    (voxel.neighbours[0] != null && voxel.neighbours[0].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[0].neighbours[3] != null && voxel.neighbours[0].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) ||
                    (voxel.neighbours[1] != null && voxel.neighbours[1].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[1].neighbours[3] != null && voxel.neighbours[1].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) ||
                    (voxel.neighbours[4] != null && voxel.neighbours[4].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[4].neighbours[3] != null && voxel.neighbours[4].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) ||
                    (voxel.neighbours[5] != null && voxel.neighbours[5].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[5].neighbours[3] != null && voxel.neighbours[5].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) ||
                    (voxel.neighbours[0] != null && voxel.neighbours[0].neighbours[2] != null && voxel.neighbours[0].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[0].neighbours[2])) ||
                    (voxel.neighbours[1] != null && voxel.neighbours[1].neighbours[2] != null && voxel.neighbours[1].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[1].neighbours[2])) ||
                    (voxel.neighbours[4] != null && voxel.neighbours[4].neighbours[2] != null && voxel.neighbours[4].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[4].neighbours[2])) ||
                    (voxel.neighbours[5] != null && voxel.neighbours[5].neighbours[2] != null && voxel.neighbours[5].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[5].neighbours[2]))
                    )
                {
                    return true;
                }
                break;

            case (byte)VoxelBlockID.WATER_BLOCK:
                if (
                    (voxel.neighbours[0] != null && (voxel.neighbours[0].id == (byte)VoxelBlockID.AIR_BLOCK)) || 
                    (voxel.neighbours[1] != null && (voxel.neighbours[1].id == (byte)VoxelBlockID.AIR_BLOCK)) || 
                    (voxel.neighbours[3] != null && (voxel.neighbours[3].id == (byte)VoxelBlockID.AIR_BLOCK)) || 
                    (voxel.neighbours[4] != null && (voxel.neighbours[4].id == (byte)VoxelBlockID.AIR_BLOCK)) || 
                    (voxel.neighbours[5] != null && (voxel.neighbours[5].id == (byte)VoxelBlockID.AIR_BLOCK)) 
                    )
                {
                    return true;
                }
                break;
            case (byte)VoxelBlockID.FURNANCE:
                Item toSmelt = (voxel.itemsContained != null && voxel.itemsContained[0] != null && voxel.itemsContained[0].amount > 0) ? World.Instance.itemTypes[voxel.itemsContained[0].ID] : null;
            
                if (
                    voxel.itemsContained != null &&
                    toSmelt != null && toSmelt.isSmeltable &&
                    voxel.itemsContained[1] != null && voxel.itemsContained[1].amount > 0 && World.Instance.itemTypes[voxel.itemsContained[1].ID].isFuel &&
                    (voxel.itemsContained[2] == null || (voxel.itemsContained[2].ID == (byte)toSmelt.targetSmeltItem && voxel.itemsContained[2].amount < World.Instance.itemTypes[(byte)toSmelt.targetSmeltItem].maxItemStack))
                    )
                {
                    return true;
                }
                break;
            case (byte)VoxelBlockID.FURNANCE_LIT:
                return true;
            case (byte)VoxelBlockID.OAK_SAPLING:
                if (IsEmptyOrLeavesAbove(voxel) && (
                    voxel.neighbours[2] == null || (
                        IsEmptyOrLeavesAbove(voxel.neighbours[2]) &&
                        (voxel.neighbours[2].neighbours[0] == null || voxel.neighbours[2].neighbours[0].id == (byte)VoxelBlockID.AIR_BLOCK) &&
                        (voxel.neighbours[2].neighbours[1] == null || voxel.neighbours[2].neighbours[1].id == (byte)VoxelBlockID.AIR_BLOCK) &&
                        (voxel.neighbours[2].neighbours[4] == null || voxel.neighbours[2].neighbours[4].id == (byte)VoxelBlockID.AIR_BLOCK) &&
                        (voxel.neighbours[2].neighbours[5] == null || voxel.neighbours[2].neighbours[5].id == (byte)VoxelBlockID.AIR_BLOCK)
                        )
                    )
                ) {
                    return true;
                }
                break;

        }

        return false;
    }

    public static void Behave(VoxelState voxel)
    {
        switch (voxel.id)
        {
            case (byte)VoxelBlockID.GRASS_BLOCK: // Grass
                GrassBehaviour(voxel);
                break;
            case (byte)VoxelBlockID.WATER_BLOCK:
                WaterBehaviour(voxel);
                break;
            case (byte)VoxelBlockID.FURNANCE:
                FurnanceBehaviour(voxel);
                break;
            case (byte)VoxelBlockID.FURNANCE_LIT:
                FurnanceLitBehaviour(voxel);
                break;
            case (byte)VoxelBlockID.OAK_SAPLING:
                SaplingBehaviour(voxel);
                break;
        }

    }

    private static void GrassBehaviour(VoxelState voxel)
    {
        if (voxel.neighbours[2] != null && voxel.neighbours[2].id != (byte)VoxelBlockID.AIR_BLOCK)
        {
            voxel.chunkData.chunk.RemoveActiveVoxel(voxel);
            voxel.chunkData.ModifyVoxel(voxel.position, (byte)VoxelBlockID.DIRT_BLOCK, 0);
            return;
        }

        if (Random.Range(0.0f, 1.0f) > VoxelData.GrassSpreadSpeed) return;

        List<VoxelState> neighbours = new List<VoxelState>();
        if (voxel.neighbours[0] != null && voxel.neighbours[0].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[0])) neighbours.Add(voxel.neighbours[0]);
        if (voxel.neighbours[1] != null && voxel.neighbours[1].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[1])) neighbours.Add(voxel.neighbours[1]);
        if (voxel.neighbours[4] != null && voxel.neighbours[4].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[4])) neighbours.Add(voxel.neighbours[4]);
        if (voxel.neighbours[5] != null && voxel.neighbours[5].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[5])) neighbours.Add(voxel.neighbours[5]);
        if (voxel.neighbours[0] != null && voxel.neighbours[0].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[0].neighbours[3] != null && voxel.neighbours[0].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) neighbours.Add(voxel.neighbours[0].neighbours[3]);
        if (voxel.neighbours[1] != null && voxel.neighbours[1].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[1].neighbours[3] != null && voxel.neighbours[1].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) neighbours.Add(voxel.neighbours[1].neighbours[3]);
        if (voxel.neighbours[4] != null && voxel.neighbours[4].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[4].neighbours[3] != null && voxel.neighbours[4].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) neighbours.Add(voxel.neighbours[4].neighbours[3]);
        if (voxel.neighbours[5] != null && voxel.neighbours[5].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[5].neighbours[3] != null && voxel.neighbours[5].neighbours[3].id == (byte)VoxelBlockID.DIRT_BLOCK) neighbours.Add(voxel.neighbours[5].neighbours[3]);
        if (voxel.neighbours[0] != null && voxel.neighbours[0].neighbours[2] != null && voxel.neighbours[0].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[0].neighbours[2])) neighbours.Add(voxel.neighbours[0].neighbours[2]);
        if (voxel.neighbours[1] != null && voxel.neighbours[1].neighbours[2] != null && voxel.neighbours[1].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[1].neighbours[2])) neighbours.Add(voxel.neighbours[1].neighbours[2]);
        if (voxel.neighbours[4] != null && voxel.neighbours[4].neighbours[2] != null && voxel.neighbours[4].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[4].neighbours[2])) neighbours.Add(voxel.neighbours[4].neighbours[2]);
        if (voxel.neighbours[5] != null && voxel.neighbours[5].neighbours[2] != null && voxel.neighbours[5].neighbours[2].id == (byte)VoxelBlockID.DIRT_BLOCK && IsEmptyAbove(voxel.neighbours[5].neighbours[2])) neighbours.Add(voxel.neighbours[5].neighbours[2]);

        if (neighbours.Count == 0) return;

        int index = Random.Range(0, neighbours.Count);
        neighbours[index].chunkData.ModifyVoxel(neighbours[index].position, (byte)VoxelBlockID.GRASS_BLOCK, 0);
    }

    private static void WaterBehaviour(VoxelState voxel)
    {
        if (voxel.neighbours[0] != null && (voxel.neighbours[0].id == (byte)VoxelBlockID.AIR_BLOCK))
        {
            // Flow front
            voxel.neighbours[0].chunkData.ModifyVoxel(voxel.neighbours[0].position, (byte)VoxelBlockID.WATER_BLOCK, 0);
        }
        if (voxel.neighbours[1] != null && (voxel.neighbours[1].id == (byte)VoxelBlockID.AIR_BLOCK))
        {
            // Flow back 
            voxel.neighbours[1].chunkData.ModifyVoxel(voxel.neighbours[1].position, (byte)VoxelBlockID.WATER_BLOCK, 0);

        }
        if (voxel.neighbours[3] != null && (voxel.neighbours[3].id == (byte)VoxelBlockID.AIR_BLOCK))
        {
            // Flow Down
            voxel.neighbours[3].chunkData.ModifyVoxel(voxel.neighbours[3].position, (byte)VoxelBlockID.WATER_BLOCK, 0);

        }
        if (voxel.neighbours[4] != null && (voxel.neighbours[4].id == (byte)VoxelBlockID.AIR_BLOCK))
        {
            // Flow Left
            voxel.neighbours[4].chunkData.ModifyVoxel(voxel.neighbours[4].position, (byte)VoxelBlockID.WATER_BLOCK, 0);

        }
        if (voxel.neighbours[5] != null && (voxel.neighbours[5].id == (byte)VoxelBlockID.AIR_BLOCK))
        {
            // Flow Right
            voxel.neighbours[5].chunkData.ModifyVoxel(voxel.neighbours[5].position, (byte)VoxelBlockID.WATER_BLOCK, 0);

        }
    }

    public static bool Spawnable(VoxelState voxel)
    {
        return voxel.properties.isSpawnable &&
            ((voxel.position.y == VoxelData.ChunkHeight - 1) ||
            (voxel.position.y == VoxelData.ChunkHeight - 2 && voxel.neighbours[2].id == (byte)VoxelBlockID.AIR_BLOCK) ||
            (voxel.neighbours[2].id == (byte)VoxelBlockID.AIR_BLOCK && voxel.neighbours[2].neighbours[2].id == (byte)VoxelBlockID.AIR_BLOCK));
    }

    public static void SpawnMob(VoxelState voxel)
    {
        if (EntitiesCounter.enemyCreaturesEntity.Count >= EntitiesCounter.mobCap) return;
        if (voxel.light > 7) return;
        if (Vector3.Distance(voxel.globalPosition, World.Instance.player.position) < EntitiesCounter.spawnMinDistance) return;
        if (Vector3.Distance(voxel.globalPosition, World.Instance.player.position) > EntitiesCounter.despawnDistance) return;        
        if (Random.Range(0.0f, 1.0f) > EntitiesCounter.voxelSpawnChance) return;


        int mobIndex = Random.Range(0, World.Instance.overworldMobs.Length);
        if (mobIndex >= World.Instance.overworldMobs.Length) return;
        GameObject.Instantiate(World.Instance.overworldMobs[mobIndex], new Vector3(voxel.globalPosition.x + 0.5f, voxel.position.y + 1, voxel.globalPosition.z + 0.5f), Quaternion.Euler(0, Random.Range(0f, 359.0f), 0));
    }

    public static void FurnanceBehaviour(VoxelState voxel)
    {
        Item toSmelt = voxel.itemsContained[0] != null ? World.Instance.itemTypes[voxel.itemsContained[0].ID] : null;

        if (
            voxel.itemsContained != null &&
            toSmelt != null &&
            voxel.itemsContained[1] != null && World.Instance.itemTypes[voxel.itemsContained[1].ID].isFuel &&
            (voxel.itemsContained[2] == null || (voxel.itemsContained[2].ID == (byte)toSmelt.targetSmeltItem && voxel.itemsContained[2].amount < World.Instance.itemTypes[(byte)toSmelt.targetSmeltItem].maxItemStack))
            )
        {
            voxel.chunkData.ModifyVoxel(voxel.position, (byte)VoxelBlockID.FURNANCE_LIT, voxel.orientation);
            voxel.smeltingTimer = 4f;
        }
    }

    public static void FurnanceLitBehaviour(VoxelState voxel)
    {
        Item toSmeltLit = (voxel.itemsContained != null && voxel.itemsContained[0] != null && voxel.itemsContained[0].amount > 0) ? World.Instance.itemTypes[voxel.itemsContained[0].ID] : null;
        Item fuel = (voxel.itemsContained != null && voxel.itemsContained[1] != null && voxel.itemsContained[1].amount > 0) ? World.Instance.itemTypes[voxel.itemsContained[1].ID] : null;
        voxel.remainingSmeltingTImer -= VoxelData.TickLength;
        if (
            voxel.remainingSmeltingTImer > 0 || (
                    voxel.itemsContained != null &&
                    toSmeltLit != null && toSmeltLit.isSmeltable &&
                    fuel != null && fuel.isFuel &&
                    (voxel.itemsContained[2] == null || (voxel.itemsContained[2].ID == (byte)toSmeltLit.targetSmeltItem && voxel.itemsContained[2].amount < World.Instance.itemTypes[(byte)toSmeltLit.targetSmeltItem].maxItemStack))
                )
            )
        {
            if (voxel.itemsContained[0] != null && voxel.itemsContained[0].amount > 0)
            {
                if (voxel.remainingSmeltingTImer <= 0 && fuel != null && fuel.isFuel)
                {
                    if (voxel.itemsContained[1] == null || voxel.itemsContained[1].amount <= 0)
                    {
                        voxel.itemsContained[1] = null;
                        World.Instance.furnanceInventoryMenu.GetComponent<ContainerInventory>().PopulateContainerUI(voxel);
                    }
                    else
                    {
                        voxel.remainingSmeltingTImer = fuel.fuelTimer;
                        voxel.maxRemainingSmeltingTImer = fuel.fuelTimer;
                        voxel.itemsContained[1].amount--;
                        if (voxel.itemsContained[1].amount <= 0) voxel.itemsContained[1] = null;
                        World.Instance.furnanceInventoryMenu.GetComponent<ContainerInventory>().PopulateContainerUI(voxel);
                    }

                }
                voxel.smeltingTimer -= VoxelData.TickLength;
                if (voxel.smeltingTimer < 0)
                {
                    voxel.smeltingTimer = 4f;
                    if (voxel.itemsContained[2] == null || voxel.itemsContained[2].amount == 0)
                    {
                        voxel.itemsContained[2] = new ItemStack((byte)toSmeltLit.targetSmeltItem, 1);
                    }
                    else
                    {
                        voxel.itemsContained[2].amount++;
                    }
                    if (voxel.itemsContained[0] != null && voxel.itemsContained[0].amount > 0)
                    {
                        voxel.itemsContained[0].amount--;
                    }
                    else
                    {
                        voxel.itemsContained[0] = null;
                    }
                    World.Instance.furnanceInventoryMenu.GetComponent<ContainerInventory>().PopulateContainerUI(voxel);

                }
            } 
            else
            {
                voxel.smeltingTimer = 4f;
                voxel.itemsContained[0] = null;
                World.Instance.furnanceInventoryMenu.GetComponent<ContainerInventory>().PopulateContainerUI(voxel);
            }
        }
        else
        {
            voxel.chunkData.ModifyVoxel(voxel.position, (byte)VoxelBlockID.FURNANCE, voxel.orientation);
        }
    }

    public static void SaplingBehaviour(VoxelState voxel)
    {
        if (Random.Range(0.0f, 1.0f) > VoxelData.SaplingGrowthChance) return;

        if (IsEmptyOrLeavesAbove(voxel) && (
                    voxel.neighbours[2] == null || (
                        IsEmptyOrLeavesAbove(voxel.neighbours[2]) &&
                        (voxel.neighbours[2].neighbours[0] == null || voxel.neighbours[2].neighbours[0].id == (byte)VoxelBlockID.AIR_BLOCK) &&
                        (voxel.neighbours[2].neighbours[1] == null || voxel.neighbours[2].neighbours[1].id == (byte)VoxelBlockID.AIR_BLOCK) &&
                        (voxel.neighbours[2].neighbours[4] == null || voxel.neighbours[2].neighbours[4].id == (byte)VoxelBlockID.AIR_BLOCK) &&
                        (voxel.neighbours[2].neighbours[5] == null || voxel.neighbours[2].neighbours[5].id == (byte)VoxelBlockID.AIR_BLOCK)
                        )
                    )
                )
        {
            int maxHeight = 2;
            VoxelState currentHeight = voxel.neighbours[2].neighbours[2];
            for (int i = maxHeight; i < VoxelData.OakMaxHeight; i++)
            {
                if (currentHeight.neighbours[2] != null && currentHeight.neighbours[2].id == (byte)VoxelBlockID.AIR_BLOCK) maxHeight++;
                else break;

                currentHeight = currentHeight.neighbours[2];
            }
            World.Instance.AddToModificationsList(Structure.MakeTree(voxel.globalPosition, Mathf.Clamp(maxHeight - 4, 1, maxHeight), maxHeight));
            voxel.chunkData.ModifyVoxel(voxel.position, (byte)VoxelBlockID.OAK_LOG, voxel.orientation);
        }
    }
}
