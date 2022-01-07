using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolderAnimation : MonoBehaviour
{
    public string axisRotation;
    public float upLimit;
    public float downLimit;
    public Transform animatedObject;

    public bool animate;
    public float animationSpeed = 400f;

    private bool sign = false;

    // Start is called before the first frame update
    void Start()
    {
        sign = axisRotation == "X";
    }

    // Update is called once per frame
    void Update()
    {
        if (animatedObject == null) animatedObject = this.transform.GetChild(0);
        if (animatedObject != null && animate)
        {
            animatedObject.Rotate(new Vector3(axisRotation == "X" ? 1 : 0, axisRotation == "Y" ? 1 : 0, axisRotation == "Z" ? 1 : 0), animationSpeed * (sign ? 1 : -1) * Time.deltaTime);

            float angleOnAxis = axisRotation == "X" ? animatedObject.localRotation.eulerAngles.x : (axisRotation == "Y" ? animatedObject.localRotation.eulerAngles.y : animatedObject.localRotation.eulerAngles.z);
            if (angleOnAxis < 180f && angleOnAxis > downLimit) sign = false;
            if (angleOnAxis >= 180f && angleOnAxis < upLimit) sign = true;

        }
        else
        {
            sign = axisRotation == "X";
            animatedObject.rotation = new Quaternion();
        }
    }
}
