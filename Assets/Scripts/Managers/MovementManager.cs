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

    [SerializeField]
    private float xRot, zRot;

    [SerializeField]
    private float maxXRot = 45, maxZRot = 90;

    [SerializeField]
    private float speedRotEffect = 1f;
    


    [SerializeField]
    private float accelCurveCoeff=0;
    [SerializeField]
    private float accelRate=0, backwardAccelRate=0;
    /// <summary>
    /// empty transform to handle pivoting torque
    /// </summary>
    public Transform rotationPivot;

    private MoveType moveType;

    private bool burnout = false;

    ///<summary>
    /// Turbo variables.
    ///</summary>

    [SerializeField]
    private float TurboAccel = 2.0f, turboDuration = 0.3f;
    private bool onTurboMode = false;

    private float turboTime = 0f, turboCounter = 0f;

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
        Movement(BaseInput.Instance.ForwardInput, BaseInput.Instance.VerticalInput, BaseInput.Instance.HorizontalInput, BaseInput.Instance.TurboInput);
    }

    public void Movement(float forward, float vertical, float horizontal, float turbo)
    {
        //getting the horizontal and vertical inputs that changes over time
        float horizontalRatio = horizontal * Time.fixedDeltaTime * sideYSpeed;
        float skewRatio = -horizontal * 2f;
        float skewYRatio = -vertical * 4f;
        float verticalRatio = vertical * Time.fixedDeltaTime * sideXSpeed;

        DetermineSideRatio(verticalRatio, horizontalRatio);

        if(turbo>0){
            if(GlobalStatsManager.Instance.turboBarRatio > 20){
                onTurboMode = true;
            }   
        }

        if(onTurboMode){
            CheckTurboTime();
        }

        if(!burnout){
            CalcForwardSpeed(forward, vertical);
            TurnTransform();
            Skew(skewRatio, skewYRatio, BaseInput.Instance.FirstComboInput);
            
        }
        if(burnout && forward < 0){
            PerformStunt(skewYRatio, 0);
            return;
        }
        else if(burnout && forward >= 0){
            SetPivot();
            burnout = false;
        }

    }


    private void DetermineSideRatio(float verticalRatio, float horizontalRatio){
        accelerationYrate = horizontalRatio != 0 ? accelerationYrate + (horizontalRatio * Time.fixedDeltaTime) : 0;
        accelerationXRate = verticalRatio != 0 ? accelerationXRate + (verticalRatio * Time.fixedDeltaTime) : 0;
        // accelerationZrate = skewRatio != 0 ? accelerationZrate + (skewRatio * Time.deltaTime) : 0;
        accelerationYrate = Mathf.Clamp(accelerationYrate, -maxSideSpeed, maxSideSpeed);
        // accelerationZrate = Mathf.Clamp(accelerationZrate, -maxSideSpeed*0.5f, maxSideSpeed*0.5f);
        accelerationXRate = Mathf.Clamp(accelerationXRate, -maxSideSpeed, maxSideSpeed);
    }
    
    private void CalcForwardSpeed(float forward, float vertical){
        float accelPower =  forward>= 0 ? accelRate : backwardAccelRate;

        float accelRamp = onTurboMode ? rbd.velocity.magnitude/(maxSpeed * TurboAccel) : rbd.velocity.magnitude/maxSpeed;
        float accelFinalRamp = Mathf.Lerp(accelCurveCoeff, 1, accelRamp * accelRamp);

        float finalAcceleration = accelPower * accelFinalRamp;

        if(forward>=0){
            finalAcceleration = onTurboMode? finalAcceleration * TurboAccel : finalAcceleration;
        }

        Quaternion turnAngleY = Quaternion.AngleAxis(accelerationYrate, rbd.transform.up);

        Vector3 fwd = turnAngleY * rbd.transform.forward;
        
        
        /*Should be activated only if you want vertical speed */
        // Vector3 towards = accelerationXRate * rbd.transform.up;


        /*---------- BURNOUT MECHANIC------------------------------------------------------*/
        if(forward < 0 && Mathf.Sign(fwd.z) != Mathf.Sign(rbd.velocity.z)){
            if(rbd.velocity.magnitude<11 && xRot >= -30){
                burnout = true;
                SetPivot();
                return;
            }
            
        }

        /*---------------------------------------------------------------------------------------------------*/

        Vector3 movement = fwd * forward * finalAcceleration;
        Vector3 adjustedVelocity = rbd.velocity + /*+ towards +*/ movement * Time.fixedDeltaTime;
        
        if(adjustedVelocity.magnitude > maxSpeed){
            adjustedVelocity = forward > 0 && onTurboMode ?Vector3.ClampMagnitude(adjustedVelocity, maxSpeed * TurboAccel) : Vector3.ClampMagnitude(adjustedVelocity, maxSpeed);
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


        
        // Quaternion bodyRotation = transform.rotation * Quaternion.Euler(skewYRatio, 0f, skewRatio);
        transform.rotation = CalcRotation(skewRatio, skewYRatio, stuntCoeff);

        
        // transform.eulerAngles = new Vector3(Mathf.Sign(transform.eulerAngles.x) * Mathf.Clamp(Mathf.Abs(transform.eulerAngles.x),0,45), transform.eulerAngles.y, Mathf.Clamp(transform.eulerAngles.z,-90f, 90f));
    }


    private Quaternion CalcRotation(float skewRatio, float skewYRatio, float stuntCoeff){
        float previousXRot = xRot;
        float previousZRot = zRot;

        //stunt coeff = 0? se previousZ > 45 decrementar
        float xRotRatio = maxXRot*0.01f*speedRotEffect;
        float zRotRatio = maxZRot*0.01f*speedRotEffect;

        // xRot = skewYRatio!= 0 ? xRot + skewYRatio : xRot - (Mathf.Sign(xRot)*xRotRatio);
        // zRot = skewRatio!= 0 ? zRot + skewRatio : zRot - (Mathf.Sign(zRot)*zRotRatio);
        if(stuntCoeff == 0 && Mathf.Abs(previousZRot) > 45){
            zRot = zRot - (Mathf.Sign(zRot)*zRotRatio);
        }
        else{
            xRot = skewYRatio!= 0 ? xRot + skewYRatio : xRot - (Mathf.Sign(xRot)*xRotRatio);
            zRot = skewRatio!= 0 ? zRot + skewRatio : zRot - (Mathf.Sign(zRot)*zRotRatio);
        }
        
        xRot = Mathf.Clamp(xRot, -maxXRot, maxXRot);
        xRot = Mathf.Abs(xRot)>xRotRatio? xRot : 0;
        if(stuntCoeff != 0){
            zRot = Mathf.Clamp(zRot + skewRatio*2, -90f, 90f);
        }
        else{
            zRot = Mathf.Abs(zRot) > Mathf.Abs(previousZRot) ? Mathf.Clamp(zRot, -maxZRot, maxZRot) : zRot;
            zRot = Mathf.Abs(zRot) > zRotRatio? zRot : 0;
        }

        return Quaternion.Euler(Mathf.Lerp(previousXRot, xRot, Time.deltaTime * 5f), transform.eulerAngles.y, Mathf.Lerp(previousZRot, zRot, Time.deltaTime * 5f));
    }
    private void PerformStunt(float skewYRatio, float stuntCoeff){
        transform.rotation = CalcRotation(0, skewYRatio, stuntCoeff);
        transform.RotateAround(rotationPivot.position, Vector3.up, accelerationYrate);
    }

    private void SetPivot(){
        rotationPivot.localPosition = new Vector3(rotationPivot.localPosition.x, rotationPivot.localPosition.y, -rotationPivot.localPosition.z);
    }

    private void CheckTurboTime(){
        GlobalStatsManager.Instance.turboBarRatio -= Time.fixedDeltaTime * (1/turboDuration);
        if(GlobalStatsManager.Instance.turboBarRatio < 0){
            onTurboMode = false;
            GlobalStatsManager.Instance.turboBarRatio = 0;
        }
    }
}