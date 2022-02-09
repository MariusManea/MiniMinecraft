using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : Entity
{
    public float detectionRange = 30f;
    public float attackRange = 1f;
    public float attackTime = 1f;

    protected bool targetInRange = false;
    protected bool targetInAttackRange = false;
    protected float searchTime = 1f;
    protected float attackTimeCounter = 2f;

    protected float despawnTimerCheck = 1f;

    public Player target;

    public float damage;
    public Animator enemyAnimator;

    private Vector3 patrolDisplacement;
    private bool patrolDisplacementX;
    private bool patrolDisplacementZ;
    private float verticalPatrol;
    protected bool invincible = false;

    protected override void Awake()
    {
        base.Awake();
        EntitiesCounter.enemyCreaturesEntity.Add(this);
    }

    protected override void Start()
    {
        base.Start();

        target = FindObjectOfType<Player>();
    }

    protected override void Update()
    {
        if (despawnTimerCheck < 0)
        {
            despawnTimerCheck = 1f;
            float distance = Vector3.Distance(target.transform.position, this.transform.position);
            if (distance > EntitiesCounter.despawnDistance || distance > VoxelData.ChunkWidth * World.Instance.settings.viewDistance)
            {
                Destroy(this.gameObject);
                return;
            }
            if (distance > EntitiesCounter.despawnRandomMinDistance)
            {
                if (Random.Range(0.0f, 1.0f) < EntitiesCounter.despawnRandomChance)
                {
                    Destroy(this.gameObject);
                    return;
                }
            }
        }
        else {
            despawnTimerCheck -= Time.deltaTime;
        }

        base.Update();

        horizontal = 0;
        vertical = 0;

        if (!world.inUI)
        {
            if (!targetInRange)
            {
                if (searchTime < 0)
                {
                    LookForPlayer();
                }
                else
                {
                    searchTime -= Time.deltaTime;
                }
                vertical = verticalPatrol;
            }
            else
            {
                if (!targetInAttackRange)
                {
                    RushToPlayer();
                }
                else
                {
                    AttackPlayer();
                }
            }

            if (jumpRequest) Jump();

            CalculateVelocity();
            if (velocity.x != 0 || velocity.z != 0)
            {
                enemyAnimator.SetBool("Walking", true);
            }
            else
            {
                enemyAnimator.SetBool("Walking", false);
            }

            patrolDisplacement.x += Random.Range(0.0f, 2.0f) * Time.deltaTime * (patrolDisplacementX ? 1 : -1);
            patrolDisplacement.z += Random.Range(0.0f, 2.0f) * Time.deltaTime * (patrolDisplacementZ ? 1 : -1);
            if (patrolDisplacement.x > 1) { patrolDisplacement.x = 1; patrolDisplacementX = !patrolDisplacementX; }
            if (patrolDisplacement.x < -1) { patrolDisplacement.x = -1; patrolDisplacementX = !patrolDisplacementX; }
            if (patrolDisplacement.z > 1) { patrolDisplacement.z = 1; patrolDisplacementZ = !patrolDisplacementZ; }
            if (patrolDisplacement.z < -1) { patrolDisplacement.z = -1; patrolDisplacementZ = !patrolDisplacementZ; }

            RotateTowardsTargetPoint(targetInRange ? target.transform.position : new Vector3(transform.position.x + patrolDisplacement.x, 0, transform.position.z + patrolDisplacement.z));

            transform.Translate(velocity, Space.World);
        }
    }

    private void RotateTowardsTargetPoint(Vector3 targetPoint)
    {
        Vector3 direction = (targetPoint - this.transform.position);
        direction.y = 0;
        direction = direction.normalized;

        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.rotation = lookRotation;
    }

    protected virtual void LookForPlayer()
    {
        searchTime = 1f;

        if (Vector3.Distance(this.transform.position, target.transform.position) < detectionRange)
        {
            targetInRange = true;
        } 
        else
        {
            verticalPatrol = Mathf.Clamp(verticalPatrol + Random.Range(-0.4f, 0.4f), 0, 1);
        }
    }

    protected virtual void RushToPlayer()
    {
        float distance = Vector3.Distance(this.transform.position, target.transform.position);
        if (distance > detectionRange)
        {
            targetInRange = false;
            searchTime = 1f;
            return;
        }

        if (distance < 0.67f * attackRange)
        {
            targetInAttackRange = true;
            return;
        }

        Vector3 direction = target.transform.position - this.transform.position;
        direction.y = 0;
        direction = this.transform.InverseTransformDirection(direction.normalized);
        horizontal = direction.x;
        vertical = direction.z;

        if (isGrounded && (front == 2 || back == 2 || left == 2 || right == 2))
        {
            jumpRequest = true;
        }
    }

    protected virtual void AttackPlayer()
    {
        if (Vector3.Distance(this.transform.position, target.transform.position) > attackRange)
        {
            targetInAttackRange = false;
            return;
        }

        if (attackTimeCounter < 0)
        {
            // Attack
            attackTimeCounter = attackTime;
            if (ClearLineOfSight())
            {
                Attack();
            }
        } 
        else
        {
            attackTimeCounter -= Time.deltaTime;
        }

    }

    protected virtual void Attack()
    {
        enemyAnimator.SetTrigger("Attack");
    }

    private void OnDestroy()
    {
        EntitiesCounter.enemyCreaturesEntity.Remove(this.GetComponent<Entity>());
    }

    protected virtual bool ClearLineOfSight()
    {
        Vector3 checkPosition = this.transform.position + new Vector3(0, entityHeight, 0);
        Vector3 targetPosition = this.transform.position + new Vector3(0, target.entityHeight, 0);
        while(Vector3.Distance(checkPosition, targetPosition) >= 0.5f)
        {
            VoxelState voxel = World.Instance.GetVoxelState(checkPosition);
            if (voxel != null && voxel.properties.isSolid && !(voxel.properties.isLiquid))
            {
                return false;
            }
            checkPosition += (targetPosition - checkPosition) * 0.5f;
        }
        return true;
    }

    public virtual void ChangeHealth(float amount, Vector3 knockbackDirection)
    {
        if (!invincible)
        {
            health += amount;

            if (amount < 0)
            {
                verticalMomentum = 4;
                knockbackDirection.z = -knockbackDirection.z;
                Quaternion diff = Quaternion.LookRotation(knockbackDirection - transform.forward, Vector3.up);
                horizontalMomentum = diff * knockbackDirection;
            }

            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
