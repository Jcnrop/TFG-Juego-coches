using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class createAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public GameObject loadingScreen;

    public GameObject[] listaJugadores = new GameObject [2];
    //public GameObject listaJugadoresEntrantes;

    private string nombreActualRoom = null;

    public void CreateRoom(TMP_Dropdown dropdown)
    {
        if (createInput.text==null || createInput.text.Length<4 || createInput.text.Length > 10)
        {
            print("Error: El codigo de la sala debe tener entre 4 y 10 caracteres");
            return;
        }

        int n_bots = int.Parse(dropdown.options[dropdown.value].text);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)(10 - n_bots);//Número máximo de jugadores en una sala. Los bots limitan el número de personas que puede haber

        //Cuando alguien crea una room, se une automaticamente a él
        PhotonNetwork.CreateRoom(createInput.text, roomOptions, null);
        loadingScreen.SetActive(true);

        //print("He creado la sala " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreatedRoom()
    {
        print("Se han sincronizado las salas");
        //PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        loadingScreen.SetActive(false);
        print("Codigo: " + returnCode + ", Mensaje:" + message);

        if (returnCode == 32766)//Ya existe una sala con ese nombre
        {
            GameObject.Find("/Canvas/OpcionesPartidaOnline/Mensaje_error").GetComponent<TMP_Text>().text = "Ya existe una sala con ese nombre";
        }
        else
        {
            GameObject.Find("/Canvas/OpcionesPartidaOnline/Mensaje_error").GetComponent<TMP_Text>().text = "Hubo un error al crear la sala";
        }
    }

    public void JoinRoom()
    {
        //Errores que pueden surgir antes de intentar meterte a una sala
        if (joinInput.text.Length<1)
        {
            GameObject.Find("/Canvas/BuscarPartida/Mensaje_error").GetComponent<TMP_Text>().text = "El nombre de la sala no puede estar vacio";
            return;
        }
        else if (PhotonNetwork.InRoom)
        {
            GameObject.Find("/Canvas/BuscarPartida/Mensaje_error").GetComponent<TMP_Text>().text = "Ya estas en una sala. Sal de la que estas para acceder a otra";
            return;
        }

        PhotonNetwork.JoinRoom(joinInput.text);
        loadingScreen.SetActive(true);
    }

    public void LeaveRoom()
    {
        
        if (PhotonNetwork.CurrentRoom!=null)
        {
            loadingScreen.SetActive(true);
            print("He salido de la sala " + PhotonNetwork.CurrentRoom.Name);
            PhotonNetwork.LeaveRoom();
            nombreActualRoom = null;
        }
    }

    public override void OnJoinedRoom()
    {
        loadingScreen.SetActive(false);
        nombreActualRoom = PhotonNetwork.CurrentRoom.Name;
        print("He entrado a la sala " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.PlayerCount==1)//Si solo hay una persona en la sala al unirte, es que eres el creador de la sala.
        {
            print("Soy el creador de la sala");
            listaJugadores[0].SetActive(true);
            GameObject.Find("/Canvas/OpcionesPartidaOnline").SetActive(false);
            actualizarListaJugadores();
        }
        else
        {
            GameObject.Find("/Canvas/Crearpartida").SetActive(false);
            listaJugadores[1].SetActive(true);
            actualizarListaJugadores();
        }

        //PhotonNetwork.LoadLevel("Pruebas_online");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        loadingScreen.SetActive(false);
        print("Codigo: "+ returnCode+ ", Mensaje:"+message);

        if (returnCode==32758)//No existe la sala
        {
            GameObject.Find("/Canvas/BuscarPartida/Mensaje_error").GetComponent<TMP_Text>().text = "No existe una sala con ese nombre";
        }
        else if (returnCode == 32765)//La sala está llena
        {
            GameObject.Find("/Canvas/BuscarPartida/Mensaje_error").GetComponent<TMP_Text>().text = "La sala está llena";
        }
        else if (returnCode == 32764)//La partida esta en curso
        {
            GameObject.Find("/Canvas/BuscarPartida/Mensaje_error").GetComponent<TMP_Text>().text = "La partida esta en curso. Espera a que termine";
        }
        else
        {
            GameObject.Find("/Canvas/BuscarPartida/Mensaje_error").GetComponent<TMP_Text>().text = "Hubo un problema al unirse a la sala";
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        actualizarListaJugadores();
        print(newPlayer.NickName + "ha entrado a la sala");
        //PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnLeftRoom()
    {
        if (quitting)
        {
            return;
        }

        loadingScreen.SetActive(false);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        actualizarListaJugadores();
        print(otherPlayer.NickName + "ha abandonado la sala");
    }


    public void actualizarListaJugadores()
    {
        if (PhotonNetwork.InRoom)
        {
            int index = listaJugadores[0].activeSelf ? 0 : 1;

            TMP_Text lista = listaJugadores[index].transform.Find("Scroll View/Viewport/Content").GetComponent<TMP_Text>();
            lista.text = "";
            IDictionary<int, Player> jugadoresEnSala = PhotonNetwork.CurrentRoom.Players;
            jugadoresEnSala = jugadoresEnSala.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<int, Player> jugador in jugadoresEnSala)
            {
                print("Numero lista:" + jugador.Key + " Nombre: " + jugador.Value.NickName +" idJugador:"+jugador.Value.UserId + " Soy yo?: " + (jugador.Value.UserId!=null ? "Si":"No"));
                lista.text += jugador.Value.NickName;

                if (jugador.Value.IsMasterClient)
                {
                    lista.text += "(master)";
                }

                lista.text += "<br>";
            }
        }
    }

    private bool quitting =false;
    private void OnApplicationQuit()
    {
        quitting = true;
    }

}
