using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : EnemyEntity
{
    protected override void Attack()
    {
        base.Attack();

        target.HealthChange(-damage, (target.transform.position - this.transform.position).normalized);
    }

    private void OnDestroy()
    {
        if (health <= 0)
        {
            EntitiesCounter.enemyCreaturesEntity.Remove(this);
            int randomStringsDrop = Random.Range(0, 3);
            for (int i = 0; i < randomStringsDrop; ++i)
            {
                float dropValue = Random.Range(0.0f, 1.0f);
                ItemID extraItem = dropValue < 0.3f ? ItemID.STRING : dropValue < 0.5f ? ItemID.CARROT : dropValue < 0.7 ? ItemID.POTATO : ItemID.WHEAT_SEEDS;
                Item extraDrop = GameObject.Instantiate(World.Instance.itemTypes[(byte)extraItem], this.transform.position + new Vector3(0, 1, 0), new Quaternion());
                extraDrop.verticalMomentum = Random.Range(2f, 6f);
                extraDrop.horizontal = Random.Range(-1.0f, 1.0f);
                extraDrop.vertical = Random.Range(-1.0f, 1.0f);
            }
        }
    }
}
