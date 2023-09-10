using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class generadorSuelo : MonoBehaviour
{
    [SerializeField] GameObject prefabSuelo;
    GameObject[] instanciasSuelo = null;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    int filas = 51;
    int columnas = 51;

    int filasInstancias;
    int columnasInstancias;

    public GameObject[] InstanciasSuelo { get => instanciasSuelo; set => instanciasSuelo = value; }
    public int Filas { get => filas; set => filas = value; }

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        //var prueba = gameObject.GetComponent<quitaCasillas>().minDistance;

        filasInstancias = Filas - 1;
        columnasInstancias = columnas - 1;

        Debug.Log("Tiempo al comienzo de la aplicacion: " + Time.realtimeSinceStartup);

        MakeMeshData();

        Debug.Log("Tiempo tras instanciar los trozos: " + Time.realtimeSinceStartup);
        createMesh();

        gameObject.GetComponent<quitaCasillas>().InstanciasSuelo = instanciasSuelo;
        gameObject.GetComponent<quitaCasillas>().inicializar();

    }

    void MakeMeshData()
    {
        InstanciasSuelo = new GameObject[(Filas - 1) * (columnas - 1)];
        int indice = 0;

        vertices = new Vector3[Filas * columnas];
        triangles = new int[(Filas - 1) * (columnas - 1) * 6];
        uvs = new Vector2[vertices.Length];

        int t = 0;

        int colorActual = 0;

        for (int i = 0; i < Filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                int posXVert = j - columnas / 2;
                int posZVert = -i + Filas / 2;

                vertices[i * columnas + j] = new Vector3(posXVert, 0, posZVert);

                if (i != Filas - 1 && j != columnas - 1)
                {
                    triangles[t] = i * columnas + j;
                    triangles[t + 1] = i * columnas + j + columnas + 1;
                    triangles[t + 2] = i * columnas + j + columnas;
                    triangles[t + 3] = i * columnas + j;
                    triangles[t + 4] = i * columnas + j + 1;
                    triangles[t + 5] = i * columnas + j + columnas + 1;

                    t += 6;

                    GameObject instanciaSuelo = Instantiate(prefabSuelo, new Vector3(0, 0, 0), Quaternion.identity, GameObject.Find("generadorSuelo").transform);
                    instanciaSuelo.transform.localPosition = new Vector3(posXVert + 0.5f, -0.5f, posZVert - 0.5f);

                    Color[] colores = { new Color32(255, 168, 13, 255), new Color32(232, 143, 12, 255), new Color32(255, 136, 0, 255), new Color32(232, 114, 12, 255), new Color32(255,101,0, 255) };
                    Material materialNuevo = instanciaSuelo.GetComponentInChildren<MeshRenderer>().materials[0];
                    materialNuevo.color = colores[Random.Range(0,colores.Length)];
                    //materialNuevo.color = colores[colorActual];
                    colorActual = (colorActual + 1) % colores.Length;

                    int[] auxTiling = { -1, 1 };
                    float numRandom = Random.Range(-1.00f, 1.00f);
                    materialNuevo.mainTextureScale = new Vector2(auxTiling[Random.Range(0, 2)], auxTiling[Random.Range(0, 2)]);
                    //materialNuevo.mainTextureScale = new Vector2(0.5f, 0.5f);
                    //materialNuevo.mainTextureOffset = new Vector2(numRandom, numRandom);

                    InstanciasSuelo[indice] = instanciaSuelo;
                    indice++;

                }

                uvs[i * columnas + j] = new Vector2(1 - ((float)j / (columnas - 1)), (float)i / (Filas - 1));
            }
        }

    }

    void createMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    public void instanciasSueloCaen()
    {
        List<GameObject> instanciasSueloNuevo=new List<GameObject>();

        for (int j = 0; j < InstanciasSuelo.Length; j++)
        {
            if (j >= 0 && j < columnasInstancias || j % columnasInstancias == 0 || j % columnasInstancias == columnasInstancias - 1 ||
                j >= columnasInstancias * (filasInstancias - 1) && j < columnasInstancias  * filasInstancias)
            {
                InstanciasSuelo[j].GetComponentInChildren<Animator>().enabled = true;
            }
            else
            {
                instanciasSueloNuevo.Add(InstanciasSuelo[j]);
            }
        }

        filasInstancias -= 2;
        columnasInstancias -= 2;

        InstanciasSuelo = instanciasSueloNuevo.ToArray();
    }

}
