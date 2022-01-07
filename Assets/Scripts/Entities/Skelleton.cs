using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skelleton : EnemyEntity
{
    public GameObject flyingArrow;
    
    protected override void Attack()
    {
        base.Attack();

        GameObject projectile = GameObject.Instantiate(flyingArrow, this.transform.position + new Vector3(0, 0.67f * entityHeight, 0), this.transform.rotation);
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().verticalMomentum = 0.5f;
    }
}
