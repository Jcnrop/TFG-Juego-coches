using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rueda_loading : MonoBehaviour
{
    private float rps = 0.01f;


    // Update is called once per frame
    void Update()
    {
        gameObject.transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z+360 * rps * Mathf.Abs(Mathf.Sin(2.5f*Time.time)));

        if (transform.eulerAngles.z>=360)
        {
            gameObject.transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z -360);
        }
    }
}
