using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skelleton : EnemyEntity
{
    public GameObject flyingArrow;
    
    protected override void Attack()
    {
        base.Attack();

        Projectile projectile = GameObject.Instantiate(flyingArrow, this.transform.position + new Vector3(0, 0.67f * entityHeight, 0), this.transform.rotation).GetComponent<Projectile>();
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().verticalMomentum = 0.5f;
        Quaternion rotation = Quaternion.LookRotation(target.transform.position - this.transform.position, Vector3.up);
        projectile.verticalMomentum = -18 * Mathf.Sin(rotation.eulerAngles.x * Mathf.Deg2Rad);
        projectile.SetVertical(Mathf.Cos(rotation.eulerAngles.x * Mathf.Deg2Rad));
        projectile.SetRotation(rotation);
    }

    private void OnDestroy()
    {
        if (health <= 0)
        {
            EntitiesCounter.enemyCreaturesEntity.Remove(this);
            int randomArrowsDrop = Random.Range(3, 8);
            for (int i = 0; i < randomArrowsDrop; ++i)
            {
                Item extraDrop = GameObject.Instantiate(World.Instance.itemTypes[(byte)ItemID.ARROW], this.transform.position + new Vector3(0, 1, 0), new Quaternion());
                extraDrop.verticalMomentum = Random.Range(2f, 6f);
                extraDrop.horizontal = Random.Range(-1.0f, 1.0f);
                extraDrop.vertical = Random.Range(-1.0f, 1.0f);
            }
        }
    }
}
