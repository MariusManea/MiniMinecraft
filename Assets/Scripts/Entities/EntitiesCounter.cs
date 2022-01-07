using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntitiesCounter
{
    public static float spawnMinDistance = 10;
    public static float voxelSpawnChance = 0.1f;
    public static float despawnDistance = 35;
    public static float despawnRandomMinDistance = 25;
    public static float despawnRandomChance = 0.05f;

    public static float mobCap = 20;

    public static Entity playerEntity;

    public static List<Entity> itemsEntity = new List<Entity>();

    public static List<Entity> enemyCreaturesEntity = new List<Entity>();

    public static List<Entity> friendlyCreaturesEntity = new List<Entity>();
}
