using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// variables to handle out the acceleration and deceleration of the player
    /// </summary>
    public float speed, accelerationRate = 0, maxSpeed = 250;
    /// <summary>
    /// variables to handle out the player's side movement and torquerotation
    /// </summary>
    public float sideSpeed,skewSpeed,accelerationSideRate = 0,accelerationSkewRate=0, maxSideSpeed = 125;
    /// <summary>
    /// empty transform to handle pivoting torque
    /// </summary>
    public Transform rotationPivot;
    Rigidbody rbd;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        rbd = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    public void Movement()
    {
        //getting the horizontal and vertical inputs that changes over time
        float h = Input.GetAxis("Horizontal") * Time.deltaTime * sideSpeed;
        float v = Input.GetAxis("Vertical") * Time.deltaTime * sideSpeed;


        if (v == 0 && h == 0)
        {
            if (accelerationSideRate > 0)
                accelerationSideRate -= sideSpeed * Time.deltaTime;
            if (accelerationSideRate < 0)
                accelerationSideRate = 0;
        }

        if (h != 0 || v != 0)
        {
            if (accelerationSideRate < maxSideSpeed)
                accelerationSideRate += sideSpeed * Time.deltaTime;
            if (accelerationSideRate > maxSideSpeed)
                accelerationSideRate = maxSideSpeed;
        }

        if (rbd.velocity.z > maxSpeed)
            rbd.velocity = new Vector3(rbd.velocity.x, rbd.velocity.y, rbd.velocity.z);
        if (accelerationSideRate > maxSideSpeed)
            accelerationSideRate = maxSideSpeed;

        if (Input.GetKey(KeyCode.J))
        {
            accelerationRate += speed * Time.deltaTime * 2;
            rbd.AddForce((transform.forward * accelerationRate), ForceMode.Acceleration);
            transform.RotateAround(rotationPivot.position, transform.up, h);
            transform.RotateAround(rotationPivot.position, transform.right, v);
            transform.RotateAround(rotationPivot.position, transform.forward, skewSpeed);
            // if (v > 0)
            //     transform.RotateAround(rotationPivot.position, new Vector3(v, h, 0), sideSpeed * Time.deltaTime * accelerationSideRate);
            // else if (v < 0)
            //     transform.RotateAround(rotationPivot.position, new Vector3(-v, h, 0), sideSpeed * Time.deltaTime * accelerationSideRate);
            rbd.AddForce(new Vector3(h, v, rbd.velocity.z), ForceMode.Acceleration);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            accelerationRate += speed * Time.deltaTime * 2;
            rbd.AddForce((-transform.forward * accelerationRate / 2), ForceMode.Acceleration);
            // transform.RotateAround(rotationPivot.position, new Vector3(-v, h, 0), sideSpeed * Time.deltaTime * accelerationSideRate);
            transform.RotateAround(rotationPivot.position, transform.up, h);
            transform.RotateAround(rotationPivot.position, transform.right, v);
            transform.RotateAround(rotationPivot.position, transform.forward, skewSpeed);
            rbd.AddForce(new Vector3(h, v, rbd.velocity.z), ForceMode.Acceleration);
        }
        
        if(Input.GetKey(KeyCode.Q)){
            skewSpeed+=accelerationSkewRate * Time.deltaTime;
        }
        else if(Input.GetKey(KeyCode.E)){
            skewSpeed-=accelerationSkewRate * Time.deltaTime;
        }
        else{
            skewSpeed = 0;
        }

        if (accelerationRate > 0)
        {
            accelerationRate -= speed * Time.deltaTime;
        }
        else if (accelerationRate < 0) accelerationRate = 0;

    }
}