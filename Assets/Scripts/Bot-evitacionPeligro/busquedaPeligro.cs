using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class busquedaPeligro : MonoBehaviour
{
    private string[] peligros = {"Fuego","Coche","AvisoAsteroide"};

    private GameObject peligroCercano;

    public Transform[] limitesMapa;

    public GameObject coche;

    private bool mantenerDireccion = false;
    private string direccionActual = "recto";

    // Start is called before the first frame update
    void Start()
    {
        GameObject limites = GameObject.Find("limites_mapa");

        limitesMapa = new Transform[2];

        limitesMapa[0]= limites.transform.GetChild(0);
        limitesMapa[1] = limites.transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!mantenerDireccion)
        {
            direccionActual= queDireccionTomar(coche);
        }

        coche.GetComponent<carController>().Direccion = direccionActual;
    }

    public string queDireccionTomar(GameObject coche)
    {
        string direccion = "recto";

        Vector3 caida = hayCaidaCerca();

        //print(caida);

        if (caida.x != 0 || caida.z != 0)//Si la caida esta muy cerca se detecta
        {
            //print("Me caigo. Caida en " + caida);

            Vector3 distCocheCaida = caida - coche.transform.position;

            //print(distCocheCaida.magnitude);

            //print(cercaDeEsquina(caida));
            
            if (cercaDeEsquina(coche.transform.position))
            {
                direccion = "atras";
                StartCoroutine(mantenerDireccionSegundos(0.4f));
            }
            else
            {
                direccion = Vector3.Cross(distCocheCaida.normalized, coche.transform.forward).y > 0 ? "derecha" : "izquierda";
            }

        }
        else if (peligroCercano != null)
        {
            //print("Peligro en posicion " + peligroCercano.transform.position);

            Vector3 distCochePeligro = peligroCercano.transform.position - coche.transform.position;
            direccion = Vector3.Cross(distCochePeligro.normalized, coche.transform.forward).y > 0 ? "derecha" : "izquierda";
        }

        //print(direccion);

        return direccion;
    }

    private void OnTriggerStay(Collider other)//Aquí se analizan los choques y se comprueba si es un peligro o no
    {

        bool esPeligro = esPeligroso(other.gameObject);

        if (!esPeligro)
        {
            return;
        }

        Vector3 distancia = other.transform.position - transform.position;
        distancia.y = 0;

        if (peligroCercano != null)
        {
            Vector3 distanciaPeligroCercano = peligroCercano.transform.position - transform.position;
            distanciaPeligroCercano.y = 0;

            if (distancia.magnitude < distanciaPeligroCercano.magnitude)
            {
                peligroCercano = other.gameObject;
            }
        }
        else
        {
            peligroCercano = other.gameObject;
        }

        //print("Ha entrado el objeto " + other.gameObject.name + ". Esta a una distancia de " + distancia.magnitude);

    }

    private void OnTriggerExit(Collider other)
    {
        if (GameObject.ReferenceEquals(other.gameObject,peligroCercano))
        {
            peligroCercano = null;
        }
    }


    public bool esPeligroso(GameObject g)
    {
        bool esPeligro = false;

        foreach (string peligro in peligros)
        {
            if (g.CompareTag(peligro))
            {
                esPeligro = true;
                break;
            }
        }

        return esPeligro;
    }

    public Vector3 hayCaidaCerca()
    {
        Vector3 hayCaida = Vector3.zero;

        Vector3[] distanciasBordes = new Vector3[4];

        distanciasBordes[0] = new Vector3(limitesMapa[0].position.x - transform.position.x, 0, 0);//izq
        distanciasBordes[1] = new Vector3(limitesMapa[1].position.x - transform.position.x, 0, 0);//der
        distanciasBordes[2] = new Vector3(0, 0, limitesMapa[0].position.z - transform.position.z);//abajo
        distanciasBordes[3] = new Vector3(0, 0, limitesMapa[1].position.z - transform.position.z);//arriba

        Vector3 menorVector = distanciasBordes[0]; //Indica la distancia del coche al limite mas cercano del mapa

        for (int i =1; i<distanciasBordes.Length; i++)//Comprobamos a que lado del mapa estamos mas cerca
        {
            menorVector = distanciasBordes[i].magnitude < menorVector.magnitude ? distanciasBordes[i] : menorVector;
            
        }

        //if ((menorVector.z==0 &&  Mathf.Abs(menorVector.x) <=transform.localScale.x/2f) || (menorVector.x == 0 && Mathf.Abs(menorVector.z) <= transform.localScale.z/2f))//Si esta lo suficientemente cerca se considera un peligro
        if (menorVector.magnitude <= 40)//Si esta lo suficientemente cerca se considera un peligro
        {
            if (peligroCercano == null || (peligroCercano!=null && menorVector.magnitude < (transform.position-peligroCercano.transform.position).magnitude))
            {
                //print("menorVector: "+ menorVector);
                hayCaida = transform.position + menorVector;
                //print("hayCaida: "+ hayCaida);

                /*if ((menorVector-distanciasBordes[0]).magnitude==0)
                {
                    print("caida izq");
                }
                else if ((menorVector - distanciasBordes[1]).magnitude == 0)
                {
                    print("caida der");
                }
                else if ((menorVector - distanciasBordes[2]).magnitude == 0)
                {
                    print("caida abajo");
                }
                else
                {
                    print("caida arriba");
                }*/
            }
        }

        return hayCaida;

    }

    public bool cercaDeEsquina(Vector3 posicion)
    {
        bool estaCerca = false;

        Vector3[] esquinas = new Vector3[4];

        esquinas[0] = limitesMapa[0].position;//abajo izq
        esquinas[1] = new Vector3(limitesMapa[0].position.x, limitesMapa[0].position.y, limitesMapa[1].position.z);//arriba izq
        esquinas[2] = limitesMapa[1].position;// arriba der
        esquinas[3] = new Vector3(limitesMapa[1].position.x, limitesMapa[0].position.y, limitesMapa[0].position.z);//abajo der

        foreach (Vector3 esquina in esquinas)
        {
            if ((posicion-esquina).magnitude <30)
            {
                estaCerca = true;
                break;
            }
        }

        return estaCerca;
    }

    IEnumerator mantenerDireccionSegundos(float segundos)
    {
        mantenerDireccion = true;

        yield return new WaitForSeconds(segundos);
        mantenerDireccion = false;
    }

}
