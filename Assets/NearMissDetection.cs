using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NearMissDetection : MonoBehaviour
{
    
    private bool isStunting = false;

    private void OnTriggerEnter(Collider other){
        if(other.tag == "Automobile"){
            isStunting = true;
        }
    }

    private void OnTriggerStay(Collider other){
        Debug.Log(other.name);
        if(isStunting){
            Debug.DrawRay(this.transform.position, new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z) * 5, new Color(0.9f,0,0));
        }

        float distance = Vector3.Distance(transform.position, other.transform.position);
        if(distance < 4.9f){
            isStunting = false;
        }
    }

    private void OnTriggerExit(Collider other){
        if(isStunting){
            StartCoroutine(MovementManager.Instance.NearMissStunt());
            isStunting = false;
        }
    }
}
