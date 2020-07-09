using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : Singleton<TimeManager>
{
    

    [SerializeField]
    private float seconds = 30;

    [SerializeField]
    private int minutes = 0; 

    private string timeText = "0:00";

    [SerializeField]
    private Text elementText;
    void LateUpdate(){
        if(GlobalStatsManager.Instance.ready){
            DecrementTime();
        }
    }


    ///<summary>
    /// Adds a quantity of time, then formats the number to display again.
    ///</summary>
    public void AddTime(int quantitySeconds){
        seconds += quantitySeconds;
        if(seconds>60){
            int getMinutes = Mathf.FloorToInt(seconds) / 60;
            seconds-= getMinutes * 60;
            minutes += getMinutes;
        }

        FormatTimeText();
    }

    ///<summary>
    /// Decrements the time on a update loop, by the deltaTime ratio.
    ///</summary>
    private void DecrementTime(){
        if(seconds>0)
            seconds -= Time.deltaTime;
        if(seconds < 0){
            if(minutes>0){
                minutes--;
                seconds += 60.0f;
            }
            else{
                Debug.Log("Time Ended");
                seconds = 0;
            }            
        }
        FormatTimeText();
    }

    ///<summary>
    /// Formats the timer to appear on the UI.
    ///</summary>
    private void FormatTimeText(){
        string formatedSeconds = Mathf.FloorToInt(seconds)>9?Mathf.FloorToInt(seconds).ToString():"0" + Mathf.FloorToInt(seconds);
        timeText = $"{minutes}:{formatedSeconds}";
        if(elementText != null){
            elementText.text = timeText;
        }
    }
}
