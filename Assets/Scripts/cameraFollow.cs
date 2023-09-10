using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private GameObject[] coches;
    public Image dañoCamara;
    private bool dañado = false;

    private Vector3 vibracion = Vector3.zero;

    [SerializeField] private GameObject botones;

    public GameObject[] Coches { get => coches; set => coches = value; }


    private void Update()
    {
        
        if (dañado == true)
        {
            dañoCamara.color = new Color(dañoCamara.color.r, dañoCamara.color.g, dañoCamara.color.b, Mathf.Lerp(dañoCamara.color.a, 0.25f, 0.05f));
        }
        else
        {
            dañoCamara.color = new Color(dañoCamara.color.r, dañoCamara.color.g, dañoCamara.color.b, Mathf.Lerp(dañoCamara.color.a, 0, 0.05f));
        }
        
    }

    private void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition + vibracion;

        //transform.LookAt(target);
    }

    public void setTarget(Transform t)
    {
        target = t;
    }

    public void cambiarObjetivoCamaraIzq(float seg)
    {
        cambiarObjetivoCamara( seg,  -1);
    }

    public void cambiarObjetivoCamaraDer(float seg)
    {
        print("Derecha");
        cambiarObjetivoCamara(seg, 1);
    }

    public void cambiarObjetivoCamara(float seg, int direccion)
    {
        direccion = direccion == 0 ? 1 : direccion;
        int posicionCoche = 0;

        //Se busca la posicion en la lista de coches del coche al que sigue la camara actualmente
        for(int i =0; i<coches.Length;i++)
        {
            if (coches[i].name.Equals(target.gameObject.name))
            {
                posicionCoche = i;
                break;
            }
        }


        //Se busca un nuevo coche al que seguir segun la dirección
        for (int i = 0; i < coches.Length; i++)
        {
            posicionCoche += direccion;
            posicionCoche = posicionCoche >= coches.Length ? 0 : posicionCoche;
            posicionCoche = posicionCoche < 0 ? coches.Length-1 : posicionCoche;

            if (coches[posicionCoche].activeInHierarchy)
            {
                StartCoroutine(delayCambio(seg, coches[posicionCoche].transform));
                break;
            }
        }
    }

    public void mostrarDaño()
    {
        dañoCamara.color = new Color(255, 0, 0, 0);
        StartCoroutine("tiempoDaño");
    }

    public void mostrarCuracion()
    {
        dañoCamara.color = new Color(0, 255, 0, 0);
        StartCoroutine("tiempoDaño");
    }

    public void startShake(float duration, float magnitude)
    {
        StartCoroutine(cameraShake(duration, magnitude));
    }

    IEnumerator cameraShake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;

        float elapsed = 0.0f;

        while (elapsed<duration)
        {
            float x = Random.Range(-1f,1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            //transform.position = new Vector3(originalPosition.x+x, originalPosition.y+y, transform.localPosition.z);

            vibracion = new Vector3(x,y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        //transform.localPosition = new Vector3(0,0, transform.localPosition.z);
        vibracion = Vector3.zero;
    }


    IEnumerator tiempoDaño()
    {
        dañado = true;

        yield return new WaitForSeconds(0.1f);

        dañado = false;
    }

    IEnumerator delayCambio(float seg, Transform t)
    {
        yield return new WaitForSeconds(seg);
        setTarget(t);

        if (!botones.activeSelf)
        {
            botones.SetActive(true);
        }
    }
}
