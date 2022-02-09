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
    public bool shortAnimation;
    public float animationSpeed = 400f;
    public bool animateOnce;

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

        if (animateOnce && !animate)
        {
            animatedObject.rotation = new Quaternion();
        }

        if (animatedObject != null && (animate || animateOnce))
        {
            animate = true;
            animatedObject.Rotate(new Vector3(axisRotation == "X" ? 1 : 0, axisRotation == "Y" ? 1 : 0, axisRotation == "Z" ? 1 : 0), animationSpeed * (sign ? 1 : -1) * Time.deltaTime * (animateOnce ? 0.8f : 1));

            float angleOnAxis = axisRotation == "X" ? animatedObject.localRotation.eulerAngles.x : (axisRotation == "Y" ? animatedObject.localRotation.eulerAngles.y : animatedObject.localRotation.eulerAngles.z);

            if (!shortAnimation)
            {
                if (angleOnAxis < 180f && ((animateOnce && angleOnAxis > Mathf.Clamp(downLimit - 20, 0, downLimit)) || (!animateOnce && angleOnAxis > downLimit)))
                {
                    sign = false;
                    if (animateOnce)
                    {
                        animateOnce = false;
                        animate = false;
                    }
                }
                if (angleOnAxis >= 180f && angleOnAxis < upLimit) sign = true;
            } 
            else
            {
                if (angleOnAxis < 180f && ((animateOnce && angleOnAxis > Mathf.Clamp(downLimit - 35, 0, downLimit)) || (!animateOnce && angleOnAxis > downLimit - 30)))
                {
                    sign = false;
                    if (animateOnce)
                    {
                        animateOnce = false;
                        animate = false;
                        shortAnimation = false;
                    }
                }
                if (angleOnAxis >= 180f && angleOnAxis < upLimit + 20) sign = true;
            }

        }
        else
        {
            sign = axisRotation == "X";
            animatedObject.rotation = new Quaternion();
        }
    }
}
