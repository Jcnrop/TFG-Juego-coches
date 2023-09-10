using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class NetworkEventos : MonoBehaviour
{
    public GameObject prefabAsteroide;

    private const byte DESTROY_CAR_EVENT = 1;
    private const byte SEND_ASTEROID = 2;
    private const byte DESTROY_HEALING = 3;

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += manejadorDeEventos;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= manejadorDeEventos;
    }


    public void sendDestruirCocheJugador(string nickJugador)
    {
        object[] content = new object[] { nickJugador };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(DESTROY_CAR_EVENT, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void sendLanzarMeteorito(Vector3 lugarCaida, bool generarVida, string nombreAsteroide)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        object[] content = new object[] { lugarCaida, generarVida, nombreAsteroide };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(SEND_ASTEROID, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void sendDestruirCura(string nombreAsteroide)
    {
        object[] content = new object[] { nombreAsteroide };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(DESTROY_HEALING, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void manejadorDeEventos(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == DESTROY_CAR_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;

            string nickJugador = (string)data[0];

            GameObject.Find(nickJugador).GetComponent<estadoCoche>().desactivarCoche();
        }
        else if (eventCode == SEND_ASTEROID)
        {
            object[] data = (object[])photonEvent.CustomData;

            Vector3 lugarCaida = (Vector3)data[0];
            bool generarVida = (bool)data[1];
            string nombreAsteroide = (string)data[2];

            GameObject asteroide = Instantiate(prefabAsteroide, lugarCaida, Quaternion.Euler(90, 0, 0));
            asteroide.GetComponent<aviso_asteroide>().GenerarVida = generarVida;
            asteroide.name = nombreAsteroide;

        }
        else if (eventCode == DESTROY_HEALING)
        {
            object[] data = (object[])photonEvent.CustomData;

            string nombreCura = (string)data[0];

            Destroy(GameObject.Find(nombreCura));
        }
    }
}
