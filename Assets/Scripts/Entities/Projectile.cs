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
        CheckEnemyCollision();

        CalculateVelocity();

        if (front == 2 || back == 2 || left == 2 || right == 2 || isGrounded)
        {
            Destroy(this.gameObject);
            return;
        }


        existTimer -= Time.deltaTime;
        if (existTimer < 0) Destroy(this.gameObject);
        transform.GetChild(0).LookAt(this.transform.position + velocity);

        transform.Translate(velocity, Space.World);

    }

    protected virtual void CheckEnemyCollision()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.GetChild(0).forward;
        RaycastHit hitObject;
        if (Physics.Raycast(rayOrigin, rayDirection, out hitObject, 2))
        {
            EnemyEntity entityHit = hitObject.transform.GetComponent<EnemyEntity>();
            if (entityHit != null)
            {
                Vector3 knockbackDirection = transform.GetChild(0).forward;
                knockbackDirection.y = 0;
                entityHit.ChangeHealth(-damage, knockbackDirection.normalized);
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void CheckPlayerCollision()
    {
        Vector3 thisPosition = this.transform.position;
        Vector3 targetPosition = target.transform.position;

        if (targetPosition.x - target.entityWidth < thisPosition.x && thisPosition.x < targetPosition.x + target.entityWidth &&
            targetPosition.y <= thisPosition.y && thisPosition.y < targetPosition.y + target.entityHeight &&
            targetPosition.z - target.entityWidth < thisPosition.z && thisPosition.z < targetPosition.z + target.entityWidth)
        {
            target.HealthChange(-damage, (target.transform.position - this.transform.position).normalized);
            Destroy(this.gameObject);
        }
    }

    public void SetVertical(float amount)
    {
        vertical = amount;
    }

    public void SetRotation(Vector3 rotation)
    {
        this.transform.GetChild(0).rotation = Quaternion.Euler(rotation);
    }

    public void SetRotation(Quaternion rotation)
    {
        this.transform.GetChild(0).rotation = rotation;
    }
}
