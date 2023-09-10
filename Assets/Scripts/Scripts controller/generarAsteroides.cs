using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class generarAsteroides : MonoBehaviour
{
    public Transform[] rangoPosiciones;
    public GameObject prefabAsteroide;
    
    private float tiempoEntreAsteroides = 5f;
    private float tiempoMinEntreAsteroides = 1f;

    private float maxPosicionX;
    private float minPosicionX;
    private float maxPosicionZ;
    private float minPosicionZ;


    // Start is called before the first frame update
    void Start()
    {
        maxPosicionX = Mathf.Max(rangoPosiciones[0].position.x, rangoPosiciones[1].position.x);
        minPosicionX = Mathf.Min(rangoPosiciones[0].position.x, rangoPosiciones[1].position.x);
        maxPosicionZ = Mathf.Max(rangoPosiciones[0].position.z, rangoPosiciones[1].position.z);
        minPosicionZ = Mathf.Min(rangoPosiciones[0].position.z, rangoPosiciones[1].position.z);

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine("lanzarAsteroide");
            }
        }
        else
        {
            StartCoroutine("lanzarAsteroide");
        }
    }

    private int contadorAsteroides = 0;
    IEnumerator lanzarAsteroide()
    {
        yield return new WaitForSeconds(tiempoEntreAsteroides);

        float posicionX = Random.Range(minPosicionX,maxPosicionX);
        float posicionZ = Random.Range(minPosicionZ,maxPosicionZ);

        Vector3 lugar_caida = new Vector3(posicionX, 0.01f, posicionZ);

        float probabilidadCura = 0.5f;
        bool GenerarVida = Random.Range(0.00f, 1.00f)<=probabilidadCura;

        contadorAsteroides++;

        string nombreAsteroide = "Asteroide_" + contadorAsteroides;

        if (PhotonNetwork.InRoom)
        {
            GameObject.Find("NetworkEventos").GetComponent<NetworkEventos>().sendLanzarMeteorito(lugar_caida,GenerarVida,nombreAsteroide);
        }
        else
        {
           GameObject asteroide = Instantiate(prefabAsteroide, lugar_caida, Quaternion.Euler(90, 0, 0));

            asteroide.GetComponent<aviso_asteroide>().GenerarVida = GenerarVida;
            asteroide.name = nombreAsteroide;
        }

        tiempoEntreAsteroides *= 0.8f;
        tiempoEntreAsteroides = tiempoEntreAsteroides < tiempoMinEntreAsteroides ? tiempoMinEntreAsteroides : tiempoEntreAsteroides;

        StartCoroutine("lanzarAsteroide");

    }
}
