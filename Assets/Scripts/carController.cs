using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class carController : MonoBehaviour
{

    [SerializeField] private Rigidbody theRB;

    [SerializeField] private Transform barraNitro;

    private float maxLongBarraNitro = 7.5f;

    [SerializeField] private float actualForwardSpeed = 8f, reverseAccel = 4f, maxSpeed = 200f, turnStrenghtNormal = 180, turnStrenghtDerrape = 80, gravityForce = 10f, dragOnGround = 3f;

    private float minSpeed = 50f, acceleration = 75f;

    private float dragActual;

    private float speedInput, turnInput, turnStrenght;

    private bool grounded = false;

    private float freno = 1f;

    [SerializeField] private float nitroMax = 80f;

    private float nitroCargadoFreno = 0;
    private float nitroCargadoDerrape = 0;

    private float tiempoMaxNitro = 2.5f;
    private float tiempoActualNitro = 0;

    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundRayLength = .5f;
    [SerializeField] private Transform[] groundRayPoint = new Transform[2];

    private bool saltar = false;
    private Vector3 dirPreSalto;

    private float derrape = 0;

    private bool calado = false;
    private float tiempoMaxCalado = 2f;
    private float tiempoActualCalado = 0;

    [SerializeField] private bool esUnBot = false;
    private enum Heuristica { basica, nitroFreno,evitacionPeligro, a };
    [SerializeField] private Heuristica heuristicaElegida;

    //Variables para los bots

    private Vector2 objetivo = new Vector2(9999, 9999);

    //Variables exclusivas del online

    private bool cocheDesconectado = false;


    public bool EsUnBot { get => esUnBot; set => esUnBot = value; }
    public Rigidbody TheRB { get => theRB; set => theRB = value; }
    public bool CocheDesconectado { get => cocheDesconectado; set => cocheDesconectado = value; }
    public bool Calado { get => calado; set => calado = value; }

    //public string[] Heuristica { get => heuristica; set => heuristica = value; }
    //public Heuristica HeuristicaElegida { get => heuristicaElegida; set => heuristicaElegida = value; }

    void Start()
    {
        if (!PhotonNetwork.InRoom)//theRB!=null)
        {
            TheRB.transform.parent = null;
        }
        else
        {
            //TheRB = GameObject.Find("Collider_auxiliar").GetComponent<Rigidbody>();
            //TheRB.transform.gameObject.SetActive(false);
            //TheRB = GameObject.Find("C-"+PhotonNetwork.).GetComponent<Rigidbody>();
            this.transform.parent = null;
        }

        this.gameObject.GetComponent<estadoCoche>().Collider = theRB.gameObject;

        dirPreSalto = transform.forward;
        dragActual = 0.1f;

        actualForwardSpeed = minSpeed;
    }

    //Aquí se van a calcular las distintas fuerzas que hay que aplicarle al coche
    private void Update()
    {
        if (CocheDesconectado)
        {
            return;
        }

        if (Calado)
        {
            actualForwardSpeed = minSpeed;
            speedInput = 0;

            nitroCargadoFreno = 0;
            nitroCargadoDerrape = 0;
            tiempoActualNitro = 0;

            theRB.constraints = RigidbodyConstraints.FreezeRotation;

            tiempoActualCalado += Time.deltaTime;

            if (tiempoActualCalado >= tiempoMaxCalado)
            {
                Calado = false;
                tiempoActualCalado = 0;
                dragActual = dragOnGround;
                theRB.constraints = RigidbodyConstraints.None;
                theRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
        else
        {
            if (esUnBot)
            {
                switch (heuristicaElegida)
                {
                    case Heuristica.basica:
                        heuristicaBasica();
                        break;
                    case Heuristica.nitroFreno:
                        heuristicaNitroFreno();
                        break;
                    case Heuristica.evitacionPeligro:
                        heuristicaEvitacionPeligro();
                        break;
                    case Heuristica.a:
                        speedInput = 0;
                        break;
                    default:
                        //print("La heuristica "+ heuristicaElegida + " no es valida.");
                        break;
                }
            }
            else
            {
                controlJugador();
            }

        }


        if (-0.1f <= speedInput && speedInput <= 0.1f) // Si el coche no se pone en movimiento, se le baja la velocidad al minimo
        {
            actualForwardSpeed = minSpeed;
        }
        else if(!Input.GetKey(KeyCode.Z) || EsUnBot)// Se aumenta la velocidad del coche con el tiempo
        {
            actualForwardSpeed += acceleration * Time.deltaTime;
        }
        actualForwardSpeed = actualForwardSpeed > maxSpeed ? maxSpeed : actualForwardSpeed;

        TheRB.transform.rotation = transform.rotation;
        //Se pone el modelo del coche en el mismo lugar donde esté su rigidbody
        transform.position = TheRB.transform.position;

    }

    private void controlJugador()
    {
        //Reinicia el speedInput
        speedInput = 0;


        //Se comprueba si el jugador ha intentado frenar
        if (Input.GetKey(KeyCode.X))//Si frena, el valor del freno disminuye (El coche empieza a decelerar hasta frenar)
        {
            freno = freno - 7f * Time.deltaTime;
            freno = freno <= 0.1f ? 0 : freno;
        }
        else//Si no, el valor de freno pasa a 1, por lo que el coche avanza y suelta el nitro que haya acumulado
        {
            freno = 1f;
            nitroCargadoFreno = nitroCargadoFreno - 2f * Time.deltaTime;
            nitroCargadoFreno = nitroCargadoFreno <= 0.1f ? 0 : nitroCargadoFreno;
        }

        //Comprueba si el jugador intenta acelerar
        float aceleradorPulsado = Input.GetAxis("Vertical");

        if (!Input.GetKey(KeyCode.Z) && nitroCargadoDerrape != 0)//Se gasta el nitro del derrape
        {
            nitroCargadoDerrape = nitroCargadoDerrape - 2f * Time.deltaTime;
            nitroCargadoDerrape = nitroCargadoDerrape <= 0.01f ? 0f : nitroCargadoDerrape;
        }

        //Se calcula la velocidad del coche dependiendo de la dirección a la que vaya, la aceleración y el nitro que tenga.
        if (aceleradorPulsado > 0 && (Input.GetButton("Vertical") || TheRB.velocity.magnitude > 1))//Hacia delante
        {

            calcularSpeedInput(aceleradorPulsado);

            aumentarNitroFreno();

        }
        else if (aceleradorPulsado < 0 && (Input.GetButton("Vertical") || TheRB.velocity.magnitude > 1))//Hacia atras
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

        //Se calcula la rotación del coche
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrenght * Time.deltaTime * Input.GetAxis("Vertical"), 0f));
        //theRB.transform.rotation = transform.rotation;

        //Sirve para activar el salto del derrape
        if (Input.GetKeyDown(KeyCode.Z))
        {
            saltar = true;
        }

        comprobarTiempoNitroCargado();
    }

    //El coche irá a lugares aleatorios dentro del mapa
    private void heuristicaBasica()
    {
        speedInput = actualForwardSpeed * 1000f;

        if (objetivo.x >= 5000)//Si el objetivo está a mas de x=5000 se considera que no hay objetivo y se añade uno dentro del rango normal
        {
            GameObject limites = GameObject.Find("limites_mapa");

            Transform limite1 = limites.transform.GetChild(0);
            Transform limite2 = limites.transform.GetChild(1);

            float minX = Mathf.Min(limite1.position.x, limite2.position.x);
            float maxX = Mathf.Max(limite1.position.x, limite2.position.x);
            float minZ = Mathf.Min(limite1.position.z, limite2.position.z);
            float maxZ = Mathf.Max(limite1.position.z, limite2.position.z);


            objetivo = new Vector2(Random.Range(minX, maxX), Random.Range(minZ, maxZ));
            //print(objetivo);
        }
        else // Si hay objetivo se intenta llegar hasta él
        {
            Vector2 vPosicionObjetivo = objetivo - new Vector2(gameObject.transform.position.x, gameObject.transform.position.z); //Vector de nuestra posición hasta el objetivo

            Vector2 vParteDelantera = new Vector2(gameObject.transform.forward.x, gameObject.transform.forward.z); // Vector de donde está mirando el coche

            //float valorResta = (vPosicionObjetivo.normalized - vParteDelantera).magnitude; //Indica que tanto hay que girar

            Vector2 valorResta = (vPosicionObjetivo.normalized - vParteDelantera);

            if (valorResta.magnitude > 0.1) //El coche gira hasta que consiga alinearse lo suficiente al objetivo
            {
                int direccionGiro = 1;

                //Para averiguar la direccion del giro es necesario obtener el valor resta de la parte delantera con el opuesto del vector objetivo

                Vector2 valorRestaOpuesto = ((vPosicionObjetivo*-1).normalized - vParteDelantera);

                //Al calcularse con vectores unitarios tanto valorResta como valorRestaOpuesto, solo será necesario saber si la x de uno y la y del otro tienen el mismo signo
                //para determinar la dirección del giro
                if (valorResta.x * valorRestaOpuesto.y > 0)
                {
                    direccionGiro = -1;
                }

                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnStrenghtNormal * Time.deltaTime * direccionGiro, 0f));
            }

            if (vPosicionObjetivo.magnitude < 75) //Si la distancia es menor el umbral que le pongamos, se considera que se ha llegado al objetivo
            {
                objetivo = new Vector2(5000, 5000);
            }
        }

    }

    private bool objetivoFijado = false;
    public void heuristicaNitroFreno()
    {
        //Reinicia el speedInput
        speedInput = 0;

        //Aumenta el valor del freno
        freno = freno - 7f * Time.deltaTime;
        freno = freno <= 0.1f ? 0 : freno;
        aumentarNitroFreno();


        if (objetivoFijado)//Si pasa alguien por delante del coche, se suelta el freno
        {
            freno = 1f;
            nitroCargadoFreno = nitroCargadoFreno - 2f * Time.deltaTime;
            nitroCargadoFreno = nitroCargadoFreno <= 0.1f ? 0 : nitroCargadoFreno;

            heuristicaBasica();//El coche seguira una heuristica básica hasta que decida frenar de nuevo
        }
        else // Si no, se queda esperando, cargando el nitro
        {
            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Coche");
            if (Physics.Raycast(groundRayPoint[2].position, transform.forward, out hit, 100, mask)) //Si se choca el rayo con algun coche, se suelta el freno
            {
                //print("Objetivo: " + hit.collider.transform.position);
                if (hit.collider.gameObject.GetInstanceID() != TheRB.gameObject.GetInstanceID())
                {
                    objetivoFijado = true;
                    StartCoroutine("reactivacionHeuristicaNitroFreno", Random.Range(0.5f,3));
                }
            }
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnStrenghtNormal * Time.deltaTime * 1, 0f));
        }

        calcularSpeedInput(1);

        comprobarTiempoNitroCargado(); //Esto cala el coche si esta mucho tiempo cargando el nitro

        if (tiempoActualNitro >= tiempoMaxNitro*0.90f)// Se suelta el freno antes de que se cale
        {
            objetivoFijado = true;
            StartCoroutine("reactivacionHeuristicaNitroFreno", Random.Range(0.5f, 3));
        }
    }


    private string direccion = "recto";
    public string Direccion { get => direccion; set => direccion = value; }
    public void heuristicaEvitacionPeligro()
    {
        speedInput = actualForwardSpeed * 1000f;

        float direccionGiro = 0;

        switch (direccion)
        {
            case "recto":
                direccionGiro = 0;
                break;
            case "izquierda":
                direccionGiro = -1;
                break;
            case "derecha":
                direccionGiro = 1;
                break;
            case "atras":
                direccionGiro = 1;
                speedInput = -reverseAccel * 1000f;
                break;
            case "parar":
                direccionGiro = 0;
                speedInput = 0;
                break;
            default:
                direccionGiro = 0;
                break;
        }

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnStrenghtNormal * Time.deltaTime * direccionGiro, 0f));
    }

    IEnumerator reactivacionHeuristicaNitroFreno(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        objetivoFijado = false;
    }

    // Aquí se aplican las fuerzas al coche
    void FixedUpdate()
    {
        if (CocheDesconectado)
        {
            return;
        }

        //grounded = false;
        RaycastHit hit;
        bool rayoChoca = false;

        foreach (Transform rayo in groundRayPoint)
        {
            if (Physics.Raycast(rayo.position, -transform.up, out hit, groundRayLength, whatIsGround))
            {
                rayoChoca = true;
                break;
            }
        }

        //Raycast que comprueba si el coche está en el suelo
        if (rayoChoca)
        {
            if (!grounded)
            {
                dragActual = dragOnGround;
            }
            grounded = true;
        }
        else
        {
            grounded = false;
            dragActual = 0.1f;
        }

        TheRB.drag = dragActual;

        //Si el coche está en el suelo, se le aplican las fuerzas correpondientes calculadas en el método update
        if (grounded)
        {
            //Se le pone el drag correspondiente al suelo al rigidbody
            TheRB.drag = dragActual;

            //Se acelera el coche según la velocidad calculada
            if (Mathf.Abs(speedInput) > 0)
            {
                //theRB.AddForce(transform.forward * speedInput * (1- derrape));
                TheRB.AddForce(transform.forward * speedInput);
            }

            bool derrapePresionado = Input.GetKey(KeyCode.Z);
            //Sirve para el derrape del coche
            if (saltar == true)//El coche salta
            {
                TheRB.AddForce(Vector3.up * gravityForce * 10000);
                dirPreSalto = transform.forward;
                saltar = false;
                derrape = 0.5f;
            }
            else if (derrapePresionado && turnInput != 0 && derrape > 0)//El coche derrapa mientras se mantenga pulsada la tecla
            {
                //theRB.AddForce(dirPreSalto * speedInput * derrape * 3);
                TheRB.AddForce(dirPreSalto * speedInput * derrape);

                derrape = derrape < 0.1f ? 0 : derrape - 0.001f;
                nitroCargadoDerrape = nitroCargadoDerrape >= 0.9f ? 1f : (nitroCargadoDerrape + 2f * Time.deltaTime);
            }
            else if (!derrapePresionado)//El coche deja de derrapar
            {
                derrape = 0;
            }
        }
        else//Si el coche está en el aire, se le cambia el drag y se la aplica una fuerza hacia abajo para que caiga
        {
            //theRB.drag = 0.1f;
            TheRB.AddForce(Vector3.up * -gravityForce * 1000);
        }
    }

    //Función que permite empujar a los coches cuando estos se chocan
    /*private void OnTriggerEnter(Collider cocheChocado)
    {
        if (cocheChocado.gameObject.layer != 7)
        {
            //Debug.Log("No me he chocado");
        }
        else
        {
            //Debug.Log("Me he chocado");

            cocheChocado.gameObject.GetComponent<Rigidbody>().drag = 1;
            cocheChocado.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * speedInput * 2);
            theRB.AddForce(-transform.forward * speedInput * 2);

            calado = true;
            tiempoActualCalado = 1.75f;
        }
    }*/

    private void OnCollisionEnter(Collision collision)
    {
        print("Me he chocado");
    }

    public void setHeuristica(string h)
    {
        switch (h)
        {
            case "basica":
                heuristicaElegida = Heuristica.basica;
                break;
            case "nitroFreno":
                heuristicaElegida = Heuristica.nitroFreno;
                break;
            case "evitacionPeligro":
                heuristicaElegida = Heuristica.evitacionPeligro;
                break;

            default:
                heuristicaElegida = Heuristica.a;
                break;
        }
    }

    public void caladoPorColision()
    {
        Calado = true;
        tiempoActualCalado = 1.75f; //Son 0.25s
    }

    public void calcularSpeedInput(float aceleradorPulsado)
    {
        float nitroTotal = aceleradorPulsado == 1 ? nitroCargadoFreno * nitroMax * 1000f : 0;
        float nitroDerrape = nitroCargadoDerrape * nitroMax * 1000f;
        nitroTotal = System.Math.Min(nitroDerrape + nitroTotal, nitroMax * 1000f);

        barraNitro.localScale = new Vector3(1, 1, nitroTotal * maxLongBarraNitro / (nitroMax * 1000f));

        if (!Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.Z))
        {
            speedInput = nitroTotal;
        }
        speedInput = aceleradorPulsado * (actualForwardSpeed * 1000f + speedInput) * freno;
    }

    public void aumentarNitroFreno()
    {
        //Se aumenta el nitro si el freno esta puesto
        if (freno == 0)
        {
            nitroCargadoFreno = nitroCargadoFreno + 4f * Time.deltaTime;
            nitroCargadoFreno = nitroCargadoFreno >= 0.9f ? 1f : nitroCargadoFreno;
        }
    }

    public bool hayNitroCargado()
    {
        return (nitroCargadoFreno != 0 || nitroCargadoDerrape != 0);
    }

    public void comprobarTiempoNitroCargado()
    {
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
            Calado = true;
            nitroCargadoDerrape = 0;
            nitroCargadoFreno = 0;
            tiempoActualNitro = 0;
        }
    }

    public void setDrag(float drag)
    {
        dragActual = drag;
    }

    public float getVelActual()
    {
        return actualForwardSpeed;
    }

    public float getVelMax()
    {
        return maxSpeed;
    }

    public float getVelMin()
    {
        return minSpeed;
    }

    public float getNitroMax()
    {
        return nitroMax;
    }


}
