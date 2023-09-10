using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class señalizador : MonoBehaviour
{
    private GameObject coche_principal;
    private GameObject señal;
    private GameObject canvas;
    private Camera camara;

    private void Start()
    {
        camara = GameObject.Find("/Directional Light/Main Camera").GetComponent<Camera>();
    }

    private void Update()
    {
        Vector3 posCocheRespCam = camara.WorldToViewportPoint(gameObject.transform.position); //Guarda la posición del coche con respecto a la camara (Todo lo que ve la camara tiene valor entre 0 y 1)

        if ( 0 <= posCocheRespCam.x && posCocheRespCam.x <= 1 && 0 <= posCocheRespCam.y && posCocheRespCam.y <= 1)//La señal se desactiva si la camara ve al coche
        {
            señal.SetActive(false);
            //print("soy visible");
        }
        else //Si no se ve en camara, se activa la señal
        {
            //Hacemos visible al señalizador
            señal.SetActive(true);

            // Le asignamos el tamaño que le corresponda según lo cerca que esté del coche principal

            Vector3 vDistancia = coche_principal.transform.position - gameObject.transform.position;

            float tamañoMax = 0.5f;
            float cercaniaMin = 200f;
            float cercaniaMax = 100f;
            float magnitudVDist = vDistancia.magnitude;

            if (magnitudVDist > cercaniaMin)
            {
                señal.GetComponent<RectTransform>().localScale = Vector3.zero;
            }
            else if (magnitudVDist < cercaniaMax)
            {
                señal.GetComponent<RectTransform>().localScale = new Vector3(tamañoMax, tamañoMax, tamañoMax);
            }
            else
            {
                float tamañoActual = tamañoMax * (1 - ((magnitudVDist - cercaniaMax) / (cercaniaMin - cercaniaMax)));

                señal.GetComponent<RectTransform>().localScale = new Vector3(tamañoActual, tamañoActual, tamañoActual);
            }


            //Calculamos la posición donde debería estar el señalizador

            float alturaCanvas = canvas.GetComponent<RectTransform>().rect.height * 0.85f;
            float anchuraCanvas = canvas.GetComponent<RectTransform>().rect.width * 0.90f;

            //Se calcula los limites a donde puede llegar el señalizador
            float limiteAltura = alturaCanvas / 2f * (vDistancia.z >= 0 ? -1 : 1);
            float limiteAnchura = anchuraCanvas / 2f * (vDistancia.x >= 0 ? -1 : 1);

            //Se calculan dos posiciones. Una con en el que la señal esté en el limite de altura y otra en el límite de anchura
            Vector3 posicionSeñal1 = new Vector3((vDistancia.x * -1 * limiteAltura) /((Mathf.Max(Mathf.Pow(vDistancia.z,2), Mathf.Pow(0.1f, 2))/ vDistancia.z) * -1), limiteAltura, 0);
            Vector3 posicionSeñal2 = new Vector3(limiteAnchura, (vDistancia.z * -1 * limiteAnchura) / ((Mathf.Max(Mathf.Pow(vDistancia.x, 2), Mathf.Pow(0.1f, 2)) / vDistancia.x) * -1), 0);

            Vector3 posicionSeñalFinal;

            //Se elige la posición que mas cerca esté del centro
            if (posicionSeñal1.magnitude < posicionSeñal2.magnitude)
            {
                posicionSeñalFinal = posicionSeñal1;
            }
            else
            {
                posicionSeñalFinal = posicionSeñal2;
            }

            //señal.GetComponent<RectTransform>().localPosition = new Vector3(vDistancia.x * -1, vDistancia.z * -1, 0);
            señal.GetComponent<RectTransform>().localPosition = posicionSeñalFinal;

            //Finalmente hacemos que la flecha apunte a donde debería

            GameObject flecha = señal.transform.Find("flecha").gameObject;

            flecha.GetComponent<RectTransform>().rotation= Quaternion.Euler(0, 0, Vector3.Angle(vDistancia,new Vector3(0,0,-1)) * (vDistancia.x >= 0 ? 1 : -1));

        }
        //print(camara.WorldToViewportPoint(gameObject.transform.position));
    }

    public void setCoche_principal(GameObject coche)
    {
        coche_principal = coche;
    }

    public void setSeñal(GameObject s)
    {
        señal = s;
    }

    public void setCanvas( GameObject c)
    {
        canvas = c;
    }


    private bool quitting = false;
    private void OnDisable()
    {
        if (quitting || generarCoches.cargarMenu)
        {
            return;
        }

        print("Se ha desactivado la señal");
        señal.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }


}
