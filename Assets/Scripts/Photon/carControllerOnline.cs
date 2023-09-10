using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class carControllerOnline : MonoBehaviour
{

    public Rigidbody theRB;

    [SerializeField] private Transform barraNitro;

    private float maxLongBarraNitro = 7.5f;

    public float forwardAccel = 8f, reverseAccel = 4f, maxSpeed = 50f, turnStrenghtNormal = 180, turnStrenghtDerrape = 80, gravityForce = 10f, dragOnGround = 3f;

    private float speedInput, turnInput, turnStrenght;

    private bool grounded;

    private float freno = 1f;

    [SerializeField] private float nitroMax = 80f;

    private float nitroCargadoFreno = 0;
    private float nitroCargadoDerrape = 0;

    private float tiempoMaxNitro = 2.5f;
    private float tiempoActualNitro = 0;

    public LayerMask whatIsGround;
    public float groundRayLength = .5f;
    public Transform groundRayPoint;

    private bool saltar = false;
    private Vector3 dirPreSalto;

    private float derrape = 0;

    private bool calado = false;
    private float tiempoMaxCalado = 2f;
    private float tiempoActualCalado = 0;

    PhotonView view;


    void Start()
    {
        theRB.transform.parent = null;
        dirPreSalto = transform.forward;
        view = GetComponent<PhotonView>();
    }

    //Aquí se van a calcular las distintas fuerzas que hay que aplicarle al coche
    private void Update()
    {
        if (view.IsMine)
        {
            //Reinicia el speedInput
            speedInput = 0;

            if (calado)
            {
                tiempoActualCalado += Time.deltaTime;

                if (tiempoActualCalado >= tiempoMaxCalado)
                {
                    calado = false;
                    tiempoActualCalado = 0;
                }
            }
            else
            {
                //Se comprueba si el jugador ha intentado frenar
                if (Input.GetKey(KeyCode.X))//Si frena, el valor del freno disminuye (El coche empieza a decelerar hasta frenar)
                {
                    freno = freno <= 0.1f ? 0 : (freno - 0.025f);
                }
                else//Si no, el valor de freno pasa a 1, por lo que el coche avanza y suelta el nitro que haya acumulado
                {
                    freno = 1f;
                    nitroCargadoFreno = nitroCargadoFreno <= 0.1f ? 0 : (nitroCargadoFreno - 0.01f);
                }

                //Comprueba si el jugador intenta acelerar
                float aceleradorPulsado = Input.GetAxis("Vertical");

                if (nitroCargadoDerrape != 0)
                {
                    nitroCargadoDerrape = nitroCargadoDerrape <= 0.01f ? 0f : (nitroCargadoDerrape - 0.01f);
                }

                //Se calcula la velocidad del coche dependiendo de la dirección a la que vaya, la aceleración y el nitro que tenga.
                if (aceleradorPulsado > 0 && (Input.GetButton("Vertical") || theRB.velocity.magnitude > 1))//Hacia delante
                {

                    float nitroTotal = aceleradorPulsado == 1 ? nitroCargadoFreno * nitroMax * 1000f : 0;
                    float nitroDerrape = nitroCargadoDerrape * nitroMax * 1000f;
                    nitroTotal = System.Math.Min(nitroDerrape + nitroTotal, nitroMax * 1000f);

                    barraNitro.localScale = new Vector3(1, 1, nitroTotal * maxLongBarraNitro / (nitroMax * 1000f));

                    if (!Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.Z))
                    {
                        speedInput = nitroTotal;
                    }
                    speedInput = aceleradorPulsado * (forwardAccel * 1000f + speedInput) * freno;

                    //Se aumenta el nitro si el freno esta puesto
                    if (freno == 0)
                    {
                        nitroCargadoFreno = nitroCargadoFreno >= 0.9f ? 1f : (nitroCargadoFreno + 0.02f);
                    }

                }
                else if (aceleradorPulsado < 0 && (Input.GetButton("Vertical") || theRB.velocity.magnitude > 1))//Hacia atras
                {
                    speedInput = aceleradorPulsado * reverseAccel * 1000f * freno;
                }

                //Comprueba si el jugador intenta girar
                turnInput = Input.GetAxis("Horizontal");

                //Comprueba si el jugador intenta hacer un derrape y cambia la fuerza de giro por la del derrape
                if (grounded && Input.GetKey(KeyCode.Z))
                {
                    turnStrenght = turnStrenghtDerrape;
                }
                else
                {
                    turnStrenght = turnStrenghtNormal;
                }
            }
            //Se calcula la rotación del coche
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrenght * Time.deltaTime * Input.GetAxis("Vertical"), 0f));

            //El se pone el modelo del coche en el mismo lugar donde esté su rigidbody
            transform.position = theRB.transform.position;

            //Sirve para activar el salto del derrape
            if (Input.GetKeyDown(KeyCode.Z))
            {
                saltar = true;
            }

            if (barraNitro.localScale.z >= maxLongBarraNitro - 1)
            {
                tiempoActualNitro += Time.deltaTime;
            }
            else
            {
                tiempoActualNitro = 0;
            }

            if (tiempoActualNitro >= tiempoMaxNitro)
            {
                calado = true;
                nitroCargadoDerrape = 0;
                nitroCargadoFreno = 0;
                tiempoActualNitro = 0;
            }
        }
        


    }

    // Aquí se aplican las fuerzas al coche
    void FixedUpdate()
    {
        grounded = false;
        RaycastHit hit;

        //Raycast que comprueba si el coche está en el suelo
        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;
        }

        //Si el coche está en el suelo, se le aplican las fuerzas correpondientes calculadas en el método update
        if (grounded)
        {
            //Se le pone el drag correspondiente al suelo al rigidbody
            theRB.drag = dragOnGround;

            //Se acelera el coche según la velocidad calculada
            if (Mathf.Abs(speedInput) > 0)
            {
                //theRB.AddForce(transform.forward * speedInput * (1- derrape));
                theRB.AddForce(transform.forward * speedInput);
            }

            bool derrapePresionado = Input.GetKey(KeyCode.Z);
            //Sirve para el derrape del coche
            if (saltar == true)//El coche salta
            {
                theRB.AddForce(Vector3.up * gravityForce * 10000);
                dirPreSalto = transform.forward;
                saltar = false;
                derrape = 0.5f;
            }
            else if (derrapePresionado && turnInput != 0 && derrape > 0)//El coche derrapa mientras se mantenga pulsada la tecla
            {
                //theRB.AddForce(dirPreSalto * speedInput * derrape * 3);
                theRB.AddForce(dirPreSalto * speedInput * derrape);

                derrape = derrape < 0.1f ? 0 : derrape - 0.001f;
                nitroCargadoDerrape = nitroCargadoDerrape >= 0.9f ? 1f : (nitroCargadoDerrape + 0.05f);
            }
            else if (!derrapePresionado)//El coche deja de derrapar
            {
                derrape = 0;
            }
        }
        else//Si el coche está en el aire, se le cambia el drag y se la aplica una fuerza hacia abajo para que caiga
        {
            theRB.drag = 0.1f;
            theRB.AddForce(Vector3.up * -gravityForce * 1000);
        }


    }
}
