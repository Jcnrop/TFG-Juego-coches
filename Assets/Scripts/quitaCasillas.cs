using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class quitaCasillas : MonoBehaviour
{
    public float minDistance;
    public Transform trackedObject;

    public MeshFilter filter;

    public MeshCollider meshCollider;

    Mesh mesh;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;
    int[] triangles;
    bool[] trianglesDisabled;
    bool[] verticesDisabled;
    List<int>[] trisWithVertex;

    Vector3[] origvertices;
    Vector3[] orignormals;
    Vector2[] origuvs;
    int[] origtriangles;

    int nColumnasVert;
    int nFilasVert;

    GameObject[] instanciasSuelo;

    Thread[] hilos;
    public GameObject[] InstanciasSuelo { get => instanciasSuelo; set => instanciasSuelo = value; }

    public void inicializar()
    {
        //generadorSuelo asasa;
        //var prueba = asasa.getColumnas();
        //instanciasSuelo = gameObject.GetComponent<generadorSuelo>().InstanciasSuelo;

        mesh = new Mesh();
        filter = GetComponent<MeshFilter>();
        orignormals = filter.mesh.normals;
        origvertices = filter.mesh.vertices;
        origuvs = filter.mesh.uv;
        origtriangles = filter.mesh.triangles;

        vertices = new Vector3[origvertices.Length];
        normals = new Vector3[orignormals.Length];
        uvs = new Vector2[origuvs.Length];
        triangles = new int[origtriangles.Length];
        trianglesDisabled = new bool[origtriangles.Length];
        verticesDisabled = new bool[origvertices.Length];

        orignormals.CopyTo(normals, 0);
        origvertices.CopyTo(vertices, 0);
        origtriangles.CopyTo(triangles, 0);
        origuvs.CopyTo(uvs, 0);

        actualizarNFilasColumnas();

        Debug.Log("Tiempo antes de calcular filas y columnas: " + Time.realtimeSinceStartup);
        //StartCoroutine(calcularTrisWithVertex());

        trisWithVertex = new List<int>[origvertices.Length];// Aquí se guarda un array donde cada elemento es de un vertice y guarda una lista de los indices en triangles de los triangulos en los que participa

        int nThreads = 2;
        hilos = new Thread[nThreads];

        for (int i = 0; i < nThreads; i++)
        {
            int aux = i; //No se puede usar la i porque cuando cambia su valor tambien cambia en el resto de hilos ya creados
            Thread t = new Thread(() => calcularTrisWithVertex(aux, nThreads));
            hilos[i] = t;
            //t.Start();
        }

        foreach (Thread hilo in hilos)
        {
            hilo.Start();
        }


        Debug.Log("Tiempo despues de calcular filas y columnas: " + Time.realtimeSinceStartup);

        filter.mesh = GenerateMeshWithHoles();
        meshCollider.sharedMesh = filter.mesh;

        StartCoroutine(esperarEliminarCasillas());
    }

    public void Remesh()
    {
        filter.mesh = GenerateMeshWithHoles();
        meshCollider.sharedMesh = filter.mesh;
    }

    Mesh GenerateMeshWithHoles()
    {
        Vector3 trackPos = trackedObject.position;
        for (int i = 0; i < origvertices.Length; ++i)
        {
            Vector3 v = new Vector3(origvertices[i].x * transform.localScale.x, origvertices[i].y * transform.localScale.y, origvertices[i].z * transform.localScale.z);
            if ((v + transform.position - trackPos).magnitude < minDistance)
            {
                for (int j = 0; j < trisWithVertex[i].Count; ++j)
                {
                    int value = trisWithVertex[i][j];
                    int remainder = value % 3;
                    trianglesDisabled[value - remainder] = true;
                    trianglesDisabled[value - remainder + 1] = true;
                    trianglesDisabled[value - remainder + 2] = true;
                }
            }
        }
        triangles = origtriangles;
        triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();

        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        for (int i = 0; i < trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;
        return mesh;
    }
    Mesh GenerateMeshWithFakeHoles()
    {
        Vector3 trackPos = trackedObject.position;
        for (int i = 0; i < origvertices.Length; ++i)
        {
            if ((origvertices[i] + transform.position - trackPos).magnitude < minDistance)
            {
                normals[i] = -orignormals[i];
            }
            else
            {
                normals[i] = orignormals[i];
            }
        }
        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        return mesh;
    }

    private void actualizarNFilasColumnas()
    {
        Vector3 vert1 = new Vector3(vertices[0].x * transform.localScale.x, vertices[0].y * transform.localScale.y, vertices[0].z * transform.localScale.z);
        Vector3 vert2 = new Vector3(vertices[1].x * transform.localScale.x, vertices[1].y * transform.localScale.y, vertices[1].z * transform.localScale.z);
        Vector3 vectFila = vert2 - vert1;
        Vector3 vectColumna;

        for (int i = 1; i < vertices.Length; ++i)
        {
            vert1 = new Vector3(vertices[i].x * transform.localScale.x, vertices[i].y * transform.localScale.y, vertices[i].z * transform.localScale.z);
            vert2 = new Vector3(vertices[i + 1].x * transform.localScale.x, vertices[i + 1].y * transform.localScale.y, vertices[i + 1].z * transform.localScale.z);
            Vector3 vPrueba = vert2 - vert1;
            if (vectFila != vPrueba)
            {
                vert1 = new Vector3(vertices[0].x * transform.localScale.x, vertices[0].y * transform.localScale.y, vertices[0].z * transform.localScale.z);
                vectColumna = vert2 - vert1;
                nColumnasVert = i + 1;
                break;
            }
        }
        int contador = 0;
        for (int i = 0; i < vertices.Length; i += nColumnasVert)
        {
            try
            {
                vert1 = new Vector3(vertices[i].x * transform.localScale.x, vertices[i].y * transform.localScale.y, vertices[i].z * transform.localScale.z);
                vert2 = new Vector3(vertices[i + nColumnasVert].x * transform.localScale.x, vertices[i + nColumnasVert].y * transform.localScale.y, vertices[i + nColumnasVert].z * transform.localScale.z);
                contador++;
            }
            catch
            {
                nFilasVert = contador + 1;
            }
        }
    }

    Mesh quitarCasillasExtremos(int rango)
    {
        int verticeMedio = (nFilasVert / 2) * nColumnasVert + nColumnasVert / 2;
        for (int i = 0; i < origvertices.Length; ++i)
        {

            if (!(i >= verticeMedio - rango && i <= verticeMedio + rango ||
                (i % nColumnasVert) >= (verticeMedio % nColumnasVert) - rango && (i % nColumnasVert) <= (verticeMedio % nColumnasVert) + rango
                && i >= verticeMedio - nColumnasVert * rango - rango && i <= verticeMedio + nColumnasVert * rango + rango))
            {

                lock (trisWithVertex)
                {
                    for (int j = 0; j < trisWithVertex[i].Count; ++j)
                    {
                        int value = trisWithVertex[i][j];
                        int remainder = value % 3;
                        trianglesDisabled[value - remainder] = true;
                        trianglesDisabled[value - remainder + 1] = true;
                        trianglesDisabled[value - remainder + 2] = true;


                    }
                }

            }
        }
        triangles = origtriangles;
        triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();

        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        for (int i = 0; i < trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;
        return mesh;
    }

    public void eliminarCasillas(int rango)
    {
        filter.mesh = quitarCasillasExtremos(rango);
        meshCollider.sharedMesh = filter.mesh;
    }

    IEnumerator esperarEliminarCasillas()
    {
        Debug.Log("Tiempo antes de quitar nada: " + Time.realtimeSinceStartup);

        yield return new WaitForSecondsRealtime(10f);

        for (int i = ((nColumnasVert - 1) / 2) - 1; i > 0; i--)
        {
            yield return new WaitForSecondsRealtime(5f);
            eliminarCasillas(i);
            GetComponent<generadorSuelo>().instanciasSueloCaen();
        }

    }

    /*

    void calcularTrisWithVertex(int ThreadId, int nThreads)
    {

        lock (trisWithVertex){
            for (int i = 0; i < origvertices.Length; ++i)
            {
                trisWithVertex[i] = origtriangles.IndexOf(i);
            }
            //Debug.Log("Tiempo tras calcularTrisWithVertex: " + Time.realtimeSinceStartup);
        }
        
    }
    */

    void calcularTrisWithVertex(int ThreadId, int nThreads)
    {
        int nIteraciones = (int)Mathf.Ceil((float)origvertices.Length / nThreads);
        int indice = ThreadId * nIteraciones;
        List<int>[] listaAux = new List<int>[nIteraciones];

        for (int i = 0; i < nIteraciones; ++i)
        {
            int indiceActual = indice + i;
            listaAux[i] = origtriangles.IndexOf(indiceActual);
        }
        //Debug.Log("Tiempo tras calcularTrisWithVertex: " + Time.realtimeSinceStartup);

        lock (trisWithVertex)
        {
            //Debug.Log("ThreadId: " + ThreadId);
            for (int i = 0; i < nIteraciones; ++i)
            {
                int indiceActual = indice + i;
                if (indiceActual < origvertices.Length)
                {
                    trisWithVertex[indiceActual] = listaAux[i];
                }
                else
                {
                    break;
                }
            }
        }

    }
}