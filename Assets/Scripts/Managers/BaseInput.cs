using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInput : Singleton<BaseInput>
{
    public bool devMode;

    [SerializeField]
    private FloatingJoystick touchJoy;
    public float HorizontalInput{get{
        return devMode ? Input.GetAxis("Horizontal") :  touchJoy.Horizontal;
    }}

    public float VerticalInput{get{
        return devMode ? Input.GetAxis("Vertical") :  touchJoy.Vertical;
    }}

    public float ForwardInput{get{
        return devMode ? (Input.GetKey(KeyCode.J)?1:0) - (Input.GetKey(KeyCode.K)?1:0) : 1; 
    }}
    
}
