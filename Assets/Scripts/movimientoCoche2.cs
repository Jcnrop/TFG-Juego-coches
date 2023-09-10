using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimientoCoche2 : MonoBehaviour
{

    [SerializeField] private Rigidbody cocheRb;

    [SerializeField] private float forwardAccel = 8f;
    [SerializeField] private float reverseAccel = 4f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float turnStrength = 180;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = cocheRb.transform.position;
    }

    private void FixedUpdate()
    {
        cocheRb.AddForce(transform.forward * forwardAccel);

    }
}
