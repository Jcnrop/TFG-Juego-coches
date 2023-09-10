using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class colisionTrasGolpe : MonoBehaviour
{
    [SerializeField] private GameObject coche;
    
    private float potenciaGolpe = 750000;
    private float porcentajeMinimo = 0.250f;//Esto es indica el porcentaje de potencia minimo que puede tener un golpe
    private float porcentajeMaxDaño = 0.1f; //Esto indica lo maximo que se puede quitar con un golpe

    public GameObject Coche { get => coche; set => coche = value; }

    private void Start()
    {
        if (coche==null)
        {
            coche = new GameObject();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer != 6)//Si la collision no es contra el suelo
        {
            Coche.GetComponent<carController>().setDrag(1);
            Coche.GetComponent<carController>().caladoPorColision();

            if (PhotonNetwork.InRoom && !GetComponent<PhotonView>().IsMine)
            {
                return;
            }

            Vector3 normalCollision = collision.GetContact(0).normal;
            normalCollision = (new Vector3(normalCollision.x, 0, normalCollision.z)).normalized;

            float potenciaFinalGolpe = calcularFuerzaGolpe(normalCollision);

            if (collision.gameObject.layer == 7) //Si es un coche se le empuja
            {
                estadoCoche estadoCollision = collision.gameObject.GetComponent<colisionTrasGolpe>().Coche.GetComponent<estadoCoche>();// Estado del coche con el que hemos chocado

                if (!estadoCollision.Golpeado)//Solo se golpea al coche si no esta en modo golpeado
                {
                    

                    if (!PhotonNetwork.InRoom)//Si es offline
                    {
                        estadoCollision.recibirImpacto(potenciaFinalGolpe);

                        if (Coche.GetComponent<estadoCoche>().Golpeado)//Si nuestro coche ha sido golpeado antes, se comprueba cual ha sido el golpe potente y se resta la vida al coche que pierda
                        {
                            /*if (coche.name.Equals("coche principal") || collision.gameObject.GetComponent<colisionTrasGolpe>().Coche.name.Equals("coche principal"))
                            {
                                print("Le han dado al coche principal con un angulo de " + angulo);
                            }*/

                            print("Fuerza del golpe de "+coche.name+": "+ estadoCollision.PotenciaGolpe + "Fuerza del golpe de " + collision.gameObject.name + ": " + Coche.GetComponent<estadoCoche>().PotenciaGolpe);

                            if (estadoCollision.PotenciaGolpe >= Coche.GetComponent<estadoCoche>().PotenciaGolpe)
                            {
                                estadoCollision.bajarVida(estadoCoche.VidaMax * (1 - ((potenciaGolpe - estadoCollision.PotenciaGolpe) / potenciaGolpe)) * porcentajeMaxDaño);
                            }
                            else
                            {
                                Coche.GetComponent<estadoCoche>().bajarVida(estadoCoche.VidaMax * (1 - ((potenciaGolpe - Coche.GetComponent<estadoCoche>().PotenciaGolpe) / potenciaGolpe)) * porcentajeMaxDaño);
                            }
                        }

                        //print("Me he chocado con " + collision.gameObject.name + ". El golpe ha tenido una fuerza de " + porcentajeGolpe);


                        //La fuerza total de la colision se calcula con la dirección del golpe por la potencia por el porcentaje de velocidad que tenga el coche
                        Vector3 potenciaFinalEmpujon = calcularFuerzaEmpujon(normalCollision,potenciaFinalGolpe);

                        collision.gameObject.GetComponent<Rigidbody>().AddForce(potenciaFinalEmpujon);

                        Debug.DrawRay(collision.GetContact(0).point, normalCollision * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
                    }
                    else //Si es online
                    {
                        
                        Coche.GetComponent<estadoCoche>().darImpacto(potenciaFinalGolpe);
                        estadoCollision.recibirImpacto(potenciaFinalGolpe);
                        collision.gameObject.GetComponent<PhotonView>().RPC("comprobarGolpeMasFuerte", collision.gameObject.GetComponent<PhotonView>().Owner,potenciaFinalGolpe, 
                            normalCollision, GetComponent<PhotonView>().Owner,Coche.name);

                        GetComponent<Rigidbody>().AddForce(normalCollision * potenciaFinalGolpe);
                        collision.gameObject.GetComponent<Rigidbody>().AddForce(-normalCollision * potenciaFinalGolpe);
                    }

                }
            }
            else //Si se le da a una pared
            {
                //gameObject.GetComponent<Rigidbody>().AddForce(normalCollision * potenciaGolpe * porcentajeGolpe);
                gameObject.GetComponent<Rigidbody>().AddForce(calcularFuerzaEmpujon(-normalCollision, potenciaFinalGolpe));

                if (collision.gameObject.layer == 9)
                {
                    
                        collision.gameObject.GetComponent<colisionPared>().aumentarNumGolpes();

                        try//Buscamos el network paredes para actualizar la pared para el resto de jugadores. En offline no hace nada
                        {
                            NetworkParedes np = GameObject.Find("NetworkParedes").GetComponent<NetworkParedes>();

                            np.actualizarParedAlResto(collision.gameObject.GetComponent<colisionPared>().Id_pared);
                        }
                        catch (System.Exception e)
                        {
                            //print("No se encontró networkparedes" + e.ToString());
                        }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.InRoom && !GetComponent<PhotonView>().IsMine)
        {
            return;
        }

        if (other.gameObject.CompareTag("Muerte"))//Esto destruye al coche si sale de la arena
        {
            Coche.GetComponent<estadoCoche>().bajarVida(estadoCoche.VidaMax);
        }
        else if (other.gameObject.CompareTag("Asteroide"))
        {
            if (Coche.GetComponent<estadoCoche>().Golpeado)
            {
                return;
            }

            Vector3 direccionEmpujon = other.transform.position - gameObject.transform.position;
            direccionEmpujon.y = 0;
            direccionEmpujon = direccionEmpujon.normalized;

            Coche.GetComponent<carController>().setDrag(1);
            Coche.GetComponent<carController>().caladoPorColision();
            Coche.GetComponent<estadoCoche>().recibirImpacto(0);

            gameObject.GetComponent<Rigidbody>().AddForce(calcularFuerzaEmpujon(direccionEmpujon, potenciaGolpe));

            Coche.GetComponent<estadoCoche>().bajarVida(estadoCoche.VidaMax * porcentajeMaxDaño * 2);
        }
        else if (other.gameObject.CompareTag("Cura"))
        {
            Coche.GetComponent<estadoCoche>().aumentarVida(estadoCoche.VidaMax * porcentajeMaxDaño * 2);

            if (PhotonNetwork.InRoom)
            {
                GameObject.Find("NetworkEventos").GetComponent<NetworkEventos>().sendDestruirCura(other.transform.parent.gameObject.name);
            }
            else
            {
                Destroy(other.gameObject.transform.parent.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Fuego") && !Coche.GetComponent<estadoCoche>().Estado.Equals("quemado"))
        {
            Coche.GetComponent<estadoCoche>().empezarAQuemarse();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Fuego"))
        {
            Coche.GetComponent<estadoCoche>().Estado = "normal";
        }
    }

    private float calcularFuerzaGolpe(Vector3 normalCollision)
    {
        carController scriptCoche = Coche.GetComponent<carController>();

        //Vector3 posicionCollision = collision.gameObject.transform.position;


        //Calculamos el porcentaje del golpe segun la velocidad a la que vaya el coche o si va con nitro
        float porcentajeGolpe;

        if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.X) && scriptCoche.hayNitroCargado()) //Si hay nitro
        {
            porcentajeGolpe = 1 - ((scriptCoche.getVelMax() - scriptCoche.getNitroMax()) / (scriptCoche.getVelMax() - scriptCoche.getVelMin()));
        }
        else //Si no hay nitro
        {
            porcentajeGolpe = 1 - ((scriptCoche.getVelMax() - scriptCoche.getVelActual()) / (scriptCoche.getVelMax() - scriptCoche.getVelMin()));
        }


        //print("Velocidad actual "+coche.name+": "+ scriptCoche.getVelActual());
        //Si el golpe es demasiado flojo se ajusta para que al menos sea el minimo
        //porcentajeGolpe = porcentajeGolpe < porcentajeMinimo ? porcentajeMinimo : porcentajeGolpe;

        porcentajeGolpe = scriptCoche.getVelActual() <= scriptCoche.getVelMin() ? 0 : porcentajeGolpe;

        //La normal de la colisión será la dirección hacia donde irá el coche tras chocarse



        Vector3 vDelante = new Vector3(Coche.transform.forward.x, 0, Coche.transform.forward.z);
        float angulo = Vector3.Angle(vDelante, normalCollision);

        float porcentajeSegunLugarGolpeado = Mathf.Abs(90 - angulo) / 90f;

        float potenciaFinalGolpe = potenciaGolpe * porcentajeGolpe * porcentajeSegunLugarGolpeado;

        return potenciaFinalGolpe;
    }

    public Vector3 calcularFuerzaEmpujon(Vector3 normalCollision, float potenciaFinalGolpe)
    {
        Vector3 potenciaFinalEmpujon = -normalCollision * potenciaFinalGolpe;
        potenciaFinalEmpujon = potenciaFinalEmpujon.magnitude < potenciaGolpe * porcentajeMinimo ? -normalCollision * potenciaGolpe * porcentajeMinimo : potenciaFinalEmpujon;

        return potenciaFinalEmpujon;
    }

    [PunRPC]
    void comprobarGolpeMasFuerte(float potenciaGolpeDado, Vector3 normalCollision, Photon.Realtime.Player ultimoEnPegar, string nombreUltEnPeg)
    {
        Coche.GetComponent<carController>().setDrag(1);
        Coche.GetComponent<carController>().caladoPorColision();

        Coche.GetComponent<estadoCoche>().recibirImpacto(potenciaGolpeDado);

        float potenciaMia = calcularFuerzaGolpe(-normalCollision);

            if (potenciaGolpeDado >= potenciaMia)
            {
                Coche.GetComponent<estadoCoche>().bajarVida(estadoCoche.VidaMax * (1 - ((potenciaGolpe - potenciaGolpeDado) / potenciaGolpe)) * porcentajeMaxDaño);
            }
            else
            {
                GetComponent<PhotonView>().RPC("perdedorPierdeVida", ultimoEnPegar, Coche.GetComponent<estadoCoche>().PotenciaGolpeDado, nombreUltEnPeg);
            }

        GetComponent<Rigidbody>().AddForce(-normalCollision * potenciaGolpeDado);
    }

    [PunRPC]
    void perdedorPierdeVida(float potenciaGolpeDado, string nombreUltEnPeg)
    {
        if (!nombreUltEnPeg.Equals(Coche.name))//Solo hace daño al coche con el mismo nombre que se le manda. Esto es para evitar errores con los bots
        {
            return;
        }

        Coche.GetComponent<estadoCoche>().bajarVida(estadoCoche.VidaMax * (1 - ((potenciaGolpe - potenciaGolpeDado) / potenciaGolpe)) * porcentajeMaxDaño);
        coche.GetComponent<estadoCoche>().HeGolpeado = false;
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.InRoom)
        {
            coche.SetActive(false);
        }
    }

}
