using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class generadorParedes : MonoBehaviour
{
    [SerializeField] private Transform posicion_inicial;
    [SerializeField] private GameObject prefabTrozosPared;


    [SerializeField] private float tamTrozos = 10;
    [SerializeField] private float espaciado = 0;
    [SerializeField] private int trozosVer = 2;
    [SerializeField] private int trozosHor = 2;

    private GameObject[] trozos = new GameObject[0];

    private int trozosHorAnterior = 0;
    private int trozosVerAnterior = 0;

    public int TrozosVer { get => trozosVer; set => trozosVer = value; }
    public int TrozosHor { get => trozosHor; set => trozosHor = value; }
    public float TamTrozos { get => tamTrozos; set => tamTrozos = value; }
    public float Espaciado { get => espaciado; set => espaciado = value; }
    public Transform Posicion_inicial { get => posicion_inicial; set => posicion_inicial = value; }
    public GameObject PrefabTrozosPared { get => prefabTrozosPared; set => prefabTrozosPared = value; }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in Posicion_inicial)
        {
            Destroy(child.gameObject);
        }
        generarParedes();
    }


    public void generarParedes()
    {
        /*if (TrozosVer != trozosHorAnterior || TrozosHor != trozosVerAnterior && TrozosHor >= 0 && TrozosVer >= 0) //Se generan nuevas paredes si no estaban las mismas paredes antes
        {

            if (trozos.Length > 0) //Se destruyen las paredes que hubiera antes
            {
                destruirParedes();
            }

            trozosHorAnterior = TrozosVer;
            trozosVerAnterior = TrozosHor;

            
        }*/
        destruirParedes();

        //trozos = new GameObject[TrozosVer * TrozosHor];

        trozos = new GameObject[TrozosVer * TrozosHor - (TrozosVer-2)*(TrozosHor-2)];

        int trozosGenerados = 0;

        for (int i = 0; i < TrozosVer; i++) //Se crean las nuevas paredes
        {
            for (int j = 0; j < TrozosHor; j++)
            {

                if (i == 0 || i == TrozosVer-1 || j == 0 || j == TrozosHor-1)
                {
                    Vector3 posicion_trozo = Posicion_inicial.position + new Vector3((TamTrozos + Espaciado) * i, 0, (TamTrozos + Espaciado) * j);
                    GameObject nuevoTrozo = Instantiate(PrefabTrozosPared, posicion_trozo, Quaternion.Euler(0, 0, 0), Posicion_inicial);

                    nuevoTrozo.transform.localScale = new Vector3(TamTrozos, TamTrozos, TamTrozos);
                    nuevoTrozo.GetComponent<colisionPared>().Id_pared = trozosGenerados;
                    nuevoTrozo.name = "Pared " + trozosGenerados;

                    trozos[trozosGenerados] = nuevoTrozo;
                    trozosGenerados++;
                }
                /*Vector3 posicion_trozo = Posicion_inicial.position + new Vector3((TamTrozos + Espaciado) * i, 0, (TamTrozos + Espaciado) * j);
                GameObject nuevoTrozo = Instantiate(PrefabTrozosPared, posicion_trozo, Quaternion.Euler(0, 0, 0), Posicion_inicial);

                nuevoTrozo.transform.localScale = new Vector3(TamTrozos, TamTrozos, TamTrozos);

                trozos[i * TrozosHor + j] = nuevoTrozo;*/
            }
        }

        try
        {
            NetworkParedes np = GameObject.Find("NetworkParedes").GetComponent<NetworkParedes>();

            np.Paredes = trozos;
        }
        catch (System.Exception e)
        {
            //print("No se encontró networkparedes" + e.ToString());
        }

        print("Se han generado nuevas paredes");
    }

    public void destruirParedes()
    {

        if (Application.isEditor)
        {
            foreach (GameObject trozo in trozos)
            {
                DestroyImmediate(trozo);
            }
            foreach (Transform child in Posicion_inicial)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        else
        {
            foreach (GameObject trozo in trozos)
            {
                Destroy(trozo);
            }

        }

    }


}
