using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public static ulong uniqueEntityIDCounter = 0;
    public ulong uniqueEntityID;

    protected World world;

    public float horizontal;
    public float vertical;
    public float rotateHorizontal;
    public float rotateVertical;
    protected Vector3 velocity;

    public bool isGrounded;
    protected bool isSprinting;
    protected bool isCrouching;

    public float crouchSpeed = 1f;
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -13.5f;

    public float entityWidth = 0.25f;
    public float deltaWidth = 0.2f;
    public float entityHeight = 1.85f;

    public float verticalMomentum = 0;
    public Vector3 horizontalMomentum = Vector3.zero;
    protected bool jumpRequest;

    public int orientation;

    public float health = 20;

    protected virtual void Awake()
    {
        uniqueEntityID = uniqueEntityIDCounter++;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        world = FindObjectOfType<World>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    protected virtual void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.deltaTime * gravity;
        }

        float speed = (!isCrouching ? (isSprinting ? sprintSpeed : walkSpeed) : crouchSpeed);

        if (horizontalMomentum.magnitude != 0)
        {
            velocity = ((transform.forward * horizontalMomentum.z) + (transform.right * horizontalMomentum.x) * Time.deltaTime * sprintSpeed);
            horizontalMomentum /= 1.2f;
            if (horizontalMomentum.magnitude < 0.005f) horizontalMomentum = Vector3.zero;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * speed;
        }

        velocity += Vector3.up * verticalMomentum * Time.deltaTime;

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else
        {
            if (velocity.y > 0)
            {
                velocity.y = CheckUpSpeed(velocity.y);
                if (velocity.y == 0) verticalMomentum = 0;
            }
        }


        if ((velocity.z > 0 && front != 0) || (velocity.z < 0 && back != 0))
        {
            if ((velocity.z > 0 && front == 4) || (velocity.z < 0 && back == 4))
            {
                //velocity.y = 0.55f;
            }
            else
            {
                if ((velocity.z > 0 && front == 1) || (velocity.z < 0 && back == 1)) velocity.z /= 2;
                else { velocity.z = 0; horizontalMomentum.z = 0; }
            }
        }
        if ((velocity.x > 0 && right != 0) || (velocity.x < 0 && left != 0))
        {
            if ((velocity.x > 0 && right == 4) || (velocity.x < 0 && left == 4))
            {
                //velocity.y = 0.55f;
            }
            else
            {
                if ((velocity.x > 0 && right == 1) || (velocity.x < 0 && left == 1)) velocity.x /= 2;
                else { velocity.x = 0; horizontalMomentum.x = 0; }
            }
        }

    }

    protected virtual float CheckDownSpeed(float downSpeed)
    {
        float nextY = transform.position.y + downSpeed;

        VoxelState LB = world.GetVoxelState(new Vector3(transform.position.x - deltaWidth, nextY, transform.position.z - deltaWidth));
        if (LB != null && world.blockTypes[LB.id].isSolid)
        {
            if (!(world.blockTypes[LB.id].isLiquid))
            {
                float blockHeight = LB.getHeightAt(transform.position.x, transform.position.z);
                if (blockHeight == 1 || (nextY - Mathf.FloorToInt(nextY) < blockHeight))
                {
                    isGrounded = true;
                    return 0;
                }
            }
            else
            {
                isGrounded = transform.position.y - Mathf.FloorToInt(transform.position.y) < 0.7f;
                return downSpeed / 4;
            }
        }

        VoxelState RB = world.GetVoxelState(new Vector3(transform.position.x + deltaWidth, nextY, transform.position.z - deltaWidth));
        if (RB != null && world.blockTypes[RB.id].isSolid)
        {
            if (!(world.blockTypes[RB.id].isLiquid))
            {
                float blockHeight = RB.getHeightAt(transform.position.x, transform.position.z);
                if (blockHeight == 1 || (nextY - Mathf.FloorToInt(nextY) < blockHeight))
                {
                    isGrounded = true;
                    return 0;
                }
            }
            else
            {
                isGrounded = transform.position.y - Mathf.FloorToInt(transform.position.y) < 0.65f;
                return downSpeed / 4;
            }
        }

        VoxelState LF = world.GetVoxelState(new Vector3(transform.position.x - deltaWidth, nextY, transform.position.z + deltaWidth));
        if (LF != null && world.blockTypes[LF.id].isSolid)
        {
            if (!(world.blockTypes[LF.id].isLiquid))
            {
                float blockHeight = LF.getHeightAt(transform.position.x, transform.position.z);
                if (blockHeight == 1 || (nextY - Mathf.FloorToInt(nextY) < blockHeight))
                {
                    isGrounded = true;
                    return 0;
                }
            }
            else
            {
                isGrounded = transform.position.y - Mathf.FloorToInt(transform.position.y) < 0.65f;
                return downSpeed / 4;
            }
        }

        VoxelState RF = world.GetVoxelState(new Vector3(transform.position.x + deltaWidth, nextY, transform.position.z + deltaWidth));
        if (RF != null && world.blockTypes[RF.id].isSolid)
        {
            if (!(world.blockTypes[RF.id].isLiquid))
            {
                float blockHeight = RF.getHeightAt(transform.position.x, transform.position.z);
                if (blockHeight == 1 || (nextY - Mathf.FloorToInt(nextY) < blockHeight))
                {
                    isGrounded = true;
                    return 0;
                }
            }
            else
            {
                isGrounded = transform.position.y - Mathf.FloorToInt(transform.position.y) < 0.65f;
                return downSpeed / 4;
            }
        }

        isGrounded = false;
        return downSpeed;
    }

    protected virtual float CheckUpSpeed(float upSpeed)
    {
        VoxelState LB = world.GetVoxelState(new Vector3(transform.position.x - deltaWidth, transform.position.y + (entityHeight > 1 ? 1.975f : entityHeight), transform.position.z - deltaWidth));
        VoxelState RB = world.GetVoxelState(new Vector3(transform.position.x + deltaWidth, transform.position.y + (entityHeight > 1 ? 1.975f : entityHeight), transform.position.z - deltaWidth));
        VoxelState LF = world.GetVoxelState(new Vector3(transform.position.x - deltaWidth, transform.position.y + (entityHeight > 1 ? 1.975f : entityHeight), transform.position.z + deltaWidth));
        VoxelState RF = world.GetVoxelState(new Vector3(transform.position.x + deltaWidth, transform.position.y + (entityHeight > 1 ? 1.975f : entityHeight), transform.position.z + deltaWidth));

        if (
            (LB != null && world.blockTypes[LB.id].isSolid) ||
            (RB != null && world.blockTypes[RB.id].isSolid) ||
            (LF != null && world.blockTypes[LF.id].isSolid) ||
            (RF != null && world.blockTypes[RF.id].isSolid)
            )
        {
            if ((LB != null && world.blockTypes[LB.id].isLiquid) ||
                (RB != null && world.blockTypes[RB.id].isLiquid) ||
                (LF != null && world.blockTypes[LF.id].isLiquid) ||
                (RF != null && world.blockTypes[RF.id].isLiquid)) return upSpeed / 3;
            return 0;
        }
        return upSpeed;
    }

    protected bool LateralLiquidBlocksCheck(VoxelState relative, VoxelState target1, VoxelState target2)
    {
        return (relative != null && world.blockTypes[relative.id].isLiquid) &&
                        (target1 == null || (world.blockTypes[target1.id].isLiquid || !(world.blockTypes[target1.id].isSolid))) &&
                        (target2 == null || (world.blockTypes[target2.id].isLiquid || !(world.blockTypes[target2.id].isSolid)));
    }

    protected bool LateralNonLiquidBlocksCheck(VoxelState relative, VoxelState target1, VoxelState target2)
    {
        return (relative != null && !(world.blockTypes[relative.id].isLiquid)) &&
                        (target1 == null || (world.blockTypes[target1.id].isLiquid || !(world.blockTypes[target1.id].isSolid))) &&
                        (target2 == null || (world.blockTypes[target2.id].isLiquid || !(world.blockTypes[target2.id].isSolid)));
    }

    protected virtual byte front
    {
        get
        {
            float nextZ = transform.position.z + entityWidth;
            VoxelState F = world.GetVoxelState(new Vector3(transform.position.x, transform.position.y, nextZ));
            VoxelState F1 = world.GetVoxelState(new Vector3(transform.position.x, transform.position.y + Mathf.Min(1f, entityHeight), nextZ));
            VoxelState FH = world.GetVoxelState(new Vector3(transform.position.x, transform.position.y + entityHeight, nextZ));

            if (
                F != null && world.blockTypes[F.id].isSolid ||
                F1 != null && world.blockTypes[F1.id].isSolid ||
                FH != null && world.blockTypes[FH.id].isSolid
                )
            {
                if (LateralNonLiquidBlocksCheck(F, F1, FH))
                {
                    float blockHeight = F.getHeightAt(transform.position.x, nextZ);
                    float heightDiff = blockHeight - (transform.position.y - Mathf.FloorToInt(transform.position.y));
                    if (Mathf.Abs(heightDiff) < 0.55f)
                    {
                        if (isGrounded)
                        {
                            velocity.y = heightDiff;
                        }
                        return 4; // climb same block
                    }
                }
                if (
                    LateralLiquidBlocksCheck(F, F1, FH) ||
                    LateralLiquidBlocksCheck(F1, F, FH) ||
                    LateralLiquidBlocksCheck(FH, F, F1)
                    )
                {
                    return 1; // liquid block ahead
                }
                return 2; // solid block ahead (move impossible)
            }
            return 0; // free to go
        }
    }

    protected virtual byte back
    {
        get
        {
            float nextZ = transform.position.z - entityWidth;
            VoxelState B = world.GetVoxelState(new Vector3(transform.position.x, transform.position.y, nextZ));
            VoxelState B1 = world.GetVoxelState(new Vector3(transform.position.x, transform.position.y + Mathf.Min(1f, entityHeight), nextZ));
            VoxelState BH = world.GetVoxelState(new Vector3(transform.position.x, transform.position.y + entityHeight, nextZ));

            if (
                B != null && world.blockTypes[B.id].isSolid ||
                B1 != null && world.blockTypes[B1.id].isSolid ||
                BH != null && world.blockTypes[BH.id].isSolid
                )
            {
                if (LateralNonLiquidBlocksCheck(B, B1, BH))
                {
                    float blockHeight = B.getHeightAt(transform.position.x, nextZ);
                    float heightDiff = blockHeight - (transform.position.y - Mathf.FloorToInt(transform.position.y));
                    if (Mathf.Abs(heightDiff) < 0.55f)
                    {
                        if (isGrounded)
                        {
                            velocity.y = heightDiff;
                        }
                        return 4;
                    }
                }
                if (
                    LateralLiquidBlocksCheck(B, B1, BH) ||
                    LateralLiquidBlocksCheck(B1, B, BH) ||
                    LateralLiquidBlocksCheck(BH, B, B1)
                    )
                {
                    return 1;
                }
                return 2;
            }
            return 0;
        }
    }

    protected virtual byte left
    {
        get
        {
            float nextX = transform.position.x - entityWidth;
            VoxelState L = world.GetVoxelState(new Vector3(nextX, transform.position.y, transform.position.z));
            VoxelState L1 = world.GetVoxelState(new Vector3(nextX, transform.position.y + Mathf.Min(1f, entityHeight), transform.position.z));
            VoxelState LH = world.GetVoxelState(new Vector3(nextX, transform.position.y + entityHeight, transform.position.z));

            if (
                L != null && world.blockTypes[L.id].isSolid ||
                L1 != null && world.blockTypes[L1.id].isSolid ||
                LH != null && world.blockTypes[LH.id].isSolid
                )
            {
                if (LateralNonLiquidBlocksCheck(L, L1, LH))
                {
                    float blockHeight = L.getHeightAt(nextX, transform.position.z);
                    float heightDiff = blockHeight - (transform.position.y - Mathf.FloorToInt(transform.position.y));
                    if (Mathf.Abs(heightDiff) < 0.55f)
                    {
                        if (isGrounded)
                        {
                            velocity.y = heightDiff;
                        }
                        return 4;
                    }
                }
                if (
                    LateralLiquidBlocksCheck(L, L1, LH) ||
                    LateralLiquidBlocksCheck(L1, L, LH) ||
                    LateralLiquidBlocksCheck(LH, L, L1)
                    )
                {
                    return 1;
                }
                return 2;
            }
            return 0;
        }
    }

    protected virtual byte right
    {
        get
        {
            float nextX = transform.position.x + entityWidth;
            VoxelState R = world.GetVoxelState(new Vector3(nextX, transform.position.y, transform.position.z));
            VoxelState R1 = world.GetVoxelState(new Vector3(nextX, transform.position.y + Mathf.Min(1f, entityHeight), transform.position.z));
            VoxelState RH = world.GetVoxelState(new Vector3(nextX, transform.position.y + entityHeight, transform.position.z));

            if (
                R != null && world.blockTypes[R.id].isSolid ||
                R1 != null && world.blockTypes[R1.id].isSolid ||
                RH != null && world.blockTypes[RH.id].isSolid
                )
            {
                if (LateralNonLiquidBlocksCheck(R, R1, RH))
                {
                    float blockHeight = R.getHeightAt(nextX, transform.position.z);
                    float heightDiff = blockHeight - (transform.position.y - Mathf.FloorToInt(transform.position.y));
                    if (Mathf.Abs(heightDiff) < 0.55f)
                    {
                        if (isGrounded)
                        {
                            velocity.y = heightDiff;
                        }
                        return 4;
                    }
                }
                if (
                    LateralLiquidBlocksCheck(R, R1, RH) ||
                    LateralLiquidBlocksCheck(R1, R, RH) ||
                    LateralLiquidBlocksCheck(RH, R, R1))
                {
                    return 1;
                }
                return 2;
            }
            return 0;
        }
    }
}
