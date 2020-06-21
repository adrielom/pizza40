using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MoveType{
    reverse,
    forward
}
public class MovementManager : MonoBehaviour
{
    /// <summary>
    /// variables to handle out the acceleration and deceleration of the player
    /// </summary>
    public float maxSpeed = 250;

    [SerializeField]
    private float speed = 0;
    /// <summary>
    /// variables to handle out the player's side movement and torquerotation
    /// </summary>
    public float sideYSpeed,sideXSpeed, maxSideSpeed = 125;
    private float accelerationYrate = 0,accelerationXRate=0;
    /// <summary>
    /// empty transform to handle pivoting torque
    /// </summary>


    [SerializeField]
    private float accelCurveCoeff;
    [SerializeField]
    private float accelRate, backwardAccelRate;
    public Transform rotationPivot;

    private MoveType moveType;


    ///<summary>
    ///Allows touch controls or key controls on true.
    ///</summary>
    [SerializeField]
    private bool devMode; 


    ///<summary>
    ///Touch joystick.
    ///</summary>
    
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
        Movement(BaseInput.Instance.ForwardInput, BaseInput.Instance.VerticalInput, BaseInput.Instance.HorizontalInput);
    }

    public void Movement(float forward, float vertical, float horizontal)
    {
        //getting the horizontal and vertical inputs that changes over time
        float horizontalRatio = horizontal * Time.deltaTime * sideYSpeed;
        float verticalRatio = vertical * Time.deltaTime * sideXSpeed;


        DetermineSideRatio(verticalRatio, horizontalRatio);
        CalcForwardSpeed(forward);
        TurnTransform();

        // float accelPower =  forward>= 0 ? accelRate : backwardAccelRate;

        // float accelRamp = rbd.velocity.magnitude/maxSpeed;
        // float accelFinalRamp = Mathf.Lerp(accelCurveCoeff, 1, accelRamp * accelRamp);

        // float finalAcceleration = accelPower * accelFinalRamp;

        

        // if (verticalRatio == 0 && horizontalRatio == 0)
        // {
        //     if (accelerationSideRate > 0)
        //         accelerationSideRate -= sideSpeed * Time.deltaTime;
        //     if (accelerationSideRate < 0)
        //         accelerationSideRate = 0;
        // }

        // if (horizontalRatio != 0 || verticalRatio != 0)
        // {
        //     if (accelerationSideRate < maxSideSpeed)
        //         accelerationSideRate += sideSpeed * Time.deltaTime;
        //     if (accelerationSideRate > maxSideSpeed)
        //         accelerationSideRate = maxSideSpeed;
        // }

        // if (rbd.velocity.magnitude > maxSpeed)
        //     rbd.velocity = rbd.velocity.normalized * maxSpeed;
        // if (accelerationSideRate > maxSideSpeed)
        //     accelerationSideRate = maxSideSpeed;

        // if (Input.GetKey(KeyCode.J))
        // {
        //     if(moveType != MoveType.forward){
        //         accelerationRate = 0;
        //         moveType = MoveType.forward;
        //     }
        //     accelerationRate += speed * Time.deltaTime * 2;
        //     rbd.AddForce((transform.forward * accelerationRate), ForceMode.Acceleration);
        //     transform.RotateAround(rotationPivot.position, transform.forward, skewSpeed);
        //     transform.RotateAround(rotationPivot.position, transform.right, verticalRatio);
        //     transform.RotateAround(rotationPivot.position, transform.up, horizontalRatio);
        //     // if (v > 0)
        //     //     transform.RotateAround(rotationPivot.position, new Vector3(v, h, 0), sideSpeed * Time.deltaTime * accelerationSideRate);
        //     // else if (v < 0)
        //     //     transform.RotateAround(rotationPivot.position, new Vector3(-v, h, 0), sideSpeed * Time.deltaTime * accelerationSideRate);
        //     rbd.AddForce(new Vector3(horizontalRatio, verticalRatio, rbd.velocity.z), ForceMode.Acceleration);
        // }
        // else if (Input.GetKey(KeyCode.K))
        // {
        //     if(moveType != MoveType.reverse){
        //         accelerationRate = 0;
        //         moveType = MoveType.reverse;
        //     }
        //     accelerationRate += speed * Time.deltaTime * 2;
        //     rbd.AddForce((-transform.forward * accelerationRate / 2), ForceMode.Acceleration);
        //     // transform.RotateAround(rotationPivot.position, new Vector3(-v, h, 0), sideSpeed * Time.deltaTime * accelerationSideRate);
        //     transform.RotateAround(rotationPivot.position, transform.forward, skewSpeed);
        //     transform.RotateAround(rotationPivot.position, transform.right, verticalRatio);
        //     transform.RotateAround(rotationPivot.position, transform.up, horizontalRatio);
        //     rbd.AddForce(new Vector3(horizontalRatio, verticalRatio, rbd.velocity.z), ForceMode.Acceleration);
        // }
        
        // if(Input.GetKey(KeyCode.Q)){
        //     skewSpeed-=accelerationSkewRate * Time.deltaTime;
        // }
        // else if(Input.GetKey(KeyCode.E)){
        //     skewSpeed+=accelerationSkewRate * Time.deltaTime;
        // }
        // else{
        //     skewSpeed = 0;
        // }

        // if (accelerationRate > 0)
        // {
        //     accelerationRate -= speed * Time.deltaTime;
        // }
        // else if (accelerationRate < 0) accelerationRate = 0;

    }


    private void DetermineSideRatio(float verticalRatio, float horizontalRatio){
        accelerationYrate = horizontalRatio != 0 ? accelerationYrate + (horizontalRatio * Time.deltaTime) : 0;
        accelerationXRate = verticalRatio != 0 ? accelerationXRate + (verticalRatio * Time.deltaTime) : 0;
        accelerationYrate = Mathf.Clamp(accelerationYrate, -maxSideSpeed, maxSideSpeed);
        accelerationXRate = Mathf.Clamp(accelerationXRate, -maxSideSpeed, maxSideSpeed);
    }
    
    private void CalcForwardSpeed(float forward){
        float accelPower =  forward>= 0 ? accelRate : backwardAccelRate;

        float accelRamp = rbd.velocity.magnitude/maxSpeed;
        float accelFinalRamp = Mathf.Lerp(accelCurveCoeff, 1, accelRamp * accelRamp);

        float finalAcceleration = accelPower * accelFinalRamp;


        Quaternion turnAngleX = Quaternion.AngleAxis(accelerationXRate, rbd.transform.right);
        Quaternion turnAngleY = Quaternion.AngleAxis(accelerationYrate, rbd.transform.up);

        Vector3 fwd = turnAngleX * rbd.transform.forward;
        fwd = turnAngleY * fwd;

        Vector3 movement = fwd * forward * finalAcceleration;

        Vector3 adjustedVelocity = rbd.velocity + movement * Time.deltaTime;
        
        if(adjustedVelocity.magnitude > maxSpeed){
            adjustedVelocity = Vector3.ClampMagnitude(adjustedVelocity, maxSpeed);
        }

        speed = adjustedVelocity.magnitude;
        rbd.velocity = adjustedVelocity;
    }

    private void TurnTransform(){
            transform.RotateAround(rotationPivot.position, transform.right, accelerationXRate);
            transform.RotateAround(rotationPivot.position, transform.up, accelerationYrate);
    }
}