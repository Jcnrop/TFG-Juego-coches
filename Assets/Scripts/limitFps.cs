using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class limitFps : MonoBehaviour
{
    //private int fps = 60;
    public int targetFrameRate = 60;

    // Start is called before the first frame update
    void Start()
    {
        /*fps = Screen.currentResolution.refreshRate;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;*/

        //QualitySettings.vSyncCount = 0;
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = targetFrameRate;
    }

    // Update is called once per frame
    /*void Update()
    {
        Application.targetFrameRate = fps;
    }*/
}
