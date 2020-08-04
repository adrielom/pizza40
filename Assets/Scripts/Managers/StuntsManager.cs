using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct GAINPOINTS
{
    public const float SKEW = 200;
    public const float MISS = 500;
    public const float BURNOUT = 100;
}
public class StuntsManager : MonoBehaviour
{

    #region  "STUNTS_AVAILABLE"
    private enum Stunts{
        NONE,
        SKEW,
        MISS,
        BURNOUT,
        CRASH,
    }
    
    #endregion


    #region "PRIVATE_VARIABLES"
    [SerializeField]
    private GAINPOINTS POINTS;

    private int multiplier = 1;

    private Stunts currentStunt, previousStunt = Stunts.NONE;

    [SerializeField]
    private float pointsToGain;

    private float secs, secsToCount = 4.0f;

    private float pointsForStunt {
        get{
            switch(currentStunt){
                case Stunts.BURNOUT:
                    return GAINPOINTS.BURNOUT;
                case Stunts.SKEW:
                    return GAINPOINTS.SKEW;
                case Stunts.MISS:
                    return GAINPOINTS.MISS;
                default:
                    return 0;
            }
        }
    }

    #endregion

    #region  "UPDATE_CYCLE"
    void LateUpdate(){
        previousStunt = currentStunt;
        currentStunt = CheckStunts();
        CompareStunts();
        CheckMultiplier();
        CheckCrash();
    }

    #endregion
    
    #region "METHODS"
    Stunts CheckStunts(){
        if(MovementManager.Instance.isMissing){
            Debug.Log("Detected near miss!");
            return Stunts.MISS;
        }
        if(MovementManager.Instance.isOnBurnout){
            return Stunts.BURNOUT;
        }
        
        if(MovementManager.Instance.isOnSkew){
            return Stunts.SKEW;
        }

        return Stunts.NONE;
    }

    void CompareStunts(){
        secs+= Time.smoothDeltaTime;
        if(currentStunt != Stunts.NONE && currentStunt != Stunts.CRASH){
            if(secs>=secsToCount){
                pointsToGain+= pointsForStunt;
                secs = 0;
            }
        }
    }

    void CheckMultiplier(){
        if(secs<secsToCount){
            if(currentStunt != Stunts.NONE && currentStunt != Stunts.CRASH){
                if(currentStunt != previousStunt)
                    multiplier *=2;
            }
        }
        else{
            GlobalStatsManager.Instance.SetTurbo(pointsToGain * multiplier);
            pointsToGain = 0;
            multiplier = 1;
        }
    }

    void CheckCrash(){
        if(currentStunt == Stunts.CRASH){
            multiplier = 1;
            pointsToGain = 0;
            currentStunt = Stunts.NONE;
            previousStunt = Stunts.NONE;
        }
    }
    #endregion
    

}
