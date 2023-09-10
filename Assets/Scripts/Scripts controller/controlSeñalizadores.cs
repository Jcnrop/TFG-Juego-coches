using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlSeñalizadores : MonoBehaviour
{
    [SerializeField] private GameObject prefabSeñalizador;
    
    //Se añade un señalizador al coche que entre cuando entra a la esfera
    private void OnTriggerEnter(Collider other)
    {
        //Se le añade una señal si es un coche y no tenia una señal
        if (other.gameObject.layer == 7 && other.gameObject.GetComponent<señalizador>() == null)
        {
            other.gameObject.AddComponent<señalizador>();
            other.gameObject.GetComponent<señalizador>().setCoche_principal(this.gameObject.transform.parent.gameObject);

            GameObject canvas = GameObject.Find("Canvas");
            other.gameObject.GetComponent<señalizador>().setSeñal(Instantiate(prefabSeñalizador, Vector2.zero, Quaternion.Euler(0, 0, 0), canvas.transform));
            other.gameObject.GetComponent<señalizador>().setCanvas(canvas);

        }
    }

    private void OnTriggerStay(Collider other)
    {
        float diferenciaPosiciones = Mathf.Abs(gameObject.transform.position.magnitude - other.transform.position.magnitude);
        if (diferenciaPosiciones>10)
        {
            //print("Hay un coche en "+other.transform.position);
        }

    }
}
