using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aviso_asteroide : MonoBehaviour
{
    public GameObject circuloAviso;
    public SpriteRenderer spriteAviso;
    public float tiempoAviso = 2;
    public GameObject collider_asteroide;
    public GameObject collider_empuje;
    private Vector3 posicionInicialMeteorito;

    private float tiempoParpadeo = 0.15f;
    private float tiempoParpadeoActual;

    public float tamMaxAviso = 50;
    private float aumentoTiempo;

    private bool reinicio = false;

    private bool generarVida = false;

    public bool GenerarVida { get => generarVida; set => generarVida = value; }


    // Start is called before the first frame update
    void Start()
    {
        aumentoTiempo = (tamMaxAviso - circuloAviso.transform.localScale.x)/(tiempoAviso/5);
        collider_asteroide.transform.localScale = new Vector3(tamMaxAviso*0.9f, tamMaxAviso * 0.9f, tamMaxAviso * 0.9f);
        collider_empuje.transform.localScale = new Vector3(tamMaxAviso * 0.9f, tamMaxAviso * 0.9f, tamMaxAviso * 0.9f);
        posicionInicialMeteorito = collider_asteroide.transform.position;

        reinicio = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (reinicio)
        {
            reiniciarParametros();
        }

        if (circuloAviso.transform.localScale.x>=tamMaxAviso)
        {
            circuloAviso.transform.localScale = new Vector3(tamMaxAviso, tamMaxAviso,1);
        }
        else
        {
            float aumento = aumentoTiempo * Time.deltaTime;
            circuloAviso.transform.localScale = new Vector3(circuloAviso.transform.localScale.x + aumento, circuloAviso.transform.localScale.y + aumento, 1);

            /*if (circuloAviso.transform.localScale.x >= tamMaxAviso)
            {
                StartCoroutine("parpadeoSeñalAviso");
            }*/
        }
    }

    public void reiniciarParametros()
    {
        circuloAviso.transform.localScale = Vector3.one;
        tiempoParpadeoActual = tiempoParpadeo;
        StartCoroutine("caidaAsteroide");
        StartCoroutine("parpadeoSeñalAviso");

        collider_asteroide.transform.position = posicionInicialMeteorito;

        reinicio = false;
    }

    public void impactoAsteroide()
    {
        collider_asteroide.SetActive(true);
        collider_asteroide.GetComponent<caida_asteroide>().GenerarVida = generarVida;
    }

    IEnumerator caidaAsteroide()
    {
        yield return new WaitForSeconds(tiempoAviso);

        print("OH NO!!!! UN ASTEROIDE!!!! *se muere*");

        //gameObject.SetActive(false);

        impactoAsteroide();
    }

    IEnumerator parpadeoSeñalAviso()
    {
        yield return new WaitForSeconds(tiempoParpadeoActual);

        spriteAviso.enabled = spriteAviso.enabled ? false : true; //Activa y desactiva la señal
        tiempoParpadeoActual = tiempoParpadeoActual / 1.05f;

        StartCoroutine("parpadeoSeñalAviso");
    }

    private void OnDisable()
    {
        reinicio = true;
    }

}
