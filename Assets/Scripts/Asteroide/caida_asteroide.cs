using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caida_asteroide : MonoBehaviour
{
    public GameObject avisoAsteroide;
    public ParticleSystem humo;
    public GameObject colliderEmpuje;
    public GameObject prefabFuego;
    public GameObject prefabVida;

    private float velocidad = 200;
    private Vector3 direccionCaida;
    private bool haColisionado = false;
    private bool generarVida = false;

    public bool GenerarVida { get => generarVida; set => generarVida = value; }

    // Start is called before the first frame update
    void Start()
    {
        direccionCaida = avisoAsteroide.transform.position - gameObject.transform.position;
        direccionCaida = direccionCaida.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        if (!haColisionado)
        {
            transform.Translate(direccionCaida * velocidad * Time.deltaTime, Space.World);
        }
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 6)
        {
            return;
        }

        //gameObject.SetActive(false);
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        humo.gameObject.SetActive(true);
        colliderEmpuje.SetActive(true);
        avisoAsteroide.SetActive(false);

        if (generarVida) // Instanciamos una cura
        {
            GameObject cura = Instantiate(prefabVida);
            cura.transform.position = avisoAsteroide.transform.position;
            cura.name = transform.parent.gameObject.name + "_cura";
        }
        else //Instanciamos fuego
        {
            GameObject fuego = Instantiate(prefabFuego);
            fuego.transform.position = avisoAsteroide.transform.position;
        }
        

        GameObject.Find("/Directional Light/Main Camera").GetComponent<cameraFollow>().startShake(0.15f,2f);


        haColisionado = true;

    }

    private void OnDisable()
    {
        humo.gameObject.SetActive(true);
        avisoAsteroide.SetActive(false);
    }

}
