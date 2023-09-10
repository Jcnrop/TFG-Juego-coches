using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class molinillo : MonoBehaviour
{
    void Update()
    {
        //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 100 * Time.deltaTime, 0f));
    }

    void FixedUpdate()
    {
        transform.GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 100 * Time.deltaTime, 0f)));
    }
}
