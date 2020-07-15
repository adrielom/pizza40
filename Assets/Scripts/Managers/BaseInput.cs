using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInput : Singleton<BaseInput>
{
    public bool devMode;
    private float previousInput = 0;
    private float secs = 0;

    [SerializeField]
    private float maxSecToCombo = 0.3f;

    [SerializeField]
    private float maxSecComboOne = 5.0f;

    private bool comboFlag = false;

    private bool inComboFlag = false;

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
            return GetByTurn();
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
    private float GetByTurn(){
        if(!comboFlag){
            if(Mathf.Abs(HorizontalInput)>0.8f){
                comboFlag = true;
                inComboFlag = false;
                previousInput = HorizontalInput;
                secs = 0;
            }
        }
        else if(inComboFlag){
            secs+= Time.deltaTime;
            if(Mathf.Abs(HorizontalInput) > 0.25f){
                if(secs < maxSecComboOne){
                    if(Mathf.Abs(HorizontalInput) > 0 && Mathf.Sign(HorizontalInput) != Mathf.Sign(previousInput)){
                        secs = 0;
                    } 
                    previousInput = HorizontalInput;
                    return Mathf.Sign(HorizontalInput) * -1;
                }
                else{
                    secs = 0;
                    inComboFlag = false;
                    comboFlag = false;
                }
            }
           
        }
        else{
            secs+= Time.deltaTime;
            if(secs<maxSecToCombo){
                if(
                    Mathf.Abs(HorizontalInput) > 0.8f && 
                    Mathf.Sign(HorizontalInput/previousInput) < 0
                ){
                    inComboFlag = true;
                    return Mathf.Sign(HorizontalInput) * -1;
                }
            }
            else{
                comboFlag = false;
            }
        }
        return 0;
    }
    
}
