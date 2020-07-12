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
    private float accelerationYrate = 0,accelerationXRate=0, accelerationZrate = 0;

    private float xRot, zRot;

    [SerializeField]
    private float maxXRot = 45, maxZRot = 90;

    [SerializeField]
    private float speedRotEffect = 1f;
    /// <summary>
    /// empty transform to handle pivoting torque
    /// </summary>


    [SerializeField]
    private float accelCurveCoeff=0;
    [SerializeField]
    private float accelRate=0, backwardAccelRate=0;
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
    void FixedUpdate()
    {
        Movement(BaseInput.Instance.ForwardInput, BaseInput.Instance.VerticalInput, BaseInput.Instance.HorizontalInput);
    }

    public void Movement(float forward, float vertical, float horizontal)
    {
        //getting the horizontal and vertical inputs that changes over time
        float horizontalRatio = horizontal * Time.deltaTime * sideYSpeed;
        float skewRatio = -horizontal * 2f;
        float skewYRatio = -vertical * 4f;
        float verticalRatio = vertical * Time.deltaTime * sideXSpeed;

        DetermineSideRatio(verticalRatio, horizontalRatio);
        CalcForwardSpeed(forward, vertical);
        TurnTransform();
        Skew(skewRatio, skewYRatio, BaseInput.Instance.FirstComboInput);

    }


    private void DetermineSideRatio(float verticalRatio, float horizontalRatio){
        accelerationYrate = horizontalRatio != 0 ? accelerationYrate + (horizontalRatio * Time.deltaTime) : 0;
        accelerationXRate = verticalRatio != 0 ? accelerationXRate + (verticalRatio * Time.deltaTime) : 0;
        // accelerationZrate = skewRatio != 0 ? accelerationZrate + (skewRatio * Time.deltaTime) : 0;
        accelerationYrate = Mathf.Clamp(accelerationYrate, -maxSideSpeed, maxSideSpeed);
        // accelerationZrate = Mathf.Clamp(accelerationZrate, -maxSideSpeed*0.5f, maxSideSpeed*0.5f);
        accelerationXRate = Mathf.Clamp(accelerationXRate, -maxSideSpeed, maxSideSpeed);
    }
    
    private void CalcForwardSpeed(float forward, float vertical){
        float accelPower =  forward>= 0 ? accelRate : backwardAccelRate;

        float accelRamp = rbd.velocity.magnitude/maxSpeed;
        float accelFinalRamp = Mathf.Lerp(accelCurveCoeff, 1, accelRamp * accelRamp);

        float finalAcceleration = accelPower * accelFinalRamp;


        Quaternion turnAngleY = Quaternion.AngleAxis(accelerationYrate, rbd.transform.up);

        Vector3 fwd = turnAngleY * rbd.transform.forward;
        
        
        /*Should be activated only if you want vertical speed */
        // Vector3 towards = accelerationXRate * rbd.transform.up;

        Vector3 movement = fwd * forward * finalAcceleration;

        Vector3 adjustedVelocity = rbd.velocity + /*+ towards +*/ movement * Time.deltaTime;
        
        if(adjustedVelocity.magnitude > maxSpeed){
            adjustedVelocity = Vector3.ClampMagnitude(adjustedVelocity, maxSpeed);
        }

        speed = adjustedVelocity.magnitude;
        rbd.velocity = adjustedVelocity;
    }

    private void TurnTransform(){
            // transform.RotateAround(rotationPivot.position, transform.right, accelerationXRate);
            transform.RotateAround(rotationPivot.position, Vector3.up, accelerationYrate);
    }

    private void Skew(float skewRatio, float skewYRatio, float stuntCoeff){



        /* --------- OLD CODE TO ALIGN CAR -------------------------------------------------
        // var groundNormal = Vector3.up;
        //Calculate the amount of pitch and roll the ship needs to match its orientation
		//with that of the ground. This is done by creating a projection and then calculating
		//the rotation needed to face that projection
		// Vector3 projection = Vector3.ProjectOnPlane(transform.forward, groundNormal);
		// Quaternion rotation = Quaternion.LookRotation(projection, groundNormal);

		//Move the ship over time to match the desired rotation to match the ground. This is 
		//done smoothly (using Lerp) to make it feel more realistic
		// rbd.MoveRotation(Quaternion.Lerp(rbd.rotation, rotation, Time.deltaTime * 5f));
        */


        float previousXRot = xRot;
        float previousZRot = zRot;

        float xRotRatio = maxXRot*0.01f*speedRotEffect;
        float zRotRatio = maxZRot*0.01f*speedRotEffect;

        xRot = skewYRatio!= 0 ? xRot + skewYRatio : xRot - (Mathf.Sign(xRot)*xRotRatio);
        zRot = skewRatio!= 0 ? zRot + skewRatio : zRot - (Mathf.Sign(zRot)*zRotRatio);
        
        xRot = Mathf.Clamp(xRot, -maxXRot, maxXRot);
        xRot = Mathf.Abs(xRot)>xRotRatio? xRot : 0;
        if(stuntCoeff != 0){
            zRot = Mathf.Clamp(zRot + skewRatio*2, -90f, 90f);
        }
        else{
            zRot = Mathf.Abs(zRot) > Mathf.Abs(previousZRot) ? Mathf.Clamp(zRot, -maxZRot, maxZRot) : zRot;
            zRot = Mathf.Abs(zRot) > zRotRatio? zRot : 0;
        }
        Quaternion newRotation = Quaternion.Euler(Mathf.Lerp(previousXRot, xRot, Time.deltaTime * 5f), transform.eulerAngles.y, Mathf.Lerp(previousZRot, zRot, Time.deltaTime * 5f));
        // Quaternion bodyRotation = transform.rotation * Quaternion.Euler(skewYRatio, 0f, skewRatio);
        transform.rotation = newRotation;
        
        // transform.eulerAngles = new Vector3(Mathf.Sign(transform.eulerAngles.x) * Mathf.Clamp(Mathf.Abs(transform.eulerAngles.x),0,45), transform.eulerAngles.y, Mathf.Clamp(transform.eulerAngles.z,-90f, 90f));
    }
}