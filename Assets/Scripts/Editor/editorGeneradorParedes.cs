using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(generadorParedes))]
public class editorGeneradorParedes : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        generadorParedes _generadorParedes = (generadorParedes)target;

        _generadorParedes.Posicion_inicial = EditorGUILayout.ObjectField("Posicion inicial:",_generadorParedes.Posicion_inicial,typeof(Transform),true) as Transform;
        _generadorParedes.PrefabTrozosPared = EditorGUILayout.ObjectField("Prefab trozo pared:", _generadorParedes.PrefabTrozosPared, typeof(GameObject), true) as GameObject;

        _generadorParedes.TrozosVer = EditorGUILayout.IntSlider("Columnas:",_generadorParedes.TrozosVer,1, 30);
        _generadorParedes.TrozosHor = EditorGUILayout.IntSlider("Filas:", _generadorParedes.TrozosHor, 1, 30);
        _generadorParedes.TamTrozos = EditorGUILayout.Slider("Tamaño trozos:", _generadorParedes.TamTrozos, 1f, 20f);
        _generadorParedes.Espaciado = EditorGUILayout.Slider("Espaciado:", _generadorParedes.Espaciado, 0f, 5f);


        if (!Application.isPlaying)
        {
            _generadorParedes.generarParedes();
        }
    }

}
