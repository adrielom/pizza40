using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInput : Singleton<BaseInput>
{
    public bool devMode;
    private float previousInput = 0;
    private float secs = 0;

    private float secsToCheck = 0, secsToIncrement = 0;

    [SerializeField]
    private float maxSecToCombo = 0.3f;

    [SerializeField]
    private float maxSecComboOne = 5.0f;

    private bool comboFlag = false;

    private bool inComboFlag = false;

    private float firstComboInputValue = 0;

    [SerializeField]
    private FloatingJoystick touchJoy;

    ///<summary>
    /// Gets the base horizontal input.
    ///</summary>
    public float HorizontalInput{get{
        return devMode ? Input.GetAxis("Horizontal") :  touchJoy.Horizontal;
    }}

    ///<summary>
    /// Gets the base vertical input.
    ///</summary>
    public float VerticalInput{get{
        return devMode ? Input.GetAxis("Vertical") :  touchJoy.Vertical;
    }}

    ///<summary>
    /// Gets the base acceleration input.
    ///</summary>
    public float ForwardInput{get{
        return devMode ? (Input.GetKey(KeyCode.J)?1:0) - (Input.GetKey(KeyCode.K)?1:0) : 1; 
    }}


    ///<summary>
    /// Returns the input for the first stunt combo.
    ///</summary>
    public float FirstComboInput{
        get{
            return firstComboInputValue;
        }
    }


    ///<summary>
    /// Get Input from the turbo button.
    ///</summary>

    public float TurboInput{
        get{
            return devMode? (Input.GetKey(KeyCode.Space)?1:0) : 0;
        }
    }

    ///<summary>
    /// Gets a value of 1 if the player moved the analog stick on the horizontal direction fast, for a stunt.
    ///</summary>
    private void GetByTurn(){
        if(!comboFlag){
            if(Mathf.Abs(HorizontalInput)>0.8f){
                comboFlag = true;
                inComboFlag = false;
                previousInput = HorizontalInput;
                secs = 0;
            }
        }
        else if(inComboFlag){
                if(secs < maxSecComboOne){
                    if(Mathf.Abs(HorizontalInput) > 0.25f && Mathf.Sign(HorizontalInput) != Mathf.Sign(previousInput)){
                        print("zerou");
                        secs = 0;
                    } 
                    previousInput = HorizontalInput;
                    firstComboInputValue = Mathf.Sign(HorizontalInput) * -1;
                    return;
                }
                else{
                    secs = 0;
                    inComboFlag = false;
                    comboFlag = false;
                }
        }
        else{
            if(secs<maxSecToCombo){
                if(
                    Mathf.Abs(HorizontalInput) > 0.8f && 
                    Mathf.Sign(HorizontalInput/previousInput) < 0
                ){
                    inComboFlag = true;
                    secs = 0;
                    firstComboInputValue = Mathf.Sign(HorizontalInput) * -1;
                    return;
                }
            }
            else{
                comboFlag = false;
            }
        }
        firstComboInputValue = 0;
        return;
    }


    void FixedUpdate(){
        secs += Time.fixedDeltaTime;
        secsToIncrement += Time.fixedDeltaTime;
        if((secsToIncrement) > 0.4){
            GetByTurn();
            secsToIncrement = 0;
        }
    }
    
}
