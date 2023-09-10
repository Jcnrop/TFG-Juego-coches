using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objetoEnMovimiento : MonoBehaviour
{
    public float normalDrag = 10;
    private void FixedUpdate()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(-transform.forward * 10000);

        if (gameObject.GetComponent<Rigidbody>().drag < normalDrag)
        {
            float nuevoDrag = 0.2f;
            gameObject.GetComponent<Rigidbody>().drag += nuevoDrag > normalDrag ? normalDrag : nuevoDrag;
        }
    }

}
