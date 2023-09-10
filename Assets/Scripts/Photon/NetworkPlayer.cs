using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


//Esta clase se encargará de reducir el lag entre los jugadores
public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    protected GameObject coche;
    protected Vector3 remotePlayerPosition;
    protected Quaternion remotePlayerRotation;

    private float tiempoRecomposicion = 1f;
    private float tiempoRecomposicionAct = 0f;

    private void Awake()
    {
        coche = GetComponent<colisionTrasGolpe>().Coche;

        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Update()
    {
        if (photonView.IsMine)//Si es nuestro coche, no hacemos nada
        {
            return;
        }

        //coche.transform.rotation = remotePlayerRotation;
        coche.transform.rotation = Quaternion.Lerp(coche.transform.rotation, remotePlayerRotation, Time.deltaTime * 6);
    }
    void FixedUpdate()
    {
        if (photonView.IsMine)//Si es nuestro coche, no hacemos nada. 
        {
            return;
        }

        if (coche.GetComponent<estadoCoche>().Golpeado && tiempoRecomposicionAct<=0)//Si el coche ha sido golpeado, reducimos que el coche clon persiga al verdadero para que no haya lag en los golpes
        {
            tiempoRecomposicionAct = tiempoRecomposicion;
        }
        else if (tiempoRecomposicionAct > 0)
        {
            tiempoRecomposicionAct = (tiempoRecomposicionAct - Time.fixedDeltaTime) <0? 0 : (tiempoRecomposicionAct - Time.fixedDeltaTime);
        }

        var lagDistance = 2*(remotePlayerPosition - transform.position);

        //print("Distancia lag: "+ lagDistance.magnitude);

        if (lagDistance.magnitude <2 && tiempoRecomposicionAct<=0) // Si esta lo suficientemente cerca, se le deja de empujar
        {
            //GetComponent<Rigidbody>().AddForce(Vector3.zero);
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().MovePosition(remotePlayerPosition);
            //transform.position = remotePlayerPosition;
        }
        else if(lagDistance.magnitude >= 1 || lagDistance.magnitude < 5)// Si no esta muy lejos, se le empuja para que el movimiento no sea brusco
        {
            //float recomposicion = (1 - (tiempoRecomposicionAct / tiempoRecomposicion)) * (tiempoRecomposicionAct > tiempoRecomposicion*0.25? 0 : 1);//Esto es un porcentaje. Cuanto mas bajo sea este valor, menos seguira el clon a su coche
            //print("Recomposicion: " + recomposicion);
            float recomposicion = 1 - (tiempoRecomposicionAct / tiempoRecomposicion);

            GetComponent<Rigidbody>().AddForce(lagDistance * 500000 * Time.deltaTime * recomposicion);
        }
        else // Si está demasiado lejos, se transporta hasta donde esté el punto.
        {
            GetComponent<Rigidbody>().MovePosition(remotePlayerPosition);
            //transform.position = remotePlayerPosition;
        }

        //GetComponent<Rigidbody>().MovePosition(remotePlayerPosition);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position); //Mando posicion coche
            stream.SendNext(transform.rotation); //Mando la rotacion del coche
            stream.SendNext(coche.GetComponent<estadoCoche>().Vida); //Mando la vida actual del coche
            stream.SendNext(coche.name); //Mando el nombre del gameobject del coche
            stream.SendNext(coche.GetComponent<estadoCoche>().Color);
        }
        else
        {
            remotePlayerPosition = (Vector3)stream.ReceiveNext(); //Recibo posicion coche
            remotePlayerRotation = (Quaternion)stream.ReceiveNext(); //Recibo la rotacion del coche
            coche.GetComponent<estadoCoche>().Vida=(float)stream.ReceiveNext(); //Recibo la vida actual del coche
            coche.name = (string)stream.ReceiveNext(); //Recibo el nombre del gameobject del coche
            coche.GetComponent<estadoCoche>().cambiarColorCarcasa((int)stream.ReceiveNext());
        }

    }

    private void OnDrawGizmosSelected()
    {
        if (photonView.IsMine)//Si es nuestro coche, no hacemos nada
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(remotePlayerPosition, 10);
        
    }
}
