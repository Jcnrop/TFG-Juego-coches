using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class collider_online : MonoBehaviour
{
    void Start()
    {
        this.gameObject.name = "C-" + this.gameObject.GetComponent<PhotonView>().Owner.NickName;
        //this.gameObject.GetComponent<colisionTrasGolpe>().Coche = GameObject.Find(this.gameObject.GetComponent<PhotonView>().Owner.NickName);
    }

    private void Update()
    {
        this.gameObject.GetComponent<colisionTrasGolpe>().Coche.transform.rotation = Quaternion.Euler(this.gameObject.transform.rotation.eulerAngles);
    }
}
