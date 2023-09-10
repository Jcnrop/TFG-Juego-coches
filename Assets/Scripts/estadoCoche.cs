using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class estadoCoche : MonoBehaviour
{
    public ParticleSystem particulasHumo;
    public float vida;
    public Material[] coloresPosibles;
    static private float vidaMax = 100;
    private int color = 0;
    private string estado = "normal";

    private bool golpeado = false; //Esta variable sirve para que haya delay entre los golpes que recibe el coche. Indica si el coche ha sudo golpeado
    private float delayGolpe = 0.05f;
    private float potenciaGolpe = 0;

    private bool esCochePrincipal = false;

    //Variables exclusivas para el online

    private bool heGolpeado = false;
    private float potenciaGolpeDado = 0;
    private Vector3 normalGolpe = new Vector3(-999,-999,-999);

    public static float VidaMax { get => vidaMax; set => vidaMax = value; }
    public bool Golpeado { get => golpeado; set => golpeado = value; }
    public float PotenciaGolpe { get => potenciaGolpe; set => potenciaGolpe = value; }
    public bool EsCochePrincipal { get => esCochePrincipal; set => esCochePrincipal = value; }
    public GameObject Collider { get; set; }
    public float Vida { get => vida; set => vida = value; }
    public bool HeGolpeado { get => heGolpeado; set => heGolpeado = value; }
    public float PotenciaGolpeDado { get => potenciaGolpeDado; set => potenciaGolpeDado = value; }
    public int Color { get => color; set => color = value; }
    public string Estado { get => estado; set => estado = value; }

    // Start is called before the first frame update
    void Start()
    {
        Vida = VidaMax;

        cambiarColorCarcasa(Random.Range(0, coloresPosibles.Length));

        if (PhotonNetwork.InRoom)//Si el jugador está en una sala, el coche se añade automaticamente a la lista de coches del controller
        {
            this.gameObject.name = Collider.GetComponent<PhotonView>().Owner.NickName;
            this.gameObject.name += Collider.GetComponent<PhotonView>().IsRoomView ? "-bot"+ gameObject.GetInstanceID() : "";

            print("Se inicializa el coche de "+ this.gameObject.name);
            GameObject.Find("Controller").GetComponent<generarCoches>().añadirCoche(this.gameObject);

            /*Collider = GameObject.Find("C-" + this.gameObject.name);
            this.gameObject.GetComponent<carController>().TheRB = Collider.GetComponent<Rigidbody>();*/
        }
    }


    private bool mensajeMuerteEnviado = false;
    // Update is called once per frame
    void Update()
    {
        if (Vida <= 0)
        {
            if (PhotonNetwork.InRoom && Collider.GetComponent<PhotonView>().IsMine && !mensajeMuerteEnviado)
            {
                mensajeMuerteEnviado = true;
                //Collider.GetComponent<PhotonView>().RPC("desactivarCocheOnline", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.UserId);
                GameObject.Find("NetworkEventos").GetComponent<NetworkEventos>().sendDestruirCocheJugador(this.gameObject.name);
            }
            else if (!PhotonNetwork.InRoom)
            {
                desactivarCoche();
            }
        }
        //bajarVida(0);
        actualizarHumo();
    }

    public void desactivarCoche()
    {
        //Destroy(this.gameObject);
        this.gameObject.SetActive(false);
        print(Collider.name);
        Collider.SetActive(false);
    }

    public void bajarVida(float cantidad)
    {
        Vida -= cantidad;

        if (esCochePrincipal)
        {
            cameraFollow camara = GameObject.Find("/Directional Light/Main Camera").GetComponent<cameraFollow>();
            camara.mostrarDaño();
            camara.startShake(0.15f,0.4f);
        }

        actualizarHumo();
    }

    public void aumentarVida(float cantidad)
    {
        Vida += cantidad;
        Vida = Vida > vidaMax ? vidaMax : Vida;

        if (esCochePrincipal)
        {
            cameraFollow camara = GameObject.Find("/Directional Light/Main Camera").GetComponent<cameraFollow>();
            camara.mostrarCuracion();
        }
        actualizarHumo();
    }

    public void actualizarHumo()
    {
        var emision = particulasHumo.emission;
        var main = particulasHumo.main;
        var colorHumo = particulasHumo.colorOverLifetime;

        //GRADIENTES DEL HUMO

        Gradient gradiente = new Gradient();

        //HUMO NORMAL

        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = new UnityEngine.Color(0.8018868f, 0.8018868f, 0.8018868f, 1);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new UnityEngine.Color(0.3773585f, 0.3773585f, 0.3773585f, 1);
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        //HUMO NEGRO

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey2 = new GradientColorKey[3];
        colorKey2[0].color = UnityEngine.Color.yellow;
        colorKey2[0].time = 0.0f;
        colorKey2[1].color = UnityEngine.Color.red;
        colorKey2[1].time = 0.5f;
        colorKey2[2].color = UnityEngine.Color.black;
        colorKey2[2].time = 0.75f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey2 = new GradientAlphaKey[2];
        alphaKey2[0].alpha = 1.0f;
        alphaKey2[0].time = 0.0f;
        alphaKey2[1].alpha = 1.0f;
        alphaKey2[1].time = 1.0f;

        //Se cambia el humo según la vida que tenga el coche
        if (Vida <= VidaMax && Vida > VidaMax * 0.75)
        {
            emision.rateOverTime = 0;
            main.startSize = 5;
            gradiente.SetKeys(colorKey, alphaKey);
            colorHumo.color = gradiente;
        }
        else if (Vida <= VidaMax * 0.75 && Vida > VidaMax * 0.5)
        {
            emision.rateOverTime = 50;
            main.startSize = 5;
            gradiente.SetKeys(colorKey, alphaKey);
            colorHumo.color = gradiente;
        }
        else if (Vida <= VidaMax * 0.5 && Vida > VidaMax * 0.25)
        {
            emision.rateOverTime = 100;
            main.startSize = 10;
            gradiente.SetKeys(colorKey, alphaKey);
            colorHumo.color = gradiente;
        }
        else if (Vida <= VidaMax * 0.25)
        {
            emision.rateOverTime = 100;
            main.startSize = 10;
            gradiente.SetKeys(colorKey2, alphaKey2);
            colorHumo.color = gradiente;
        }
    }

    //Esta función sirve para que no haya varios impactos a la vez
    public void recibirImpacto(float potencia)
    {
        Golpeado = true;
        potenciaGolpe = potencia;
        StartCoroutine("startDelay");

    }

    //Esta función es exclusiva del online.
    public void darImpacto(float potencia)
    {
        HeGolpeado = true;
        PotenciaGolpeDado = potencia;
    }

    public void cambiarColorCarcasa(int colorNuevo)
    {
        GameObject carcasa = transform.Find("Carcasa/Body/Sedan").gameObject;
        Color = colorNuevo;
        carcasa.GetComponent<MeshRenderer>().material = coloresPosibles[Color];
    }

    IEnumerator startDelay()
    {
        yield return new WaitForSeconds(delayGolpe);
        Golpeado = false;
        potenciaGolpe = 0;
    }


    private bool yaMeQuemo = false;
    public void empezarAQuemarse()
    {
        estado = "quemado";
        if (!yaMeQuemo)
        {
            StartCoroutine("quemarse");
        }
    }

    IEnumerator quemarse()
    {
        yaMeQuemo = true;

        float tiempoQuemadura = 0.5f;
        float tiempoPasado = 0.35f;

        while (Estado.Equals("quemado"))
        {
            tiempoPasado += Time.deltaTime;

            if (tiempoPasado>= tiempoQuemadura)
            {
                tiempoPasado = 0;

                bajarVida(VidaMax * 0.05f);
            }

            yield return null;
        }

        yaMeQuemo = false;
    }


    [SerializeField] private GameObject prefabExplosion;
    private bool quitting = false;
    private void OnDisable()
    {
        if (quitting || generarCoches.cargarMenu)
        {
            return;
        }

        if (EsCochePrincipal)
        {
            GameObject.Find("/Directional Light/Main Camera").GetComponent<cameraFollow>().cambiarObjetivoCamara(3,1);
        }

        GameObject.Find("Controller").GetComponent<generarCoches>().comprobarGanador();

        Instantiate(prefabExplosion, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    [PunRPC]
    void desactivarCocheOnline(string IdJugador)
    {
        print("¿Tengo que desactivar mi coche?");
        if (IdJugador.Equals(Collider.GetComponent<PhotonView>().Owner.UserId))
        {
            print("Voy a desactivar mi coche");
            desactivarCoche();
        }
        print("No voy a desactivar mi coche");
    }
}
