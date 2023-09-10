using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class generarNombre : MonoBehaviour
{
    public TMP_InputField inputNombre;
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.NickName.Length>0)
        {
            inputNombre.text = PhotonNetwork.NickName;
            return;
        }

        if (inputNombre.text==null || inputNombre.text.Length<1)
        {
            string[] vocales = { "a", "e", "i", "o", "u" };
            string[] consonantes = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z"};

            int numLetras = Random.Range(4,9);

            string nombre = "";

            for (int i=0; i<numLetras; i++)
            {
                int n = Random.Range(0, 2);

                nombre += n == 0 ? vocales[Random.Range(0, vocales.Length)] : consonantes[Random.Range(0, consonantes.Length)];
            }

            inputNombre.text = nombre.ToUpper();
            actualizarNickName();
        }
    }

    public void actualizarNickName()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = inputNombre.text;
        }
    }
}
