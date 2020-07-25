using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalStatsManager : Singleton<GlobalStatsManager>
{
    public float difficultRatio = 0.1f;

    [SerializeField] 
    private float difficultAccelRatio = -0.01f;

    [SerializeField]
    private int initialTime = 70;

    public float moneyScore = 0;

    public bool ready = false;

    public float turboBarRatio = 100f;

    void Awake(){
        TimeManager.Instance.AddTime(initialTime);
        ready = true;
    }


    public void SetTurbo(float quantity){
        turboBarRatio+= quantity / 100f;
    }
}
