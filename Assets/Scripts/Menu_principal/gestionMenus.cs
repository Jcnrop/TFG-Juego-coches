using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class gestionMenus : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject botonError;

    public GameObject menuListaJugadoresMaster;
    public GameObject menuListaJugadoresEntrante;
    public GameObject menuCrearPartida;
    public GameObject menuBuscarPartida;


    void Start()
    {

        if (PhotonNetwork.InRoom)
        {
            loadingScreen.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;
                menuBuscarPartida.SetActive(false);
                menuListaJugadoresMaster.SetActive(true);
                GetComponent<createAndJoinRooms>().actualizarListaJugadores();
            }
            else
            {
                menuCrearPartida.SetActive(false);
                menuListaJugadoresEntrante.SetActive(true);
                GetComponent<createAndJoinRooms>().actualizarListaJugadores();
            }
        }
    }


    private bool lanzado = false;
    private void Update()
    {
        if (PhotonNetwork.InLobby && !lanzado)
        {
            loadingScreen.SetActive(false);
            lanzado = true;
        }

    }

    public void problemaConexión(string problema)
    {
        botonError.SetActive(true);
        GameObject.Find("/Canvas/Pantalla_de_carga/Rueda").SetActive(false);
        GameObject.Find("/Canvas/Pantalla_de_carga/Texto").GetComponent<TextMeshProUGUI>().text = problema;
    }

}
