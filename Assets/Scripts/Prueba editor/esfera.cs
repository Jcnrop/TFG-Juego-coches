using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

public class esfera : MonoBehaviour
{

    [SerializeField] bool booleano;
    public bool booleano2;
    //string nombre;

    //public bool Booleano { get => booleano; set => booleano = value; }

    #region Editor

#if UNITY_EDITOR

    [CustomEditor(typeof(esfera))]
    public class esferaEditor : Editor
    {
        SerializedProperty booleano;

        private void OnEnable()
        {
            booleano = serializedObject.FindProperty("booleano");
        }
        public override void OnInspectorGUI()
        {
            esfera _esfera = (esfera)target;

            serializedObject.Update();

            EditorGUILayout.PropertyField(booleano);

            if (_esfera.booleano)
            {
                _esfera.transform.localScale = new Vector3(1, 5, 5);
            }
            else
            {
                _esfera.transform.localScale = new Vector3(5, 5, 5);
            }

            serializedObject.ApplyModifiedProperties();
            /*serializedObject.Update();
            
            base.OnInspectorGUI();
            esfera myScript = (esfera)target;

            //myScript.booleano = EditorGUILayout.Toggle("¿Es un bot?",myScript.booleano);
            EditorGUILayout.PropertyField(booleano);
            myScript.nombre = EditorGUILayout.TextField("Nombre", myScript.nombre);

            if (myScript.booleano)
            {
                myScript.transform.localScale = new Vector3(1,5,5);
            }
            else
            {
                myScript.transform.localScale = new Vector3(5, 5, 5);
            }
            serializedObject.ApplyModifiedProperties();*/
        }

    }
#endif
    #endregion

}
