using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class NetworkParedes : MonoBehaviour
{
    private GameObject[] paredes;

    public GameObject[] Paredes { get => paredes; set => paredes = value; }

    /*void Start()
    {
        paredes = GameObject.Find("PosicionParedes").GetComponentsInChildren<colisionPared>();

        print("Id de las primeras paredes y localización");

        print("Id: "+ paredes[0].Id_pared + "Posición: " + paredes[0].gameObject.transform.position.ToString());
        print("Id: " + paredes[1].Id_pared + "Posición: " + paredes[1].gameObject.transform.position.ToString());
        print("Id: " + paredes[2].Id_pared + "Posición: " + paredes[2].gameObject.transform.position.ToString());
    }*/

    public void actualizarParedAlResto(int id_pared)
    {
        GetComponent<PhotonView>().RPC("actualizarPared",RpcTarget.Others,id_pared);
    }


    [PunRPC]
    void actualizarPared(int id)
    {
        paredes[id].GetComponent<colisionPared>().aumentarNumGolpes();
    }

}
