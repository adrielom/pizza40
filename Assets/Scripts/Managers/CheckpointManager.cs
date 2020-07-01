using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct DeliveryPoint{
    public Vector3 startPoint;
    public Vector3 deliveryPoint;
    public GameObject ringObject;
    public float amountOfMoney;
    public int quantityOfSeconds;
}

public class CheckpointManager : MonoBehaviour
{
    [SerializeField]
    private int quantityOfPoints;

    private DeliveryPoint currentPoint;

    [SerializeField]
    private GameObject startPointModel;

    [SerializeField]
    private GameObject finalPointModel;

    private ArrayList generatedPoints;

    [SerializeField]
    private float minMagnitudeDistance, maxMagnitudeDistance;

    private GameObject rootObject;

    private IEnumerator Start(){
        yield return new WaitUntil(()=>GlobalStatsManager.Instance.ready);
        rootObject =  GameObject.FindGameObjectWithTag("RootPoint");
        GenerateRandomPoints();
    }

    private ArrayList GenerateRandomPoints(){
        ArrayList tempPoints = new ArrayList();
        for(int i = 0; i< quantityOfPoints; i++){
            tempPoints.Add(GenerateCheckpoint());
        }
        return tempPoints;
    }

    private DeliveryPoint GenerateCheckpoint(){
        DeliveryPoint x = new DeliveryPoint();
        Transform rootPoint = rootObject.transform.GetChild(
            Random.Range(0, rootObject.transform.childCount)
        );
        Vector3 point = Random.onUnitSphere * Random.Range(minMagnitudeDistance, maxMagnitudeDistance);
        Vector3 globalPoint = rootPoint.TransformPoint(point);
        x.startPoint =  new Vector3(globalPoint.x, Mathf.Clamp(globalPoint.y, -4.0f, 10f), globalPoint.z);
        x.deliveryPoint = GenerateDeliveryPoint(x.startPoint);
        x.amountOfMoney = 500;
        x.quantityOfSeconds = GenerateSeconds(x.startPoint, x.deliveryPoint);
        x.ringObject = GameObject.Instantiate(startPointModel, rootPoint);
        x.ringObject.transform.position = x.startPoint;
        return x;
    }

    private Vector3 GenerateDeliveryPoint(Vector3 startPoint){
        return (startPoint.normalized - Random.onUnitSphere) * Random.Range(minMagnitudeDistance, maxMagnitudeDistance);
    }

    private int GenerateSeconds(Vector3 startPoint, Vector3 deliveryPoint){
        return Mathf.FloorToInt((deliveryPoint - startPoint).magnitude * GlobalStatsManager.Instance.difficultRatio);
    }    
}

