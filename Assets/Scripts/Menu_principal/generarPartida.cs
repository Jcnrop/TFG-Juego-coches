using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class generarPartida : MonoBehaviour
{

    private void Start()
    {
        generarCoches.heuristica_pedida = "basica";
    }

    public void partidaOffline(TMP_Dropdown dropdown)
    {
        int n_coches = int.Parse(dropdown.options[dropdown.value].text) + 1;

        generarCoches.num_coches_pedidos = n_coches;

        SceneManager.LoadScene(1);
    }

    public void partidaOnline(TMP_Dropdown dropdown)
    {
        int n_coches = int.Parse(dropdown.options[dropdown.value].text);

        generarCoches.num_coches_pedidos = n_coches;

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount>1)//Si estamos en una sala, somos el master y somos al menos dos personas, se puede empezar
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(2);
        }
        else
        {
            print("Ha habido un error al intentar empezar la partida");
        }
    }

    public void cambiarHeuristica(TMP_Dropdown dropdown)
    {
        string[] heuristicas = { "basica", "nitroFreno", "evitacionPeligro", "aleatoria" };

        string heuristica = heuristicas[dropdown.value];

        print("Se ha cambiado la heuristica a "+ heuristica);

        generarCoches.heuristica_pedida = heuristica;
    }
}
