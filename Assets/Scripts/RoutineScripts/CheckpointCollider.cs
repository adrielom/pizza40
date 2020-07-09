using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointCollider : MonoBehaviour
{
    public DeliveryPoint self;

    void Start(){
    }

    void OnTriggerStay(Collider other){
        if(other.tag == "Player"){
            Rigidbody r1 = other.GetComponent<Rigidbody>();
            if(r1.velocity.magnitude <= 1){
                this.GetComponent<Collider>().enabled = false;
                StartCoroutine(DestroyRoutine());
            }
        }
    }

    IEnumerator DestroyRoutine(){
        yield return new WaitForSeconds(5.0f);
        if(this.tag == "StartPoint") CheckpointManager.Instance.SetDeliveryPoint(self);  
        else if(this.tag == "FinishPoint") CheckpointManager.Instance.SetInitialPoint();
        Destroy(this.gameObject);
    }
}
