using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : EnemyEntity
{
    protected override void Attack()
    {
        base.Attack();

        target.HealthChange(-damage);
    }
}
