using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pruebaMovimiento : MonoBehaviour
{

    public float velocidadMin = 3000f;
    public float velocidadMax = 5000f;
    public float velocidadActual;
    public float aceleracion = 10f;

    // Start is called before the first frame update
    void Start()
    {
        velocidadActual = velocidadMin;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //transform.Translate(new Vector3(horizontal,0,vertical)*Time.deltaTime*speed);
        gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(horizontal, 0, vertical) * Time.deltaTime * velocidadActual);

        //print(vertical);
        if (-0.1f <= vertical && vertical <= 0.1f)
        {
            velocidadActual = velocidadMin;
        }
        else
        {
            velocidadActual += aceleracion * Time.deltaTime;
        }

        velocidadActual = velocidadActual > velocidadMax ? velocidadMax : velocidadActual; 
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //transform.Translate(new Vector3(horizontal,0,vertical)*Time.deltaTime*speed);
        gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(horizontal, 0, vertical) * velocidadActual);
    }
}
