using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    public Player target;
    public float damage;
    private float existTimer = 15f;
    protected override void Awake()
    {
        base.Awake();

        vertical = 1;
    }

    protected override void Start()
    {
        base.Start();
        target = FindObjectOfType<Player>();
        
    }

    protected override void Update()
    {
        base.Update();
        CheckPlayerCollision();

        CalculateVelocity();

        if (front == 2 || back == 2 || left == 2 || right == 2 || isGrounded)
        {
            Destroy(this.gameObject);
            return;
        }


        existTimer -= Time.deltaTime;
        if (existTimer < 0) Destroy(this.gameObject);

        transform.Translate(velocity, Space.World);
    }

    protected virtual void CheckPlayerCollision()
    {
        Vector3 thisPosition = this.transform.position;
        Vector3 targetPosition = target.transform.position;

        if (targetPosition.x - target.entityWidth < thisPosition.x && thisPosition.x < targetPosition.x + target.entityWidth &&
            targetPosition.y <= thisPosition.y && thisPosition.y < targetPosition.y + target.entityHeight &&
            targetPosition.z - target.entityWidth < thisPosition.z && thisPosition.z < targetPosition.z + target.entityWidth)
        {
            this.transform.SetParent(target.transform);
            target.HealthChange(-damage);
            Destroy(this.gameObject);
        }
    }
}
