using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DeliveryPoint{
    public Vector3 startPoint;
    public Vector3 deliveryPoint;
    public GameObject ringObject;
    public float amountOfMoney;
    public int quantityOfSeconds;
}

public class CheckpointManager : Singleton<CheckpointManager>
{
    [SerializeField]
    private int quantityOfPoints;

    private DeliveryPoint currentPoint;

    [SerializeField]
    private GameObject startPointModel;

    [SerializeField]
    private GameObject finalPointModel;

    private List<DeliveryPoint> generatedPoints;

    List<Transform> spawnPoints;

    [SerializeField]
    private float minMagnitudeDistance, maxMagnitudeDistance;

    private GameObject rootObject;

    [SerializeField]
    const float DOLARBYMPH = 1.93f; 
    private IEnumerator Start(){
        yield return new WaitUntil(()=>GlobalStatsManager.Instance.ready);
        rootObject =  GameObject.FindGameObjectWithTag("RootPoint");
        generatedPoints = GenerateRandomPoints();
    }

    private List<DeliveryPoint> GenerateRandomPoints(){
        spawnPoints = new List<Transform>();
        foreach(Transform y in rootObject.transform){
            spawnPoints.Add(y);
        }
        List<DeliveryPoint> tempPoints = new List<DeliveryPoint>();
        for(int i = 0; i< quantityOfPoints; i++){
            tempPoints.Add(GenerateCheckpoint());
        }
        return tempPoints;
    }

    private DeliveryPoint GenerateCheckpoint(){
        DeliveryPoint x = new DeliveryPoint();
        Transform rootPoint = spawnPoints[Random.Range(0, spawnPoints.Count-1)];
        spawnPoints.Remove(rootPoint);
        Vector3 globalPoint = rootPoint.transform.position;
        x.startPoint =  new Vector3(globalPoint.x, globalPoint.y, globalPoint.z);
        x.deliveryPoint = GenerateDeliveryPoint(x.startPoint);
        x.amountOfMoney = GenerateMoney(x.startPoint, x.deliveryPoint);
        x.quantityOfSeconds = GenerateSeconds(x.startPoint, x.deliveryPoint);
        x.ringObject = GameObject.Instantiate(startPointModel, rootPoint);
        x.ringObject.transform.position = x.startPoint;
        x.ringObject.AddComponent<CheckpointCollider>().self = x;
        return x;
    }

    private Vector3 GenerateDeliveryPoint(Vector3 startPoint){
        Vector3 v1 = Vector3.zero;
        do
            v1 = rootObject.transform.GetChild(Random.Range(0, rootObject.transform.childCount)).position;
        while(v1.Equals(startPoint));
        return new Vector3(v1.x, v1.y, v1.z);
    }

    private int GenerateSeconds(Vector3 startPoint, Vector3 deliveryPoint){
        return Mathf.FloorToInt((deliveryPoint - startPoint).magnitude * GlobalStatsManager.Instance.difficultRatio * 10);
    }

    private float GenerateMoney(Vector3 startPoint, Vector3 deliveryPoint){
        return DOLARBYMPH * (startPoint - deliveryPoint).magnitude;
    }    

    public void SetDeliveryPoint(DeliveryPoint delivery){
        currentPoint = delivery;
        generatedPoints.Remove(currentPoint);
        generatedPoints.ForEach(e=>{
            Destroy(e.ringObject);
        });
        generatedPoints.Clear();
        SwitchPoint();
        
    }

    public void SetInitialPoint(){
        GlobalStatsManager.Instance.moneyScore += currentPoint.amountOfMoney;
        GenerateRandomPoints();

    }

    private void SwitchPoint(){
        currentPoint.ringObject = Instantiate(finalPointModel);
        currentPoint.ringObject.transform.position = currentPoint.deliveryPoint;
        currentPoint.ringObject.AddComponent<CheckpointCollider>().self = currentPoint;
        TimeManager.Instance.AddTime(currentPoint.quantityOfSeconds);
    }
}

