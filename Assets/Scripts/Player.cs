using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Transform FPSCamera;
    private World world;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;

    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    public float sensitivity = 10f;
    public float crouchSpeed = 1f;
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.807f;

    public float playerWidth = 0.15f;
    public float playerHeight = 1.8f;

    private float verticalMomentum = 0;
    private bool jumpRequest;

    public Transform highLightBlock;
    public Transform placeBlock;

    public float checkIncrement = 0.1f;
    public float reach = 5;

    public Toolbar toolbar;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        FPSCamera = FindObjectOfType<Camera>().transform;
        world = FindObjectOfType<World>();


    }

    private void FixedUpdate()
    {
       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            world.inUI = !world.inUI;
        }

        if (!world.inUI)
        {
            GetPlayerInput();
            if (jumpRequest) Jump();

            CalculateVelocity();
            transform.Rotate(Vector3.up * mouseHorizontal * sensitivity);
            FPSCamera.Rotate(Vector3.right * -mouseVertical * sensitivity);

            transform.Translate(velocity, Space.World);
            PlaceCursorBlock();
        }

        

    }

    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.deltaTime * gravity;
        }

        float speed = (!isCrouching ? (isSprinting ? sprintSpeed : walkSpeed) : crouchSpeed);
        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * speed;

        velocity += Vector3.up * verticalMomentum * Time.deltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
        }
        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else
        {
            if (velocity.y > 0)
            {
                velocity.y = CheckUpSpeed(velocity.y);
            }
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        } 
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = true;
        }
        if (Input.GetButtonUp("Crouch"))
        {
            isCrouching = false;
        }
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        if (highLightBlock.gameObject.activeSelf)
        {
            // Destroy block
            if (Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highLightBlock.position).EditVoxel(highLightBlock.position, 0);
            }

            // Place block
            if (Input.GetMouseButtonDown(1))
            {
                if (toolbar.uiItemSlots[toolbar.slotIndex].HasItem)
                {
                    world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.uiItemSlots[toolbar.slotIndex].itemSlot.stack.ID);
                    toolbar.uiItemSlots[toolbar.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
            )
        {
            isGrounded = true;
            return 0;
        }
        isGrounded = false;
        return downSpeed;
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.9f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.9f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.9f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.9f + upSpeed, transform.position.z + playerWidth))
            )
        {
            return 0;
        }
        return upSpeed;
    }

    public bool front
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
            {
                return true;
            }
            return false;
        }
    }

    public bool back
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
            {
                return true;
            }
            return false;
        }
    }

    public bool left
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
            {
                return true;
            }
            return false;
        }
    }

    public bool right
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
            {
                return true;
            }
            return false;
        }
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = FPSCamera.position + (FPSCamera.forward * step);
            if (world.CheckForVoxel(pos))
            {
                highLightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highLightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
        highLightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }
}
