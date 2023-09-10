using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class connectToServer : MonoBehaviourPunCallbacks
{

    public gestionMenus menu;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.PhotonServerSettings.DevRegion = null;

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            print("Me he conectado al servidor");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause.ToString().Equals("DnsExceptionOnConnect"))
        {
            print("No hay conexión a internet");

            menu.problemaConexión("No se ha podido conectar con el servidor\nRevise su conexión a internet");

        }
        else
        {
            print("Hubo el siguiente problema: " + cause.ToString());
        }
        
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        //SceneManager.LoadScene("Lobby");
        if (!SceneManager.GetActiveScene().name.Equals("Menu_principal"))
        {
            SceneManager.LoadScene("Menu_principal");
        }
    }
}
