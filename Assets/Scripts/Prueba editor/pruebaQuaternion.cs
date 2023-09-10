using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pruebaQuaternion : MonoBehaviour
{

    public GameObject c1;
    public GameObject c2;
    // Start is called before the first frame update
    void Start()
    {
        c2.transform.rotation = c1.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
