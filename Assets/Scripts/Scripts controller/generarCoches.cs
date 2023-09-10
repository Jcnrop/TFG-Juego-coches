using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class generarCoches : MonoBehaviourPunCallbacks
{
    public GameObject coche_principal; // Si es nulo, todo los coches serán bots. Se elegirá un coche al azar para que la cámara le siga
                                        //En el modo online, esta variable guarda el prefab de los coches para Photon

    public int num_coches = 2;//Coches totales que estarán en la escena
    public static int num_coches_pedidos = 0;// Los coches pedidos desde el menú;
    public static string heuristica_pedida = "basica";
    public GameObject[] posiciones_coches;
    public GameObject prefabCoche;
    public GameObject prefabCollision;// El rigidbody del coche y la collision. Solo sirve para el modo online
    public GameObject prefabColisionSeñalizador;//Collider que se coloca al coche principal para realizar las señalizaciones

    [SerializeField] private cameraFollow seguimientoCamara;
    [SerializeField] private bool generarCochePrincipal = false;

    private bool hayGanador = false;
    public static bool cargarMenu = false;

    private GameObject[] coches;

    public bool GenerarCochePrincipal { get => generarCochePrincipal; set => generarCochePrincipal = value; }

    void Start()
    {
        cargarMenu = false;

        if (PhotonNetwork.InRoom)//Si está en una room es que es una partida online
        {
            setupOnline();
        }
        else
        {
            setupOffline();
        }

    }

    public void setupOffline()
    {
        if (num_coches_pedidos >= 2)
        {
            num_coches = num_coches_pedidos;
        }

        //coches = new GameObject[posiciones_coches.Length];
        coches = new GameObject[num_coches];

        for (int i = 0; i < num_coches; i++)
        {
            Transform posicion_coche = posiciones_coches[i].transform;
            coches[i] = Instantiate(prefabCoche, posicion_coche.position, Quaternion.Euler(0, posicion_coche.rotation.eulerAngles.y, 0));


            coches[i].name = "coche " + i;

            string heuristica = determinarHeuristica();

            //Se le pone una heuristica al coche

            //coches[i].GetComponent<carController>().setHeuristica("basica");
            //coches[i].GetComponent<carController>().setHeuristica("nitroFreno");
            coches[i].GetComponent<carController>().setHeuristica("evitacionPeligro");

            //coches[i].GetComponent<carController>().setHeuristica(heuristica);
        }

        if (generarCochePrincipal)
        {
            coche_principal = coches[Random.Range(0, coches.Length)];
            coche_principal.GetComponent<carController>().EsUnBot = false;
            coche_principal.GetComponent<estadoCoche>().EsCochePrincipal = true;
            coche_principal.name = "coche principal";
        }

        seguimientoCamara.setTarget(coche_principal.transform);
        seguimientoCamara.Coches = coches;

        GameObject areaSeñalizador = Instantiate(prefabColisionSeñalizador, coche_principal.transform.position, new Quaternion(0, 0, 0, 0), coche_principal.transform);
    }

    public void setupOnline()
    {
        coches = new GameObject[PhotonNetwork.CurrentRoom.PlayerCount + (10 - PhotonNetwork.CurrentRoom.MaxPlayers)];//Num jugadores más los bots

        IDictionary<int, Player> jugadoresEnSala = PhotonNetwork.CurrentRoom.Players;
        jugadoresEnSala = jugadoresEnSala.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        int i = 0;
        foreach (KeyValuePair<int, Player> jugador in jugadoresEnSala)
        {
            if (jugador.Value.UserId!=null)//Solo se puede ver el id de nuestro jugador. Si el id no es nulo, es que es nuestro jugador
            {
                print("Instancio mi coche");
                Transform posicion_coche = posiciones_coches[i].transform;
                //coche_principal = PhotonNetwork.Instantiate(prefabCoche.name, posicion_coche.position, Quaternion.Euler(0, posicion_coche.rotation.eulerAngles.y, 0));
                GameObject collider = PhotonNetwork.Instantiate(prefabCollision.name, posicion_coche.position, Quaternion.Euler(0, posicion_coche.rotation.eulerAngles.y, 0));
                coche_principal = collider.GetComponent<colisionTrasGolpe>().Coche;
                coche_principal.GetComponent<carController>().EsUnBot = false;
                coche_principal.GetComponent<estadoCoche>().EsCochePrincipal = true;
                //coche_principal.GetComponent<carController>().TheRB = collider.GetComponent<Rigidbody>();
                //coche_principal.GetComponent<estadoCoche>().Collider = collider;

                break;
            }
            i++;
        }

        if (PhotonNetwork.IsMasterClient)//El master genera los bots
        {
            i = PhotonNetwork.CurrentRoom.PlayerCount;
            while (i < coches.Length)
            {
                print("Instancio el coche de un bot");
                Transform posicion_coche = posiciones_coches[i].transform;
                GameObject collider = PhotonNetwork.InstantiateRoomObject(prefabCollision.name, posicion_coche.position, Quaternion.Euler(0, posicion_coche.rotation.eulerAngles.y, 0));

                collider.GetComponent<colisionTrasGolpe>().Coche.GetComponent<carController>().EsUnBot = true;

                string heuristica = determinarHeuristica();
                collider.GetComponent<colisionTrasGolpe>().Coche.GetComponent<carController>().setHeuristica(heuristica);
                //collider.GetComponent<colisionTrasGolpe>().Coche.name = PhotonNetwork.NickName + "-bot"+i;

                print("Dueño bot: " + collider.GetComponent<PhotonView>().Owner+ ". Es mio? = "+ collider.GetComponent<PhotonView>().IsMine);

                i++;
            }
        }
        

        seguimientoCamara.setTarget(coche_principal.transform);
        seguimientoCamara.Coches = coches;

        GameObject areaSeñalizador = Instantiate(prefabColisionSeñalizador, coche_principal.transform.position, new Quaternion(0, 0, 0, 0), coche_principal.transform);
    }

    public void añadirCoche(GameObject coche)//Añade un coche si hay un hueco libre
    {
        for (int i=0; i<coches.Length;i++)
        {
            if (coches[i]==null)
            {
                coches[i]=coche;
                print("Se añadio el coche de "+ coche.name + " en la posición "+ i + "del array");
                break;
            }
        }
    }

    public string determinarHeuristica()
    {
        string heuristica;

        if (heuristica_pedida.Equals("aleatoria"))
        {
            string[] heuristicas = { "basica", "nitroFreno", "evitacionPeligro" };
            heuristica = heuristicas[Random.Range(0, heuristicas.Length)];
        }
        else
        {
            heuristica = heuristica_pedida;
        }

        return heuristica;
    }

    public void comprobarGanador()
    {
        int contadorCochesVivos = 0;
        GameObject cocheGanador = coche_principal;

        foreach (GameObject coche in coches)
        {
            if (coche.activeInHierarchy)
            {
                cocheGanador = coche;
                contadorCochesVivos++;
            }
            if (contadorCochesVivos>=2)
            {
                break;
            }
        }

        if (contadorCochesVivos==1)
        {
            print("Hay un ganador");
            hayGanador = true;

            GameObject.Find("/Canvas/Nombrar_ganador").GetComponent<TextMeshProUGUI>().text="GANADOR <br> "+ cocheGanador.name;
            StartCoroutine(cargarMenuPrincipal(5));

        }
        else if (contadorCochesVivos == 0 && !hayGanador)
        {
            print("Ha habido un error");
            GameObject.Find("/Canvas/Nombrar_ganador").GetComponent<TextMeshProUGUI>().text = "EMPATE";
            StartCoroutine(cargarMenuPrincipal(5));
        }
    }

    IEnumerator cargarMenuPrincipal(float seg)
    {
        cargarMenu = true;
        yield return new WaitForSeconds(seg);

        SceneManager.LoadScene(0);

    }

    public void cargarMenuPrincipalSeg(float seg)
    {
        if (PhotonNetwork.CurrentRoom != null)//Si estamos en una sala, salimos
        {
            foreach (GameObject coche in coches)
            {
                coche.GetComponent<carController>().CocheDesconectado = true;
            }
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            StartCoroutine(cargarMenuPrincipal(seg));
        }
    }

    public override void OnLeftRoom()
    {
        StartCoroutine(cargarMenuPrincipal(0));
    }
}
