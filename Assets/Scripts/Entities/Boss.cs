using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : EnemyEntity
{
    private float summonTime = 8;
    private float maxHealth = 200;

    public Image bossHealthBar;
    public GameObject fireball;

    protected override void Start()
    {
        base.Start();
        health = 200;
        bossHealthBar = World.Instance.bossHealthBar;
        bossHealthBar.transform.parent.gameObject.SetActive(true);
        invincible = true;
    }

    protected override void Update()
    {
        despawnTimerCheck = 1;
        if (summonTime > 0)
        {
            summonTime -= Time.deltaTime;
            bossHealthBar.rectTransform.localScale = new Vector3(1 - summonTime / 8, 1, 1);
            return;
        }
        invincible = false;
        base.Update();

        bossHealthBar.rectTransform.localScale = new Vector3(health / maxHealth, 1, 1);
    }

    protected override void Attack()
    {
        base.Attack();

        Projectile projectile = GameObject.Instantiate(fireball, this.transform.position + new Vector3(0, 0.67f * entityHeight, 0), this.transform.rotation).GetComponent<Projectile>();
        projectile.GetComponent<Projectile>().damage = damage;
        projectile.GetComponent<Projectile>().verticalMomentum = 0.5f;
        Quaternion rotation = Quaternion.LookRotation(target.transform.position - this.transform.position, Vector3.up);
        projectile.verticalMomentum = -18 * Mathf.Sin(rotation.eulerAngles.x * Mathf.Deg2Rad);
        projectile.SetVertical(Mathf.Cos(rotation.eulerAngles.x * Mathf.Deg2Rad));
        projectile.SetRotation(rotation);
    }


    private void OnDestroy()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.transform.parent.gameObject.SetActive(false);
        }
        if (health <= 0)
        {
            EntitiesCounter.enemyCreaturesEntity.Remove(this);
            int randomStringsDrop = Random.Range(10, 35);
            for (int i = 0; i < randomStringsDrop; ++i)
            {
                Item extraDrop = GameObject.Instantiate(World.Instance.itemTypes[(byte)ItemID.DIAMOND], this.transform.position + new Vector3(0, 1, 0), new Quaternion());
                extraDrop.verticalMomentum = Random.Range(2f, 6f);
                extraDrop.horizontal = Random.Range(-1.0f, 1.0f);
                extraDrop.vertical = Random.Range(-1.0f, 1.0f);
            }
        }
    }
}
